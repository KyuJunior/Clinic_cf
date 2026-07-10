; Inno Setup Script for Medical Clinic Application
; Save this file as setup.iss and compile it using Inno Setup Compiler (ISCC)

[Setup]
AppId={{F0A44161-BA2F-4ED6-A480-BF4B6EF1239C}
AppName=Medical Clinic App (تطبيق العيادة الطبية)
AppVersion=1.0
AppPublisher=Dr. Yaser
DefaultDirName={autopf}\MedicalApp
DefaultGroupName=Medical Clinic App
AllowNoIcons=yes
OutputDir=C:\Myapps\Project OF
OutputBaseFilename=MedicalAppSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Myapps\Project OF\MedicalApp\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Note: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
; Startup shortcuts pointing directly to specific rooms using arguments
Name: "{group}\Clinic Portal (البوابة الرئيسية)"; Filename: "{app}\MedicalApp.exe"; Parameters: "/home"; WorkingDir: "{app}"
Name: "{group}\Secretary Room (غرفة السكرتارية)"; Filename: "{app}\MedicalApp.exe"; Parameters: "/reg"; WorkingDir: "{app}"
Name: "{group}\Doctor Room (عيادة الطبيب)"; Filename: "{app}\MedicalApp.exe"; Parameters: "/exam"; WorkingDir: "{app}"
Name: "{autodesktop}\Clinic Portal (البوابة الرئيسية)"; Filename: "{app}\MedicalApp.exe"; Parameters: "/home"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{autodesktop}\Secretary Room (غرفة السكرتارية)"; Filename: "{app}\MedicalApp.exe"; Parameters: "/reg"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{autodesktop}\Doctor Room (عيادة الطبيب)"; Filename: "{app}\MedicalApp.exe"; Parameters: "/exam"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""MedicalApp SQL Server"" dir=in action=allow protocol=TCP localport=1433"; Flags: runhidden; StatusMsg: "Configuring Firewall settings... / جاري ضبط جدار الحماية..."
Filename: "{app}\MedicalApp.exe"; Parameters: "/home"; Description: "{cm:LaunchProgram,Clinic Portal}"; Flags: nowait postinstall skipifsilent
