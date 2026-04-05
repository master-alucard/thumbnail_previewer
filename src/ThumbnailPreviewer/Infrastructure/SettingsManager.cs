using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace ThumbnailPreviewer.Infrastructure
{
    /// <summary>
    /// Reads per-extension settings from HKCU\Software\ThumbnailPreviewer.
    /// Used by handlers to check if preview/badge is enabled.
    /// Thread-safe with 5-second cache to avoid registry hammering.
    /// </summary>
    public static class SettingsManager
    {
        private const string RegistryRoot = @"Software\ThumbnailPreviewer";
        private const string PreviewSubKey = "Preview";
        private const string BadgeSubKey = "Badge";
        private const int CacheTtlMs = 5000;

        private static readonly object Lock = new object();
        private static Dictionary<string, bool> _previewCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, bool> _badgeCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static long _lastRefreshTicks;

        /// <summary>
        /// Returns true if thumbnail preview is enabled for this extension.
        /// Default: true (enabled) if no registry key exists.
        /// </summary>
        public static bool IsPreviewEnabled(string extension)
        {
            return GetCachedValue(PreviewSubKey, ref _previewCache, extension);
        }

        /// <summary>
        /// Returns true if badge overlay is enabled for this extension.
        /// Default: true (enabled) if no registry key exists.
        /// </summary>
        public static bool IsBadgeEnabled(string extension)
        {
            return GetCachedValue(BadgeSubKey, ref _badgeCache, extension);
        }

        /// <summary>
        /// All supported extensions with their display info.
        /// Used by the Settings app to populate the grid.
        /// </summary>
        public static readonly ExtensionInfo[] AllExtensions = new[]
        {
            new ExtensionInfo("pdf",  "PDF",  "PDF Document"),
            new ExtensionInfo("docx", "DOCX", "Word Document"),
            new ExtensionInfo("csv",  "CSV",  "Comma-Separated Values"),
            new ExtensionInfo("tsv",  "CSV",  "Tab-Separated Values"),
            new ExtensionInfo("eps",  "EPS",  "Encapsulated PostScript"),
            new ExtensionInfo("ai",   "EPS",  "Adobe Illustrator"),
            new ExtensionInfo("ps",   "EPS",  "PostScript"),
            new ExtensionInfo("odt",  "ODF",  "OpenDocument Text"),
            new ExtensionInfo("ods",  "ODF",  "OpenDocument Spreadsheet"),
            new ExtensionInfo("odp",  "ODF",  "OpenDocument Presentation"),
            new ExtensionInfo("odg",  "ODF",  "OpenDocument Drawing"),
            new ExtensionInfo("svg",  "SVG",  "Scalable Vector Graphics"),
            new ExtensionInfo("svgz", "SVG",  "Compressed SVG"),
            new ExtensionInfo("psd",  "PSD",  "Photoshop Document"),
            new ExtensionInfo("dng",  "RAW",  "Digital Negative"),
            new ExtensionInfo("cr2",  "RAW",  "Canon RAW v2"),
            new ExtensionInfo("cr3",  "RAW",  "Canon RAW v3"),
            new ExtensionInfo("nef",  "RAW",  "Nikon RAW"),
            new ExtensionInfo("arw",  "RAW",  "Sony RAW"),
            new ExtensionInfo("orf",  "RAW",  "Olympus RAW"),
            new ExtensionInfo("rw2",  "RAW",  "Panasonic RAW"),
            new ExtensionInfo("raf",  "RAW",  "Fujifilm RAW"),
            new ExtensionInfo("srw",  "RAW",  "Samsung RAW"),
            new ExtensionInfo("pef",  "RAW",  "Pentax RAW"),
        };

        private static bool GetCachedValue(string subKey, ref Dictionary<string, bool> cache, string extension)
        {
            extension = Normalize(extension);

            lock (Lock)
            {
                RefreshCacheIfStale();

                if (cache.TryGetValue(extension, out var value))
                    return value;
            }

            // Not in cache = not in registry = default enabled
            return true;
        }

        private static void RefreshCacheIfStale()
        {
            var now = Environment.TickCount;
            if (Math.Abs(now - _lastRefreshTicks) < CacheTtlMs && _previewCache.Count > 0)
                return;

            _lastRefreshTicks = now;
            _previewCache = ReadAllValues(PreviewSubKey);
            _badgeCache = ReadAllValues(BadgeSubKey);
        }

        private static Dictionary<string, bool> ReadAllValues(string subKeyName)
        {
            var result = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey($@"{RegistryRoot}\{subKeyName}"))
                {
                    if (key == null) return result;

                    foreach (var name in key.GetValueNames())
                    {
                        var val = key.GetValue(name);
                        if (val is int intVal)
                            result[name] = intVal != 0;
                    }
                }
            }
            catch
            {
                // Registry access may fail in some COM hosting scenarios
            }
            return result;
        }

        /// <summary>
        /// Writes a setting value. Used by the Settings app.
        /// </summary>
        public static void SetPreviewEnabled(string extension, bool enabled)
        {
            WriteValue(PreviewSubKey, Normalize(extension), enabled);
        }

        public static void SetBadgeEnabled(string extension, bool enabled)
        {
            WriteValue(BadgeSubKey, Normalize(extension), enabled);
        }

        private static void WriteValue(string subKeyName, string extension, bool enabled)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey($@"{RegistryRoot}\{subKeyName}"))
                {
                    key?.SetValue(extension, enabled ? 1 : 0, RegistryValueKind.DWord);
                }

                // Invalidate cache
                lock (Lock) { _lastRefreshTicks = 0; }
            }
            catch
            {
                // Silently fail
            }
        }

        /// <summary>
        /// Deletes all user settings, reverting to defaults (all enabled).
        /// </summary>
        public static void ResetToDefaults()
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(RegistryRoot, throwOnMissingSubKey: false);
                lock (Lock)
                {
                    _previewCache.Clear();
                    _badgeCache.Clear();
                    _lastRefreshTicks = 0;
                }
            }
            catch { }
        }

        private static string Normalize(string ext)
        {
            return ext?.TrimStart('.').ToLowerInvariant() ?? "";
        }
    }

    public class ExtensionInfo
    {
        public string Extension { get; }
        public string BadgeLabel { get; }
        public string Description { get; }

        public ExtensionInfo(string extension, string badgeLabel, string description)
        {
            Extension = extension;
            BadgeLabel = badgeLabel;
            Description = description;
        }
    }
}
