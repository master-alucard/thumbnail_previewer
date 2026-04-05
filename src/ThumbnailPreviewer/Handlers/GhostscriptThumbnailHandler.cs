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
    [Guid("D5E6F7A8-B9C0-4123-DE45-F67890123456")]
    [COMServerAssociation(AssociationType.FileExtension, ".eps")]
    [COMServerAssociation(AssociationType.FileExtension, ".ai")]
    [COMServerAssociation(AssociationType.FileExtension, ".ps")]
    public class GhostscriptThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                if (!SettingsManager.IsPreviewEnabled("eps"))
                    return null;

                var bmp = GhostscriptRenderer.Render(SelectedItemStream, width);

                if (SettingsManager.IsBadgeEnabled("eps"))
                    BadgeOverlay.Apply(bmp, "EPS");

                return bmp;
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(GhostscriptThumbnailHandler), "Failed to generate thumbnail", ex);
                return null;
            }
        }
    }
}
