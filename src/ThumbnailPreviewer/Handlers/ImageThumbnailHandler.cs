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
    [Guid("B3C4D5E6-F7A8-4901-BC23-DEF456789012")]
    [COMServerAssociation(AssociationType.FileExtension, ".svg")]
    [COMServerAssociation(AssociationType.FileExtension, ".svgz")]
    public class SvgThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                if (!SettingsManager.IsPreviewEnabled("svg"))
                    return null;

                var bmp = MagickRenderer.RenderImage(SelectedItemStream, width);

                if (SettingsManager.IsBadgeEnabled("svg"))
                    BadgeOverlay.Apply(bmp, "SVG");

                return bmp;
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(SvgThumbnailHandler), "Failed to generate thumbnail", ex);
                return null;
            }
        }
    }

    [ComVisible(true)]
    [Guid("A1B2C3D4-E5F6-4789-AB01-23456789CDEF")]
    [COMServerAssociation(AssociationType.FileExtension, ".psd")]
    public class PsdThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                if (!SettingsManager.IsPreviewEnabled("psd"))
                    return null;

                var bmp = MagickRenderer.RenderImage(SelectedItemStream, width);

                if (SettingsManager.IsBadgeEnabled("psd"))
                    BadgeOverlay.Apply(bmp, "PSD");

                return bmp;
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(PsdThumbnailHandler), "Failed to generate thumbnail", ex);
                return null;
            }
        }
    }

    [ComVisible(true)]
    [Guid("C4D5E6F7-A8B9-4012-CD34-EF5678901234")]
    [COMServerAssociation(AssociationType.FileExtension, ".dng")]
    [COMServerAssociation(AssociationType.FileExtension, ".cr2")]
    [COMServerAssociation(AssociationType.FileExtension, ".cr3")]
    [COMServerAssociation(AssociationType.FileExtension, ".nef")]
    [COMServerAssociation(AssociationType.FileExtension, ".arw")]
    [COMServerAssociation(AssociationType.FileExtension, ".orf")]
    [COMServerAssociation(AssociationType.FileExtension, ".rw2")]
    [COMServerAssociation(AssociationType.FileExtension, ".raf")]
    [COMServerAssociation(AssociationType.FileExtension, ".srw")]
    [COMServerAssociation(AssociationType.FileExtension, ".pef")]
    public class RawThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                if (!SettingsManager.IsPreviewEnabled("dng"))
                    return null;

                var bmp = MagickRenderer.RenderRaw(SelectedItemStream, width);

                if (SettingsManager.IsBadgeEnabled("dng"))
                    BadgeOverlay.Apply(bmp, "RAW");

                return bmp;
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(RawThumbnailHandler), "Failed to generate thumbnail", ex);
                return null;
            }
        }
    }
}
