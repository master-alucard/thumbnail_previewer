using System;
using System.Drawing;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using ThumbnailPreviewer.Infrastructure;
using ThumbnailPreviewer.Renderers;

namespace ThumbnailPreviewer.Handlers
{
    [ComVisible(true)]
    [Guid("1A2B3C4D-5E6F-4A8B-9C0D-1E2F3A4B5C6D")]
    [COMServerAssociation(AssociationType.FileExtension, ".pdf")]
    public class PdfThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                if (!SettingsManager.IsPreviewEnabled("pdf"))
                    return null;

                var bmp = PdfRenderer.Render(SelectedItemStream, width);

                if (SettingsManager.IsBadgeEnabled("pdf"))
                    BadgeOverlay.Apply(bmp, "PDF");

                return bmp;
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(PdfThumbnailHandler), "Failed to generate thumbnail", ex);
                return null;
            }
        }
    }
}
