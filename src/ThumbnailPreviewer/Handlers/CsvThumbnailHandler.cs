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
    [Guid("E6F7A8B9-C0D1-4234-EF56-789012345678")]
    [COMServerAssociation(AssociationType.FileExtension, ".csv")]
    [COMServerAssociation(AssociationType.FileExtension, ".tsv")]
    public class CsvThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                ThumbnailLogger.Debug(nameof(CsvThumbnailHandler),
                    $"Generating thumbnail, width={width}");

                return CsvRenderer.Render(SelectedItemStream, width);
            }
            catch (Exception ex)
            {
                ThumbnailLogger.Error(nameof(CsvThumbnailHandler), "Failed to generate thumbnail", ex);
                return null;
            }
        }
    }
}
