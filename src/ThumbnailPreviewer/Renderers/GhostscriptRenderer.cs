using System;
using System.Drawing;
using System.IO;
using ImageMagick;
using ThumbnailPreviewer.Infrastructure;

namespace ThumbnailPreviewer.Renderers
{
    internal static class GhostscriptRenderer
    {
        private static bool _gsConfigured;
        private static bool _gsAvailable;
        private static readonly object InitLock = new object();

        private static void EnsureConfigured()
        {
            if (_gsConfigured) return;
            lock (InitLock)
            {
                if (_gsConfigured) return;

                _gsAvailable = GhostscriptDetector.IsAvailable;
                if (_gsAvailable)
                {
                    try
                    {
                        MagickNET.SetGhostscriptDirectory(GhostscriptDetector.GhostscriptDirectory);
                        ThumbnailLogger.Info("GhostscriptRenderer", $"Ghostscript configured: {GhostscriptDetector.GhostscriptDirectory}");
                    }
                    catch (Exception ex)
                    {
                        ThumbnailLogger.Error("GhostscriptRenderer", "Failed to configure Ghostscript directory", ex);
                        _gsAvailable = false;
                    }
                }

                ResourceLimits.Memory = 256 * 1024 * 1024;
                ResourceLimits.Thread = 1;

                _gsConfigured = true;
            }
        }

        /// <summary>
        /// Renders EPS, AI, or PDF file to a thumbnail bitmap.
        /// Returns null if Ghostscript is not installed.
        /// </summary>
        public static Bitmap Render(Stream stream, uint maxWidth)
        {
            EnsureConfigured();

            if (!_gsAvailable)
            {
                ThumbnailLogger.Debug("GhostscriptRenderer", "Skipping render - Ghostscript not available");
                return null;
            }

            try
            {
                var settings = new MagickReadSettings
                {
                    Density = new Density(150),
                    BackgroundColor = MagickColors.White,
                    FrameIndex = 0,
                    FrameCount = 1
                };

                using (var image = new MagickImage(stream, settings))
                {
                    image.BackgroundColor = MagickColors.White;
                    image.Alpha(AlphaOption.Remove);

                    var geometry = new MagickGeometry(maxWidth, maxWidth)
                    {
                        IgnoreAspectRatio = false,
                        Greater = true
                    };
                    image.Thumbnail(geometry);

                    // Convert via BMP byte array
                    var bytes = image.ToByteArray(MagickFormat.Bmp);
                    using (var ms = new MemoryStream(bytes))
                    {
                        var bmp = new Bitmap(ms);
                        return new Bitmap(bmp);
                    }
                }
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error("GhostscriptRenderer", "Failed to render EPS/AI/PDF", ex);
                return null;
            }
        }
    }
}
