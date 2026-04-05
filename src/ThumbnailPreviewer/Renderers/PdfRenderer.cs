using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using PDFtoImage;
using ThumbnailPreviewer.Infrastructure;

namespace ThumbnailPreviewer.Renderers
{
    /// <summary>
    /// Renders PDF first page to a thumbnail bitmap using PDFium (no Ghostscript needed).
    /// </summary>
    internal static class PdfRenderer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        private static bool _nativeLoaded;
        private static bool _nativeAvailable;
        private static readonly object InitLock = new object();

        /// <summary>
        /// Pre-loads pdfium.dll and libSkiaSharp.dll from the x64 subfolder next to our assembly.
        /// Required because dllhost.exe (COM surrogate) has its working directory at System32.
        /// </summary>
        private static void EnsureNativeLibraries()
        {
            if (_nativeLoaded) return;
            lock (InitLock)
            {
                if (_nativeLoaded) return;
                _nativeLoaded = true;

                try
                {
                    var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    if (string.IsNullOrEmpty(assemblyDir))
                    {
                        ThumbnailLogger.Error("PdfRenderer", "Cannot determine assembly directory");
                        return;
                    }

                    // Native DLLs are in the x64 subfolder
                    var nativeDir = Path.Combine(assemblyDir, "x64");
                    if (!Directory.Exists(nativeDir))
                    {
                        // Fall back to assembly directory itself
                        nativeDir = assemblyDir;
                    }

                    // Add native directory to DLL search path
                    SetDllDirectory(nativeDir);

                    // Explicitly load the native libraries
                    var pdfiumPath = Path.Combine(nativeDir, "pdfium.dll");
                    var skiaPath = Path.Combine(nativeDir, "libSkiaSharp.dll");

                    if (File.Exists(pdfiumPath))
                    {
                        var handle = LoadLibrary(pdfiumPath);
                        if (handle != IntPtr.Zero)
                            ThumbnailLogger.Debug("PdfRenderer", $"Loaded pdfium.dll from {pdfiumPath}");
                        else
                            ThumbnailLogger.Error("PdfRenderer", $"Failed to load pdfium.dll from {pdfiumPath}, error={Marshal.GetLastWin32Error()}");
                    }
                    else
                    {
                        ThumbnailLogger.Error("PdfRenderer", $"pdfium.dll not found at {pdfiumPath}");
                    }

                    if (File.Exists(skiaPath))
                    {
                        var handle = LoadLibrary(skiaPath);
                        if (handle != IntPtr.Zero)
                            ThumbnailLogger.Debug("PdfRenderer", $"Loaded libSkiaSharp.dll from {skiaPath}");
                        else
                            ThumbnailLogger.Error("PdfRenderer", $"Failed to load libSkiaSharp.dll, error={Marshal.GetLastWin32Error()}");
                    }

                    _nativeAvailable = true;
                }
                catch (Exception ex)
                {
                    ThumbnailLogger.Error("PdfRenderer", "Failed to load native libraries", ex);
                    _nativeAvailable = false;
                }
            }
        }

        public static Bitmap Render(Stream stream, uint maxWidth)
        {
            EnsureNativeLibraries();

            if (!_nativeAvailable)
            {
                ThumbnailLogger.Error("PdfRenderer", "Native libraries not available, cannot render PDF");
                return null;
            }

            try
            {
                // PDFtoImage needs bytes, not a stream (stream may not be seekable from COM)
                byte[] pdfBytes;
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                    pdfBytes = new byte[stream.Length];
                    stream.Read(pdfBytes, 0, pdfBytes.Length);
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        pdfBytes = ms.ToArray();
                    }
                }

                var options = new RenderOptions
                {
                    Width = (int)maxWidth,
                    WithAspectRatio = true,
                    WithAnnotations = true,
                    WithFormFill = true,
                    AntiAliasing = PdfAntiAliasing.Text | PdfAntiAliasing.Images | PdfAntiAliasing.Paths,
                    BackgroundColor = new SkiaSharp.SKColor(255, 255, 255)
                };

                // Render first page to PNG bytes
                using (var pngStream = new MemoryStream())
                {
                    Conversion.SavePng(pngStream, pdfBytes, 0, password: null, options: options);
                    pngStream.Position = 0;

                    using (var tempBmp = new Bitmap(pngStream))
                    {
                        // Return a copy not tied to the stream
                        return new Bitmap(tempBmp);
                    }
                }
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error("PdfRenderer", "Failed to render PDF", ex);
                return null;
            }
        }
    }
}
