# Install-MedicalApp.ps1
# Run this script as Administrator to install and configure MedicalApp automatically

# Check Administrator privileges
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "--------------------------------------------------------" -ForegroundColor Red
    Write-Host "ERROR: Please run this script as Administrator!" -ForegroundColor Red
    Write-Host "خطأ: يرجى تشغيل هذا السكربت كمسؤول لتثبيت البرنامج!" -ForegroundColor Red
    Write-Host "--------------------------------------------------------" -ForegroundColor Red
    Read-Host "Press Enter to exit / اضغط Enter للخروج"
    Exit
}

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "     Medical Clinic Application Automated Installer     " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

# Define paths
$installDir = "C:\MedicalApp"
$sourceDir = Join-Path $PSScriptRoot "MedicalApp\bin\Release\net8.0-windows\win-x64\publish"

if (-not (Test-Path $sourceDir)) {
    Write-Host "ERROR: Published files not found. Please compile the application first." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    Exit
}

# 1. Copy files
Write-Host "1. Copying files to $installDir... / جاري نسخ الملفات..." -ForegroundColor Yellow
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir | Out-Null
}
Copy-Item -Path "$sourceDir\*" -Destination $installDir -Recurse -Force

# 2. Add Firewall Rule for SQL Server Port 1433
Write-Host "2. Configuring Windows Defender Firewall for Port 1433... / جاري ضبط جدار الحماية..." -ForegroundColor Yellow
try {
    Remove-NetFirewallRule -Name "MedicalApp_SQLServer" -ErrorAction SilentlyContinue
    New-NetFirewallRule -Name "MedicalApp_SQLServer" -DisplayName "MedicalApp SQL Server (Port 1433)" -Description "Allow SQL Server remote connections for clinic PCs" -Protocol TCP -LocalPort 1433 -Action Allow -Direction Inbound | Out-Null
    Write-Host "   Firewall configured successfully." -ForegroundColor Green
} catch {
    Write-Host "   Warning: Could not configure firewall automatically." -ForegroundColor Red
}

# 3. Create Shortcuts on Desktop
Write-Host "3. Creating Desktop shortcuts... / جاري إنشاء اختصارات سطح المكتب..." -ForegroundColor Yellow
$WshShell = New-Object -ComObject WScript.Shell
$desktopPath = [System.Environment]::GetFolderPath("Desktop")

# Shortcut 1: Secretary Room
$shortcutReg = $WshShell.CreateShortcut(Join-Path $desktopPath "Secretary Room (غرفة السكرتارية).lnk")
$shortcutReg.TargetPath = "$installDir\MedicalApp.exe"
$shortcutReg.Arguments = "/reg"
$shortcutReg.WorkingDirectory = $installDir
$shortcutReg.Description = "Open Secretary Workspace Directly"
$shortcutReg.Save()

# Shortcut 2: Doctor Room
$shortcutExam = $WshShell.CreateShortcut(Join-Path $desktopPath "Doctor Room (عيادة الطبيب).lnk")
$shortcutExam.TargetPath = "$installDir\MedicalApp.exe"
$shortcutExam.Arguments = "/exam"
$shortcutExam.WorkingDirectory = $installDir
$shortcutExam.Description = "Open Doctor Workspace Directly"
$shortcutExam.Save()

# Shortcut 3: General Portal
$shortcutHome = $WshShell.CreateShortcut(Join-Path $desktopPath "Clinic Portal (البوابة الرئيسية).lnk")
$shortcutHome.TargetPath = "$installDir\MedicalApp.exe"
$shortcutHome.Arguments = "/home"
$shortcutHome.WorkingDirectory = $installDir
$shortcutHome.Description = "Open Medical Application Portal"
$shortcutHome.Save()

Write-Host "========================================================" -ForegroundColor Green
Write-Host "SUCCESS: Installation Completed! / تم التثبيت بنجاح!" -ForegroundColor Green
Write-Host "Installed Location: $installDir" -ForegroundColor Green
Write-Host "Desktop shortcuts created for all clinic rooms." -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green

Read-Host "Press Enter to exit / اضغط Enter للخروج"
