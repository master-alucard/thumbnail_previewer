; ThumbnailPreviewer Installer - Inno Setup Script
; Requires Inno Setup 6+

#define MyAppName "ThumbnailPreviewer"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Katador"
#define MyAppURL "https://gitlab.com/katador.net/thumbnailpreviewer"

; Handler CLSIDs
#define CLSID_Pdf       "{{1A2B3C4D-5E6F-4A8B-9C0D-1E2F3A4B5C6D}"
#define CLSID_Docx      "{{A2B3C4D5-E6F7-4890-AB12-CDEF34567890}"
#define CLSID_Svg       "{{B3C4D5E6-F7A8-4901-BC23-DEF456789012}"
#define CLSID_Psd       "{{A1B2C3D4-E5F6-4789-AB01-23456789CDEF}"
#define CLSID_Raw       "{{C4D5E6F7-A8B9-4012-CD34-EF5678901234}"
#define CLSID_Eps       "{{D5E6F7A8-B9C0-4123-DE45-F67890123456}"
#define CLSID_Ai        "{{2B3C4D5E-6F7A-4B8C-9D0E-1F2A3B4C5D6E}"
#define CLSID_OpenOffice "{{F1A2B3C4-D5E6-4F78-9012-3456789ABCDE}"
#define CLSID_Csv       "{{E6F7A8B9-C0D1-4234-EF56-789012345678}"
#define SHELLEX_THUMB   "{{E357FCCD-A995-4576-B01F-234630154E96}"

[Setup]
AppId={{8A9B0C1D-2E3F-4A5B-6C7D-8E9F0A1B2C3D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=auto
OutputDir=Output
OutputBaseFilename=ThumbnailPreviewerSetup-{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
SetupLogging=yes
UninstallDisplayName={#MyAppName}
UsePreviousAppDir=yes
AppVerName={#MyAppName} {#MyAppVersion}
CloseApplications=force
CloseApplicationsFilter=dllhost.exe,explorer.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\..\dist\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "..\..\dist-settings\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "deps\ghostscript\*"; DestDir: "{app}\ghostscript"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\ThumbnailPreviewer Settings"; Filename: "{app}\ThumbnailPreviewer.Settings.exe"
Name: "{group}\Uninstall ThumbnailPreviewer"; Filename: "{uninstallexe}"

[Registry]
; Extension-level shell handler registration
; --- PDF ---
Root: HKCR; Subkey: ".pdf\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Pdf}"; Flags: uninsdeletekey
; --- DOCX ---
Root: HKCR; Subkey: ".docx\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Docx}"; Flags: uninsdeletekey
; --- PSD ---
Root: HKCR; Subkey: ".psd\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Psd}"; Flags: uninsdeletekey
; --- SVG ---
Root: HKCR; Subkey: ".svg\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Svg}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".svgz\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Svg}"; Flags: uninsdeletekey
; --- RAW ---
Root: HKCR; Subkey: ".dng\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Raw}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".cr2\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Raw}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".cr3\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Raw}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".nef\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Raw}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".arw\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Raw}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".orf\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Raw}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".rw2\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Raw}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".raf\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Raw}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".srw\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Raw}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".pef\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Raw}"; Flags: uninsdeletekey
; --- EPS / PS ---
Root: HKCR; Subkey: ".eps\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Eps}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".ps\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Eps}"; Flags: uninsdeletekey
; --- AI ---
Root: HKCR; Subkey: ".ai\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Ai}"; Flags: uninsdeletekey
; --- OpenOffice ---
Root: HKCR; Subkey: ".odt\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_OpenOffice}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".ods\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_OpenOffice}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".odp\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_OpenOffice}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".odg\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_OpenOffice}"; Flags: uninsdeletekey
; --- CSV ---
Root: HKCR; Subkey: ".csv\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Csv}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".tsv\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Csv}"; Flags: uninsdeletekey

[Run]
Filename: "{dotnet4064}\regasm.exe"; Parameters: """{app}\ThumbnailPreviewer.dll"" /codebase"; \
  StatusMsg: "Registering shell extension..."; Flags: runhidden waituntilterminated
Filename: "{cmd}"; Parameters: "/c del /f /s /q ""%localappdata%\Microsoft\Windows\Explorer\thumbcache_*"" >nul 2>&1"; \
  StatusMsg: "Clearing thumbnail cache..."; Flags: runhidden waituntilterminated
Filename: "{cmd}"; Parameters: "/c timeout /t 1 /nobreak >nul & start explorer.exe"; \
  StatusMsg: "Restarting Explorer..."; Flags: runhidden waituntilterminated

[UninstallRun]
Filename: "{dotnet4064}\regasm.exe"; Parameters: "/unregister ""{app}\ThumbnailPreviewer.dll"""; \
  RunOnceId: "UnregisterCOM"; Flags: runhidden waituntilterminated
