using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using ThumbnailPreviewer.Infrastructure;

namespace ThumbnailPreviewer.Renderers
{
    internal static class CsvRenderer
    {
        private const int MaxRows = 20;
        private const int MaxColumns = 10;
        private const int MaxCellChars = 15;
        private const int CellPadding = 4;

        // Colors
        private static readonly Color BackgroundColor = Color.White;
        private static readonly Color HeaderBgColor = Color.FromArgb(240, 240, 245);
        private static readonly Color AltRowColor = Color.FromArgb(248, 248, 252);
        private static readonly Color GridColor = Color.FromArgb(200, 200, 210);
        private static readonly Color TextColor = Color.FromArgb(40, 40, 50);
        private static readonly Color HeaderTextColor = Color.FromArgb(20, 20, 30);

        public static Bitmap Render(Stream stream, uint maxWidth)
        {
            try
            {
                if (stream.CanSeek)
                    stream.Position = 0;

                // Read lines from stream
                var lines = ReadLines(stream, MaxRows + 1);
                if (lines.Count == 0) return null;

                // Detect delimiter
                var delimiter = DetectDelimiter(lines[0]);

                // Parse into cells
                var rows = lines
                    .Select(line => SplitLine(line, delimiter))
                    .ToList();

                // Limit columns
                int colCount = Math.Min(rows.Max(r => r.Length), MaxColumns);
                if (colCount == 0) return null;

                return RenderTable(rows, colCount, (int)maxWidth);
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error("CsvRenderer", "Failed to render CSV", ex);
                return null;
            }
        }

        private static List<string> ReadLines(Stream stream, int maxLines)
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 8192, leaveOpen: true))
            {
                string line;
                int bytesRead = 0;
                while ((line = reader.ReadLine()) != null && lines.Count < maxLines && bytesRead < 16384)
                {
                    lines.Add(line);
                    bytesRead += line.Length;
                }
            }
            return lines;
        }

        private static char DetectDelimiter(string firstLine)
        {
            var candidates = new[] { ',', ';', '\t', '|' };
            char best = ',';
            int bestCount = 0;

            foreach (var c in candidates)
            {
                int count = firstLine.Count(ch => ch == c);
                if (count > bestCount)
                {
                    bestCount = count;
                    best = c;
                }
            }

            return best;
        }

        private static string[] SplitLine(string line, char delimiter)
        {
            // Simple split (doesn't handle quoted fields with embedded delimiters)
            return line.Split(delimiter);
        }

        private static Bitmap RenderTable(List<string[]> rows, int colCount, int maxWidth)
        {
            // Calculate font size based on thumbnail size
            float fontSize = Math.Max(6f, maxWidth / 40f);
            using (var font = new Font("Consolas", fontSize, FontStyle.Regular))
            using (var headerFont = new Font("Consolas", fontSize, FontStyle.Bold))
            {
                // Measure cell dimensions
                using (var tempBmp = new Bitmap(1, 1))
                using (var tempG = Graphics.FromImage(tempBmp))
                {
                    var charSize = tempG.MeasureString("W", font);
                    int cellWidth = (int)(charSize.Width * MaxCellChars * 0.6f) + CellPadding * 2;
                    int cellHeight = (int)(charSize.Height) + CellPadding * 2;

                    // Fit to maxWidth
                    int tableWidth = cellWidth * colCount + 1;
                    if (tableWidth > maxWidth)
                    {
                        cellWidth = ((int)maxWidth - 1) / colCount;
                        tableWidth = cellWidth * colCount + 1;
                    }

                    int rowCount = Math.Min(rows.Count, MaxRows);
                    int tableHeight = cellHeight * rowCount + 1;

                    // Cap height
                    if (tableHeight > maxWidth)
                    {
                        rowCount = (int)maxWidth / cellHeight;
                        tableHeight = cellHeight * rowCount + 1;
                    }

                    var bmp = new Bitmap(tableWidth, tableHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                        // Background
                        g.Clear(BackgroundColor);

                        using (var gridPen = new Pen(GridColor, 1))
                        using (var textBrush = new SolidBrush(TextColor))
                        using (var headerTextBrush = new SolidBrush(HeaderTextColor))
                        using (var headerBgBrush = new SolidBrush(HeaderBgColor))
                        using (var altRowBrush = new SolidBrush(AltRowColor))
                        {
                            for (int row = 0; row < rowCount; row++)
                            {
                                int y = row * cellHeight;

                                // Row background
                                if (row == 0)
                                    g.FillRectangle(headerBgBrush, 0, y, tableWidth, cellHeight);
                                else if (row % 2 == 0)
                                    g.FillRectangle(altRowBrush, 0, y, tableWidth, cellHeight);

                                // Horizontal grid line
                                g.DrawLine(gridPen, 0, y, tableWidth - 1, y);

                                var cells = row < rows.Count ? rows[row] : new string[0];

                                for (int col = 0; col < colCount; col++)
                                {
                                    int x = col * cellWidth;

                                    // Vertical grid line
                                    g.DrawLine(gridPen, x, 0, x, tableHeight - 1);

                                    // Cell text
                                    string text = col < cells.Length ? cells[col].Trim() : "";
                                    if (text.Length > MaxCellChars)
                                        text = text.Substring(0, MaxCellChars - 1) + "\u2026";

                                    var rect = new RectangleF(x + CellPadding, y + CellPadding,
                                        cellWidth - CellPadding * 2, cellHeight - CellPadding * 2);

                                    var sf = new StringFormat
                                    {
                                        Trimming = StringTrimming.EllipsisCharacter,
                                        FormatFlags = StringFormatFlags.NoWrap
                                    };

                                    g.DrawString(text,
                                        row == 0 ? headerFont : font,
                                        row == 0 ? headerTextBrush : textBrush,
                                        rect, sf);
                                }
                            }

                            // Bottom and right border
                            g.DrawLine(gridPen, 0, tableHeight - 1, tableWidth - 1, tableHeight - 1);
                            g.DrawLine(gridPen, tableWidth - 1, 0, tableWidth - 1, tableHeight - 1);
                        }
                    }

                    return bmp;
                }
            }
        }
    }
}
