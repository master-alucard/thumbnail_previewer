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
    [Guid("F1A2B3C4-D5E6-4F78-9012-3456789ABCDE")]
    [COMServerAssociation(AssociationType.FileExtension, ".odt")]
    [COMServerAssociation(AssociationType.FileExtension, ".ods")]
    [COMServerAssociation(AssociationType.FileExtension, ".odp")]
    [COMServerAssociation(AssociationType.FileExtension, ".odg")]
    public class OpenOfficeThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                if (!SettingsManager.IsPreviewEnabled("odt"))
                    return null;

                var bmp = ZipThumbnailExtractor.ExtractOpenDocumentThumbnail(SelectedItemStream, width);

                if (SettingsManager.IsBadgeEnabled("odt"))
                    BadgeOverlay.Apply(bmp, "ODF");

                return bmp;
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(OpenOfficeThumbnailHandler), "Failed to generate thumbnail", ex);
                return null;
            }
        }
    }
}
