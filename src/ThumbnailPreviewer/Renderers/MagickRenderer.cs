using System;
using System.Drawing;
using System.IO;
using ImageMagick;
using ThumbnailPreviewer.Infrastructure;

namespace ThumbnailPreviewer.Renderers
{
    internal static class MagickRenderer
    {
        private static bool _initialized;
        private static readonly object InitLock = new object();

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            lock (InitLock)
            {
                if (_initialized) return;

                // Limit resources to avoid excessive memory usage in shell extension
                ResourceLimits.Memory = 256 * 1024 * 1024; // 256 MB
                ResourceLimits.Thread = 1;
                ResourceLimits.Throttle = 1;

                _initialized = true;
            }
        }

        /// <summary>
        /// Renders PSD and SVG files to thumbnail bitmaps.
        /// </summary>
        public static Bitmap RenderImage(Stream stream, uint maxWidth)
        {
            EnsureInitialized();

            try
            {
                var settings = new MagickReadSettings
                {
                    Density = new Density(150),
                    BackgroundColor = MagickColors.Transparent,
                    FrameIndex = 0,
                    FrameCount = 1
                };

                using (var image = new MagickImage(stream, settings))
                {
                    image.AutoOrient();

                    var geometry = new MagickGeometry(maxWidth, maxWidth)
                    {
                        IgnoreAspectRatio = false,
                        Greater = true
                    };
                    image.Thumbnail(geometry);

                    return ToBitmap(image);
                }
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error("MagickRenderer", "Failed to render image", ex);
                return null;
            }
        }

        /// <summary>
        /// Renders RAW/DNG files with fast-path thumbnail extraction.
        /// </summary>
        public static Bitmap RenderRaw(Stream stream, uint maxWidth)
        {
            EnsureInitialized();

            try
            {
                var thumbnail = TryExtractRawThumbnail(stream, maxWidth);
                if (thumbnail != null) return thumbnail;

                if (stream.CanSeek)
                    stream.Position = 0;

                var settings = new MagickReadSettings
                {
                    Density = new Density(72)
                };

                using (var image = new MagickImage(stream, settings))
                {
                    image.AutoOrient();

                    var geometry = new MagickGeometry(maxWidth, maxWidth)
                    {
                        IgnoreAspectRatio = false,
                        Greater = true
                    };
                    image.Thumbnail(geometry);

                    return ToBitmap(image);
                }
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error("MagickRenderer", "Failed to render RAW file", ex);
                return null;
            }
        }

        private static Bitmap TryExtractRawThumbnail(Stream stream, uint maxWidth)
        {
            try
            {
                if (stream.CanSeek)
                    stream.Position = 0;

                using (var image = new MagickImage(stream))
                {
                    var profile = image.GetProfile("dng:thumbnail")
                               ?? image.GetProfile("exif:thumbnail");

                    if (profile != null)
                    {
                        var thumbData = profile.ToByteArray();
                        if (thumbData != null && thumbData.Length > 0)
                        {
                            using (var thumbStream = new MemoryStream(thumbData))
                            using (var thumbImage = new MagickImage(thumbStream))
                            {
                                thumbImage.AutoOrient();

                                var geometry = new MagickGeometry(maxWidth, maxWidth)
                                {
                                    IgnoreAspectRatio = false,
                                    Greater = true
                                };
                                thumbImage.Thumbnail(geometry);

                                return ToBitmap(thumbImage);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Debug("MagickRenderer", $"Fast-path thumbnail extraction failed: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Converts MagickImage to System.Drawing.Bitmap via BMP byte array.
        /// </summary>
        private static Bitmap ToBitmap(MagickImage image)
        {
            var bytes = image.ToByteArray(MagickFormat.Bmp);
            using (var ms = new MemoryStream(bytes))
            {
                // Create a new Bitmap that owns its data (not tied to the stream)
                var bmp = new Bitmap(ms);
                return new Bitmap(bmp);
            }
        }
    }
}
