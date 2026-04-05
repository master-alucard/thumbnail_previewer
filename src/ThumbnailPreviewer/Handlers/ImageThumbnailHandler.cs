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
    /// Handles PSD and SVG thumbnails via Magick.NET.
    /// </summary>
    [ComVisible(true)]
    [Guid("B3C4D5E6-F7A8-4901-BC23-DEF456789012")]
    [COMServerAssociation(AssociationType.FileExtension, ".psd")]
    [COMServerAssociation(AssociationType.FileExtension, ".svg")]
    [COMServerAssociation(AssociationType.FileExtension, ".svgz")]
    public class ImageThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                ThumbnailLogger.Debug(nameof(ImageThumbnailHandler),
                    $"Generating thumbnail, width={width}");

                return MagickRenderer.RenderImage(SelectedItemStream, width);
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(ImageThumbnailHandler), "Failed to generate thumbnail", ex);
                return null;
            }
        }
    }

    /// <summary>
    /// Handles DNG and camera RAW file thumbnails via Magick.NET with fast-path extraction.
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

                return MagickRenderer.RenderRaw(SelectedItemStream, width);
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(RawThumbnailHandler), "Failed to generate RAW thumbnail", ex);
                return null;
            }
        }
    }
}
