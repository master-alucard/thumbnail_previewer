; ThumbnailPreviewer Installer - Inno Setup Script
; Requires Inno Setup 6+

#define MyAppName "ThumbnailPreviewer"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Katador"
#define MyAppURL "https://gitlab.com/katador.net/thumbnailpreviewer"

; Handler CLSIDs (must match Guid attributes in C# handler classes)
; Note: use {{...} for literal braces in Inno Setup
#define CLSID_Pdf       "{{1A2B3C4D-5E6F-4A8B-9C0D-1E2F3A4B5C6D}"
#define CLSID_Docx      "{{A2B3C4D5-E6F7-4890-AB12-CDEF34567890}"
#define CLSID_Svg       "{{B3C4D5E6-F7A8-4901-BC23-DEF456789012}"
#define CLSID_Psd       "{{A1B2C3D4-E5F6-4789-AB01-23456789CDEF}"
#define CLSID_Raw       "{{C4D5E6F7-A8B9-4012-CD34-EF5678901234}"
#define CLSID_Gs        "{{D5E6F7A8-B9C0-4123-DE45-F67890123456}"
#define CLSID_OpenOffice "{{F1A2B3C4-D5E6-4F78-9012-3456789ABCDE}"
#define CLSID_Csv       "{{E6F7A8B9-C0D1-4234-EF56-789012345678}"

; Windows shell thumbnail handler interface GUID
#define SHELLEX_THUMB   "{{E357FCCD-A995-4576-B01F-234630154E96}"

[Setup]
AppId={{8A9B0C1D-2E3F-4A5B-6C7D-8E9F0A1B2C3D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=ThumbnailPreviewerSetup-{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
SetupLogging=yes
UninstallDisplayName={#MyAppName}

; --- Upgrade support ---
UsePreviousAppDir=yes
AppVerName={#MyAppName} {#MyAppVersion}
CloseApplications=force
CloseApplicationsFilter=dllhost.exe,explorer.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Main DLL and all dependencies
Source: "..\..\dist\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

; Bundled Ghostscript
Source: "deps\ghostscript\*"; DestDir: "{app}\ghostscript"; Flags: ignoreversion recursesubdirs

[Registry]
; =====================================================================
; Force thumbnail handler registration directly at the EXTENSION level.
; This is critical: Windows checks HKCR\.ext\shellex\{E357FCCD-...}
; BEFORE checking the ProgID class key. Without these entries, our
; handlers are never called for extensions that already have handlers.
; =====================================================================

; --- PDF ---
Root: HKCR; Subkey: ".pdf\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Pdf}"; Flags: uninsdeletekey

; --- DOCX ---
Root: HKCR; Subkey: ".docx\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Docx}"; Flags: uninsdeletekey

; --- PSD ---
Root: HKCR; Subkey: ".psd\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Psd}"; Flags: uninsdeletekey

; --- SVG ---
Root: HKCR; Subkey: ".svg\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Svg}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".svgz\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Svg}"; Flags: uninsdeletekey

; --- RAW / DNG ---
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

; --- EPS / AI / PS ---
Root: HKCR; Subkey: ".eps\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Gs}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".ai\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Gs}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".ps\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Gs}"; Flags: uninsdeletekey

; --- OpenOffice ---
Root: HKCR; Subkey: ".odt\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_OpenOffice}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".ods\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_OpenOffice}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".odp\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_OpenOffice}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".odg\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_OpenOffice}"; Flags: uninsdeletekey

; --- CSV / TSV ---
Root: HKCR; Subkey: ".csv\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Csv}"; Flags: uninsdeletekey
Root: HKCR; Subkey: ".tsv\shellex\{#SHELLEX_THUMB}"; ValueType: string; ValueData: "{#CLSID_Csv}"; Flags: uninsdeletekey

[Run]
; Register COM DLL after install
Filename: "{dotnet4064}\regasm.exe"; Parameters: """{app}\ThumbnailPreviewer.dll"" /codebase"; \
  StatusMsg: "Registering shell extension..."; Flags: runhidden waituntilterminated

; Clear thumbnail cache so new handlers take effect immediately
Filename: "{cmd}"; Parameters: "/c del /f /s /q ""%localappdata%\Microsoft\Windows\Explorer\thumbcache_*"" >nul 2>&1"; \
  StatusMsg: "Clearing thumbnail cache..."; Flags: runhidden waituntilterminated

; Restart Explorer
Filename: "{cmd}"; Parameters: "/c timeout /t 1 /nobreak >nul & start explorer.exe"; \
  StatusMsg: "Restarting Explorer..."; Flags: runhidden waituntilterminated

[UninstallRun]
; Unregister COM DLL
Filename: "{dotnet4064}\regasm.exe"; Parameters: "/unregister ""{app}\ThumbnailPreviewer.dll"""; \
  RunOnceId: "UnregisterCOM"; Flags: runhidden waituntilterminated

; Restart Explorer
Filename: "{cmd}"; Parameters: "/c timeout /t 1 /nobreak >nul & start explorer.exe"; \
  StatusMsg: "Restarting Explorer..."; RunOnceId: "RestartExplorer"; Flags: runhidden waituntilterminated

[Code]

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
  OldDll: String;
  RegAsm: String;
  ResultCode: Integer;
begin
  OldDll := ExpandConstant('{app}\ThumbnailPreviewer.dll');
  RegAsm := ExpandConstant('{dotnet4064}\regasm.exe');

  if FileExists(OldDll) and FileExists(RegAsm) then
  begin
    Log('Unregistering old DLL: ' + OldDll);
    Exec(RegAsm, '/unregister "' + OldDll + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
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
