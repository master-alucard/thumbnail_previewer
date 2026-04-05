using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace ThumbnailPreviewer.Infrastructure
{
    internal static class GhostscriptDetector
    {
        private static bool _checked;
        private static string _gsDirectory;

        public static bool IsAvailable
        {
            get
            {
                EnsureChecked();
                return _gsDirectory != null;
            }
        }

        public static string GhostscriptDirectory
        {
            get
            {
                EnsureChecked();
                return _gsDirectory;
            }
        }

        private static void EnsureChecked()
        {
            if (_checked) return;
            _checked = true;

            // Priority: bundled GS first, then system-installed GS
            _gsDirectory = TryDetectBundled()
                        ?? TryDetectFromRegistry("SOFTWARE\\GPL Ghostscript")
                        ?? TryDetectFromRegistry("SOFTWARE\\AFPL Ghostscript")
                        ?? TryDetectFromPath()
                        ?? TryDetectFromCommonPaths();

            if (_gsDirectory != null)
            {
                ThumbnailLogger.Info("GhostscriptDetector", $"Found Ghostscript at: {_gsDirectory}");
            }
            else
            {
                ThumbnailLogger.Info("GhostscriptDetector", "Ghostscript not found. EPS/AI thumbnails will be unavailable.");
            }
        }

        /// <summary>
        /// Checks for gsdll64.dll bundled alongside our DLL in a "ghostscript" subfolder.
        /// Layout: {app}\ghostscript\gsdll64.dll
        /// </summary>
        private static string TryDetectBundled()
        {
            try
            {
                var dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(dllDir)) return null;

                var bundledDir = Path.Combine(dllDir, "ghostscript");
                var bundledDll = Path.Combine(bundledDir, "gsdll64.dll");

                if (File.Exists(bundledDll))
                {
                    ThumbnailLogger.Info("GhostscriptDetector", $"Using bundled Ghostscript: {bundledDir}");
                    return bundledDir;
                }
            }
            catch
            {
                // Assembly location access may fail in some COM hosting scenarios
            }

            return null;
        }

        private static string TryDetectFromRegistry(string subKeyPath)
        {
            try
            {
                foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                {
                    using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    using (var gsKey = baseKey.OpenSubKey(subKeyPath))
                    {
                        if (gsKey == null) continue;

                        string bestVersion = null;
                        Version bestParsed = null;

                        foreach (var versionName in gsKey.GetSubKeyNames())
                        {
                            if (Version.TryParse(versionName, out var parsed))
                            {
                                if (bestParsed == null || parsed > bestParsed)
                                {
                                    bestParsed = parsed;
                                    bestVersion = versionName;
                                }
                            }
                        }

                        if (bestVersion == null) continue;

                        using (var versionKey = gsKey.OpenSubKey(bestVersion))
                        {
                            var gsDll = versionKey?.GetValue("GS_DLL") as string;
                            if (!string.IsNullOrEmpty(gsDll) && File.Exists(gsDll))
                            {
                                return Path.GetDirectoryName(gsDll);
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        private static string TryDetectFromPath()
        {
            try
            {
                var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
                foreach (var dir in pathEnv.Split(';'))
                {
                    if (string.IsNullOrWhiteSpace(dir)) continue;

                    var gs64 = Path.Combine(dir.Trim(), "gswin64c.exe");
                    if (File.Exists(gs64)) return dir.Trim();

                    var gs32 = Path.Combine(dir.Trim(), "gswin32c.exe");
                    if (File.Exists(gs32)) return dir.Trim();
                }
            }
            catch { }

            return null;
        }

        private static string TryDetectFromCommonPaths()
        {
            var programFiles = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            foreach (var pf in programFiles)
            {
                if (string.IsNullOrEmpty(pf)) continue;

                var gsDir = Path.Combine(pf, "gs");
                if (!Directory.Exists(gsDir)) continue;

                try
                {
                    foreach (var versionDir in Directory.GetDirectories(gsDir, "gs*"))
                    {
                        var binDir = Path.Combine(versionDir, "bin");
                        if (Directory.Exists(binDir))
                        {
                            var gs64 = Path.Combine(binDir, "gswin64c.exe");
                            var gs32 = Path.Combine(binDir, "gswin32c.exe");
                            if (File.Exists(gs64) || File.Exists(gs32))
                                return binDir;
                        }
                    }
                }
                catch { }
            }

            return null;
        }
    }
}
