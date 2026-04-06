# ThumbnailPreviewer

Windows Explorer shell extension that generates thumbnail previews for file formats not natively supported by Windows.

![License](https://img.shields.io/badge/license-MIT-blue)
![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11%20x64-brightgreen)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.8-purple)

## Supported Formats

| Category | Extensions | Rendering Engine |
|----------|-----------|-----------------|
| **PDF** | `.pdf` | PDFium (bundled) |
| **Adobe** | `.psd`, `.ai`, `.eps` | Magick.NET / Ghostscript (bundled) |
| **Vector** | `.svg`, `.svgz` | Magick.NET |
| **Camera RAW** | `.dng`, `.cr2`, `.cr3`, `.nef`, `.arw`, `.orf`, `.rw2`, `.raf`, `.srw`, `.pef` | Magick.NET + LibRaw |
| **Documents** | `.docx` | Open XML parser + text renderer |
| **OpenOffice** | `.odt`, `.ods`, `.odp`, `.odg` | Embedded thumbnail extraction |
| **Data** | `.csv`, `.tsv` | Table grid renderer |
| **PostScript** | `.ps` | Ghostscript (bundled) |

## How It Works

ThumbnailPreviewer registers as a Windows Shell **IThumbnailProvider** via COM. When Explorer needs to display a thumbnail for a supported file type, Windows calls our handler which renders a preview image.

### Architecture

```
Windows Explorer
    |
    v
HKCR\.pdf\shellex\{E357FCCD-...} --> PdfThumbnailHandler (COM)
                                        |
                                        v
                                    PdfRenderer (PDFium)
                                        |
                                        v
                                    BadgeOverlay ("PDF")
                                        |
                                        v
                                    Bitmap returned to Explorer
```

1. **COM Registration** - Each handler class has a unique GUID and is registered via `regasm.exe`. Registry entries at `HKCR\.{ext}\shellex\{E357FCCD-A995-4576-B01F-234630154E96}` point Windows to the correct handler.

2. **Handler Classes** - Separate handler per format group (PDF, DOCX, SVG, PSD, RAW, EPS, AI, OpenOffice, CSV). Each inherits from SharpShell's `SharpThumbnailHandler` and overrides `GetThumbnailImage(uint width)`.

3. **Renderers** - Stateless rendering modules:
   - `PdfRenderer` - Uses PDFium (via PDFtoImage NuGet) to render the first page
   - `MagickRenderer` - Uses ImageMagick (via Magick.NET) for PSD, SVG, and RAW files. RAW files use a fast-path that extracts the embedded JPEG thumbnail before falling back to full decode
   - `GhostscriptRenderer` - Uses Magick.NET + bundled Ghostscript for EPS/AI/PS
   - `DocxRenderer` - Parses Open XML (`word/document.xml`), extracts paragraphs with heading detection, renders as a page preview
   - `ZipThumbnailExtractor` - Extracts `Thumbnails/thumbnail.png` from OpenDocument ZIP archives
   - `CsvRenderer` - Parses delimited text and renders a styled table grid
   - `BadgeOverlay` - Draws a small rounded-pill extension label (e.g. "PDF", "SVG") in the bottom-right corner

4. **Settings** - Per-extension toggle for preview and badge stored in `HKCU\Software\ThumbnailPreviewer\`. Handlers check settings via `SettingsManager` (5-second cache) before rendering. Can be configured during installation or later via the Settings app.

5. **Native DLL Loading** - Shell extensions run inside `dllhost.exe` (COM surrogate) where the working directory is `System32`. `PdfRenderer` uses `LoadLibrary`/`SetDllDirectory` P/Invoke to preload `pdfium.dll` and `libSkiaSharp.dll` from the correct path.

### Dependencies

| Package | Purpose | Bundled |
|---------|---------|---------|
| [SharpShell](https://github.com/dwmkerr/sharpshell) 2.7.2 | COM shell extension framework | NuGet |
| [Magick.NET-Q8-x64](https://github.com/dlemstra/Magick.NET) 14.11.1 | Image rendering (PSD, SVG, RAW) | NuGet |
| [PDFtoImage](https://github.com/sungaila/PDFtoImage) 5.2.0 | PDF rendering via PDFium | NuGet |
| [Ghostscript](https://ghostscript.com/) 10.07.0 | EPS/AI/PS rendering | Bundled in installer |

## Installation

### Installer (recommended)

Download `ThumbnailPreviewerSetup-1.0.0.exe` from [Releases](../../releases) and run it. The installer:

1. Copies files to `Program Files\ThumbnailPreviewer`
2. Shows a configuration page where you choose which formats to enable
3. Registers the COM shell extension via `regasm`
4. Writes extension-level registry entries
5. Clears the thumbnail cache and restarts Explorer

### Manual (development)

```powershell
# Build
dotnet build ThumbnailPreviewer.sln -c Release -p:Platform=x64

# Register (run as Administrator)
.\scripts\Register-Dev.ps1

# Unregister
.\scripts\Unregister-Dev.ps1
```

## Settings

Launch **ThumbnailPreviewer Settings** from the Start Menu (or run `ThumbnailPreviewer.Settings.exe`).

- **Preview** column - enable/disable thumbnail generation per extension
- **Badge** column - show/hide the extension label overlay
- **Save** - writes to registry and offers to restart Explorer
- **Reset Defaults** - re-enables all formats

Settings are stored in `HKCU\Software\ThumbnailPreviewer\` and take effect after Explorer restart.

## Building the Installer

```powershell
# Full build + installer (requires Inno Setup 6)
.\scripts\Build-Release.ps1
```

Output: `src\ThumbnailPreviewer.Installer\Output\ThumbnailPreviewerSetup-1.0.0.exe`

## Project Structure

```
src/
  ThumbnailPreviewer/           # Shell extension DLL
    Handlers/                   # COM handler classes (one per format group)
    Renderers/                  # Rendering engines + badge overlay
    Infrastructure/             # Settings manager, Ghostscript detector, logger
  ThumbnailPreviewer.Settings/  # WinForms settings app
  ThumbnailPreviewer.Installer/ # Inno Setup script + bundled Ghostscript
scripts/                        # Dev registration + build scripts
tests/                          # Test project
```

## License

MIT

## Contact

- Website: [katador.net](https://katador.net)
- Email: office@katador.net
