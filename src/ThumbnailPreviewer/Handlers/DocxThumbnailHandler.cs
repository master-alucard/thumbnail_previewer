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
    [Guid("A2B3C4D5-E6F7-4890-AB12-CDEF34567890")]
    [COMServerAssociation(AssociationType.FileExtension, ".docx")]
    public class DocxThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                ThumbnailLogger.Debug(nameof(DocxThumbnailHandler),
                    $"Generating thumbnail, width={width}");

                return DocxRenderer.Render(SelectedItemStream, width);
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(DocxThumbnailHandler), "Failed to generate thumbnail", ex);
                return null;
            }
        }
    }
}
