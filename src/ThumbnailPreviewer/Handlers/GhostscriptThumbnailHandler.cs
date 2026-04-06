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
    [COMServerAssociation(AssociationType.FileExtension, ".ps")]
    public class EpsThumbnailHandler : SharpThumbnailHandler
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
                ThumbnailLogger.Error(nameof(EpsThumbnailHandler), "Failed to generate thumbnail", ex);
                return null;
            }
        }
    }

    [ComVisible(true)]
    [Guid("2B3C4D5E-6F7A-4B8C-9D0E-1F2A3B4C5D6E")]
    [COMServerAssociation(AssociationType.FileExtension, ".ai")]
    public class AiThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                if (!SettingsManager.IsPreviewEnabled("ai"))
                    return null;

                var bmp = GhostscriptRenderer.Render(SelectedItemStream, width);

                if (SettingsManager.IsBadgeEnabled("ai"))
                    BadgeOverlay.Apply(bmp, "AI");

                return bmp;
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(AiThumbnailHandler), "Failed to generate thumbnail", ex);
                return null;
            }
        }
    }
}