Filename: "{cmd}"; Parameters: "/c timeout /t 1 /nobreak >nul & start explorer.exe"; \
  StatusMsg: "Restarting Explorer..."; RunOnceId: "RestartExplorer"; Flags: runhidden waituntilterminated

[Code]

// =====================================================================
// Extension configuration page using two TNewCheckListBox controls
// (Preview + Badge) side by side with scrolling support.
// =====================================================================

const
  EXT_COUNT = 24;

var
  ConfigPage: TWizardPage;
  ExtNames: array[0..EXT_COUNT-1] of String;
  ExtDescs: array[0..EXT_COUNT-1] of String;
  PreviewList: TNewCheckListBox;
  BadgeList: TNewCheckListBox;

procedure InitExtData();
begin
  ExtNames[0]  := 'pdf';   ExtDescs[0]  := '.pdf  -  PDF Document';
  ExtNames[1]  := 'docx';  ExtDescs[1]  := '.docx  -  Word Document';
  ExtNames[2]  := 'csv';   ExtDescs[2]  := '.csv  -  CSV File';
  ExtNames[3]  := 'tsv';   ExtDescs[3]  := '.tsv  -  TSV File';
  ExtNames[4]  := 'eps';   ExtDescs[4]  := '.eps  -  Encapsulated PostScript';
  ExtNames[5]  := 'ai';    ExtDescs[5]  := '.ai  -  Adobe Illustrator';
  ExtNames[6]  := 'ps';    ExtDescs[6]  := '.ps  -  PostScript';
  ExtNames[7]  := 'odt';   ExtDescs[7]  := '.odt  -  OpenDocument Text';
  ExtNames[8]  := 'ods';   ExtDescs[8]  := '.ods  -  OpenDocument Spreadsheet';
  ExtNames[9]  := 'odp';   ExtDescs[9]  := '.odp  -  OpenDocument Presentation';
  ExtNames[10] := 'odg';   ExtDescs[10] := '.odg  -  OpenDocument Drawing';
  ExtNames[11] := 'svg';   ExtDescs[11] := '.svg  -  Scalable Vector Graphics';
  ExtNames[12] := 'svgz';  ExtDescs[12] := '.svgz  -  Compressed SVG';
  ExtNames[13] := 'psd';   ExtDescs[13] := '.psd  -  Photoshop Document';
  ExtNames[14] := 'dng';   ExtDescs[14] := '.dng  -  Digital Negative (RAW)';
  ExtNames[15] := 'cr2';   ExtDescs[15] := '.cr2  -  Canon RAW v2';
  ExtNames[16] := 'cr3';   ExtDescs[16] := '.cr3  -  Canon RAW v3';
  ExtNames[17] := 'nef';   ExtDescs[17] := '.nef  -  Nikon RAW';
  ExtNames[18] := 'arw';   ExtDescs[18] := '.arw  -  Sony RAW';
  ExtNames[19] := 'orf';   ExtDescs[19] := '.orf  -  Olympus RAW';
  ExtNames[20] := 'rw2';   ExtDescs[20] := '.rw2  -  Panasonic RAW';
  ExtNames[21] := 'raf';   ExtDescs[21] := '.raf  -  Fujifilm RAW';
  ExtNames[22] := 'srw';   ExtDescs[22] := '.srw  -  Samsung RAW';
  ExtNames[23] := 'pef';   ExtDescs[23] := '.pef  -  Pentax RAW';
end;

procedure CreateConfigPage();
var
  i: Integer;
  LblPreview, LblBadge: TLabel;
