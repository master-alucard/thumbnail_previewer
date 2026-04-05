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
    /// SVG thumbnail handler via Magick.NET.
    /// </summary>
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
                ThumbnailLogger.Debug(nameof(SvgThumbnailHandler),
                    $"Generating SVG thumbnail, width={width}");

                var bmp = MagickRenderer.RenderImage(SelectedItemStream, width);
                return BadgeOverlay.Apply(bmp, "SVG");
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(SvgThumbnailHandler), "Failed to generate SVG thumbnail", ex);
                return null;
            }
        }
    }

    /// <summary>
    /// PSD thumbnail handler via Magick.NET.
    /// </summary>
    [ComVisible(true)]
    [Guid("A1B2C3D4-E5F6-4789-AB01-23456789CDEF")]
    [COMServerAssociation(AssociationType.FileExtension, ".psd")]
    public class PsdThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                ThumbnailLogger.Debug(nameof(PsdThumbnailHandler),
                    $"Generating PSD thumbnail, width={width}");

                var bmp = MagickRenderer.RenderImage(SelectedItemStream, width);
                return BadgeOverlay.Apply(bmp, "PSD");
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(PsdThumbnailHandler), "Failed to generate PSD thumbnail", ex);
                return null;
            }
        }
    }

    /// <summary>
    /// DNG and camera RAW file thumbnails via Magick.NET with fast-path extraction.
    /// </summary>
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
                ThumbnailLogger.Debug(nameof(RawThumbnailHandler),
                    $"Generating RAW thumbnail, width={width}");

                var bmp = MagickRenderer.RenderRaw(SelectedItemStream, width);
                return BadgeOverlay.Apply(bmp, "RAW");
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(RawThumbnailHandler), "Failed to generate RAW thumbnail", ex);
                return null;
            }
        }
    }
}
