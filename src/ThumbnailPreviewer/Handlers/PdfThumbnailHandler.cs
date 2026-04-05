using System;
using System.Drawing;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using ThumbnailPreviewer.Infrastructure;
using ThumbnailPreviewer.Renderers;

namespace ThumbnailPreviewer.Handlers
{
    /// <summary>
    /// PDF thumbnail handler using PDFium — renders the first page as a thumbnail.
    /// No external dependencies needed (PDFium is bundled via NuGet).
    /// </summary>
    [ComVisible(true)]
    [Guid("1A2B3C4D-5E6F-4A8B-9C0D-1E2F3A4B5C6D")]
    [COMServerAssociation(AssociationType.FileExtension, ".pdf")]
    public class PdfThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                ThumbnailLogger.Debug(nameof(PdfThumbnailHandler),
                    $"Generating PDF thumbnail, width={width}");

                var bmp = PdfRenderer.Render(SelectedItemStream, width);
                return BadgeOverlay.Apply(bmp, "PDF");
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(PdfThumbnailHandler), "Failed to generate PDF thumbnail", ex);
                return null;
            }
        }
    }
}