begin
  InitExtData();

  ConfigPage := CreateCustomPage(wpSelectDir,
    'Configure Thumbnail Previews',
    'Choose which file types should show thumbnail previews and extension badges. You can change these later in the Settings app.');

  // Preview column label
  LblPreview := TLabel.Create(ConfigPage);
  LblPreview.Parent := ConfigPage.Surface;
  LblPreview.Caption := 'Enable Preview:';
  LblPreview.Font.Style := [fsBold];
  LblPreview.Left := 0;
  LblPreview.Top := 0;

  // Preview checklist
  PreviewList := TNewCheckListBox.Create(ConfigPage);
  PreviewList.Parent := ConfigPage.Surface;
  PreviewList.Left := 0;
  PreviewList.Top := 20;
  PreviewList.Width := (ConfigPage.SurfaceWidth div 2) - 8;
  PreviewList.Height := ConfigPage.SurfaceHeight - 24;
  PreviewList.Flat := True;

  for i := 0 to EXT_COUNT - 1 do
  begin
    PreviewList.AddCheckBox(ExtDescs[i], '', 0, True, True, False, True, nil);
  end;

  // Badge column label
  LblBadge := TLabel.Create(ConfigPage);
  LblBadge.Parent := ConfigPage.Surface;
  LblBadge.Caption := 'Show Badge:';
  LblBadge.Font.Style := [fsBold];
  LblBadge.Left := (ConfigPage.SurfaceWidth div 2) + 8;
  LblBadge.Top := 0;

  // Badge checklist
  BadgeList := TNewCheckListBox.Create(ConfigPage);
  BadgeList.Parent := ConfigPage.Surface;
  BadgeList.Left := (ConfigPage.SurfaceWidth div 2) + 8;
  BadgeList.Top := 20;
  BadgeList.Width := (ConfigPage.SurfaceWidth div 2) - 8;
  BadgeList.Height := ConfigPage.SurfaceHeight - 24;
  BadgeList.Flat := True;

  for i := 0 to EXT_COUNT - 1 do
  begin
    BadgeList.AddCheckBox(ExtDescs[i], '', 0, True, True, False, True, nil);
  end;
end;

procedure WriteSettings();
var
  i: Integer;
  PreviewVal, BadgeVal: Cardinal;
begin
  for i := 0 to EXT_COUNT - 1 do
  begin
    if PreviewList.Checked[i] then PreviewVal := 1 else PreviewVal := 0;
    if BadgeList.Checked[i] then BadgeVal := 1 else BadgeVal := 0;

    RegWriteDWordValue(HKCU, 'Software\ThumbnailPreviewer\Preview',
      ExtNames[i], PreviewVal);
    RegWriteDWordValue(HKCU, 'Software\ThumbnailPreviewer\Badge',
      ExtNames[i], BadgeVal);
  end;
end;

// ---------------------------------------------------------------
// Upgrade support
// ---------------------------------------------------------------
function GetUninstallString(): String;
var
  UninstallPath: String;
begin
  Result := '';
  if RegQueryStringValue(HKLM,
    'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{8A9B0C1D-2E3F-4A5B-6C7D-8E9F0A1B2C3D}_is1',
    'UninstallString', UninstallPath) then
    Result := UninstallPath;
end;

function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;

procedure KillProcess(Name: String);
var
  ResultCode: Integer;
begin
  Exec('taskkill.exe', '/f /im ' + Name, '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

procedure UnregisterOldDll();
var
  OldDll, RegAsm: String;
  ResultCode: Integer;
begin
  OldDll := ExpandConstant('{app}\ThumbnailPreviewer.dll');
  RegAsm := ExpandConstant('{dotnet4064}\regasm.exe');
  if FileExists(OldDll) and FileExists(RegAsm) then
    Exec(RegAsm, '/unregister "' + OldDll + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

// ---------------------------------------------------------------
// Main hooks
// ---------------------------------------------------------------
procedure InitializeWizard();
begin
  CreateConfigPage();
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    if IsUpgrade() then
      UnregisterOldDll();
    KillProcess('dllhost.exe');
    KillProcess('explorer.exe');
    Sleep(1500);
  end;

  if CurStep = ssPostInstall then
  begin
    // Write user choices from config page to registry
    WriteSettings();
  end;
end;

// ---------------------------------------------------------------
// .NET Framework 4.8 check
// ---------------------------------------------------------------
function InitializeSetup(): Boolean;
var
  Release: Cardinal;
begin
  Result := True;
  if not RegQueryDWordValue(HKLM,
    'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) then
  begin
    MsgBox('.NET Framework 4.8 or later is required.' + #13#10 +
           'Please install it from https://dotnet.microsoft.com/download/dotnet-framework',
           mbCriticalError, MB_OK);
    Result := False;
    Exit;
  end;
  if Release < 528040 then
  begin
    MsgBox('.NET Framework 4.8 or later is required.' + #13#10 +
           'Your current version is older. Please update from:' + #13#10 +
           'https://dotnet.microsoft.com/download/dotnet-framework',
           mbCriticalError, MB_OK);
    Result := False;
  end;
end;
