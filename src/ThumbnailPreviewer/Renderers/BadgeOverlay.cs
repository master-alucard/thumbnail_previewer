using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ThumbnailPreviewer.Renderers
{
    /// <summary>
    /// Draws a small file-extension badge (e.g. "PDF", "SVG") in the
    /// bottom-right corner of a thumbnail bitmap.
    /// </summary>
    internal static class BadgeOverlay
    {
        /// <summary>
        /// Draws a rounded-pill badge with the extension label onto the bitmap.
        /// Mutates and returns the same bitmap.
        /// </summary>
        public static Bitmap Apply(Bitmap bitmap, string label)
        {
            if (bitmap == null || string.IsNullOrWhiteSpace(label))
                return bitmap;

            label = label.TrimStart('.').ToUpperInvariant();

            int w = bitmap.Width;
            int h = bitmap.Height;

            // Scale font to ~7% of thumbnail width, clamped
            float fontSize = Math.Max(6f, Math.Min(w * 0.07f, 14f));
            int margin = Math.Max(2, (int)(w * 0.03f));
            int paddingX = Math.Max(3, (int)(fontSize * 0.45f));
            int paddingY = Math.Max(1, (int)(fontSize * 0.15f));
            int cornerRadius = Math.Max(2, (int)(fontSize * 0.35f));

            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                using (var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    var textSize = g.MeasureString(label, font);
                    int badgeW = (int)textSize.Width + paddingX * 2;
                    int badgeH = (int)textSize.Height + paddingY * 2;

                    int x = w - badgeW - margin;
                    int y = h - badgeH - margin;

                    // Clamp to bitmap bounds
                    if (x < 0) x = 0;
                    if (y < 0) y = 0;

                    var badgeRect = new Rectangle(x, y, badgeW, badgeH);

                    // Draw rounded-rectangle background
                    using (var bgBrush = new SolidBrush(Color.FromArgb(180, 30, 30, 30)))
                    using (var path = RoundedRect(badgeRect, cornerRadius))
                    {
                        g.FillPath(bgBrush, path);
                    }

                    // Draw text
                    using (var textBrush = new SolidBrush(Color.FromArgb(240, 255, 255, 255)))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(label, font, textBrush,
                            new RectangleF(x, y, badgeW, badgeH), sf);
                    }
                }
            }

            return bitmap;
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();

            if (d <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // Top-left arc
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            // Top-right arc
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            // Bottom-right arc
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            // Bottom-left arc
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
