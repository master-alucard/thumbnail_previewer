using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using ThumbnailPreviewer.Infrastructure;

namespace ThumbnailPreviewer.Renderers
{
    /// <summary>
    /// Renders DOCX files by extracting text from the document XML and drawing it.
    /// First tries to extract an embedded thumbnail; falls back to text rendering.
    /// </summary>
    internal static class DocxRenderer
    {
        // Page appearance
        private static readonly Color PageColor = Color.White;
        private static readonly Color TextColor = Color.FromArgb(30, 30, 30);
        private static readonly Color HeadingColor = Color.FromArgb(15, 15, 80);
        private static readonly Color BorderColor = Color.FromArgb(200, 200, 200);

        public static Bitmap Render(Stream stream, uint maxWidth)
        {
            try
            {
                if (stream.CanSeek)
                    stream.Position = 0;

                // Read entire stream into memory (ZipArchive needs seekable stream)
                byte[] docBytes;
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    docBytes = ms.ToArray();
                }

                // Try 1: Extract embedded thumbnail
                var thumbnail = TryExtractEmbeddedThumbnail(docBytes, maxWidth);
                if (thumbnail != null) return thumbnail;

                // Try 2: Render text content from document.xml
                var paragraphs = ExtractParagraphs(docBytes);
                if (paragraphs.Count > 0)
                    return RenderTextPage(paragraphs, maxWidth);

                return null;
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error("DocxRenderer", "Failed to render DOCX", ex);
                return null;
            }
        }

        private static Bitmap TryExtractEmbeddedThumbnail(byte[] docBytes, uint maxWidth)
        {
            try
            {
                using (var ms = new MemoryStream(docBytes))
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
                {
                    var paths = new[]
                    {
                        "docProps/thumbnail.jpeg",
                        "docProps/thumbnail.jpg",
                        "docProps/thumbnail.png",
                    };

                    foreach (var path in paths)
                    {
                        var entry = archive.GetEntry(path);
                        if (entry == null) continue;

                        using (var entryStream = entry.Open())
                        using (var imgMs = new MemoryStream())
                        {
                            entryStream.CopyTo(imgMs);
                            imgMs.Position = 0;
                            using (var bmp = new Bitmap(imgMs))
                            {
                                return new Bitmap(bmp);
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        private static List<DocParagraph> ExtractParagraphs(byte[] docBytes)
        {
            var paragraphs = new List<DocParagraph>();
            try
            {
                using (var ms = new MemoryStream(docBytes))
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Read))
                {
                    var docEntry = archive.GetEntry("word/document.xml");
                    if (docEntry == null) return paragraphs;

                    using (var entryStream = docEntry.Open())
                    {
                        var doc = new XmlDocument();
                        doc.Load(entryStream);

                        var nsMgr = new XmlNamespaceManager(doc.NameTable);
                        nsMgr.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

                        var pNodes = doc.SelectNodes("//w:p", nsMgr);
                        if (pNodes == null) return paragraphs;

                        foreach (XmlNode pNode in pNodes)
                        {
                            if (paragraphs.Count >= 40) break;

                            var sb = new StringBuilder();
                            var runs = pNode.SelectNodes(".//w:r/w:t", nsMgr);
                            if (runs != null)
                            {
                                foreach (XmlNode t in runs)
                                    sb.Append(t.InnerText);
                            }

                            var text = sb.ToString().Trim();
                            if (text.Length == 0)
                            {
                                // Empty paragraph = spacing
                                paragraphs.Add(new DocParagraph { Text = "", IsHeading = false });
                                continue;
                            }

                            // Check if this is a heading
                            bool isHeading = false;
                            var pStyleNode = pNode.SelectSingleNode(".//w:pPr/w:pStyle/@w:val", nsMgr);
                            if (pStyleNode != null)
                            {
                                var style = pStyleNode.Value;
                                isHeading = style.StartsWith("Heading", StringComparison.OrdinalIgnoreCase)
                                         || style.StartsWith("Title", StringComparison.OrdinalIgnoreCase);
                            }

                            paragraphs.Add(new DocParagraph { Text = text, IsHeading = isHeading });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Debug("DocxRenderer", $"Failed to extract text: {ex.Message}");
            }

            return paragraphs;
        }

        private static Bitmap RenderTextPage(List<DocParagraph> paragraphs, uint maxWidth)
        {
            int width = (int)maxWidth;
            int height = (int)(maxWidth * 1.3f); // ~A4 aspect ratio
            int margin = Math.Max(8, width / 16);

            var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                // White page background
                g.Clear(PageColor);

                // Page border
                using (var borderPen = new Pen(BorderColor, 1))
                    g.DrawRectangle(borderPen, 0, 0, width - 1, height - 1);

                float fontSize = Math.Max(5f, width / 30f);
                float headingSize = fontSize * 1.4f;

                using (var bodyFont = new Font("Segoe UI", fontSize, FontStyle.Regular))
                using (var headingFont = new Font("Segoe UI", headingSize, FontStyle.Bold))
                using (var textBrush = new SolidBrush(TextColor))
                using (var headingBrush = new SolidBrush(HeadingColor))
                {
                    float y = margin;
                    float textWidth = width - margin * 2;

                    var sf = new StringFormat
                    {
                        Trimming = StringTrimming.EllipsisWord,
                        FormatFlags = StringFormatFlags.LineLimit
                    };

                    foreach (var para in paragraphs)
                    {
                        if (y >= height - margin) break;

                        if (para.Text.Length == 0)
                        {
                            y += fontSize * 0.5f;
                            continue;
                        }

                        var font = para.IsHeading ? headingFont : bodyFont;
                        var brush = para.IsHeading ? headingBrush : textBrush;

                        var size = g.MeasureString(para.Text, font, (int)textWidth, sf);
                        float lineHeight = Math.Min(size.Height, height - y - margin);

                        if (lineHeight <= 0) break;

                        g.DrawString(para.Text, font, brush,
                            new RectangleF(margin, y, textWidth, lineHeight), sf);

                        y += lineHeight + fontSize * 0.2f;

                        if (para.IsHeading)
                            y += fontSize * 0.3f;
                    }
                }
            }

            return bmp;
        }

        private class DocParagraph
        {
            public string Text { get; set; }
            public bool IsHeading { get; set; }
        }
    }
}
