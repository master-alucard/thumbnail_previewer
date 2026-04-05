using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;

namespace ThumbnailPreviewer.Renderers
{
    internal static class ZipThumbnailExtractor
    {
        /// <summary>
        /// Extracts embedded thumbnail from OpenDocument files (ODT, ODS, ODP).
        /// These files always contain Thumbnails/thumbnail.png.
        /// </summary>
        public static Bitmap ExtractOpenDocumentThumbnail(Stream stream, uint maxWidth)
        {
            return ExtractFromZip(stream, "Thumbnails/thumbnail.png", maxWidth);
        }

        /// <summary>
        /// Extracts embedded thumbnail from DOCX files.
        /// DOCX may contain docProps/thumbnail.jpeg (or .wmf, .emf, .png).
        /// Not all DOCX files have this - returns null if absent.
        /// </summary>
        public static Bitmap ExtractDocxThumbnail(Stream stream, uint maxWidth)
        {
            // Try common thumbnail paths in DOCX
            var paths = new[]
            {
                "docProps/thumbnail.jpeg",
                "docProps/thumbnail.jpg",
                "docProps/thumbnail.png",
                "docProps/thumbnail.wmf",
                "docProps/thumbnail.emf"
            };

            foreach (var path in paths)
            {
                var result = ExtractFromZip(stream, path, maxWidth);
                if (result != null) return result;

                // Reset stream position for next attempt
                if (stream.CanSeek)
                    stream.Position = 0;
            }

            return null;
        }

        private static Bitmap ExtractFromZip(Stream stream, string entryPath, uint maxWidth)
        {
            try
            {
                if (stream.CanSeek)
                    stream.Position = 0;

                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true))
                {
                    var entry = archive.GetEntry(entryPath);
                    if (entry == null) return null;

                    using (var entryStream = entry.Open())
                    using (var ms = new MemoryStream())
                    {
                        entryStream.CopyTo(ms);
                        ms.Position = 0;

                        using (var original = new Bitmap(ms))
                        {
                            return ResizeBitmap(original, maxWidth);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static Bitmap ResizeBitmap(Bitmap source, uint maxWidth)
        {
            if (source.Width <= maxWidth && source.Height <= maxWidth)
            {
                return new Bitmap(source);
            }

            float ratio = Math.Min((float)maxWidth / source.Width, (float)maxWidth / source.Height);
            int newWidth = (int)(source.Width * ratio);
            int newHeight = (int)(source.Height * ratio);

            if (newWidth < 1) newWidth = 1;
            if (newHeight < 1) newHeight = 1;

            var result = new Bitmap(newWidth, newHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(source, 0, 0, newWidth, newHeight);
            }

            return result;
        }

    }
}
