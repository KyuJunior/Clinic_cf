using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MedicalApp.Models;
using MedicalApp.Data;
using MedicalApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MedicalApp.ViewModels
{
    public partial class PrintSettingsViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ISharedStateService _sharedStateService;
        private static readonly string SettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "print_settings.json");

        [ObservableProperty]
        private string _rxBackgroundPath = string.Empty;

        [ObservableProperty]
        private bool _printBackground;

        [ObservableProperty]
        private double _patientNameX = 40;

        [ObservableProperty]
        private double _patientNameY = 100;

        [ObservableProperty]
        private double _patientAgeGenderX = 40;

        [ObservableProperty]
        private double _patientAgeGenderY = 125;

        [ObservableProperty]
        private double _patientDateX = 230;

        [ObservableProperty]
        private double _patientDateY = 100;

        [ObservableProperty]
        private double _rxSymbolX = 40;

        [ObservableProperty]
        private double _rxSymbolY = 160;

        [ObservableProperty]
        private bool _showRxSymbol = true;

        [ObservableProperty]
        private double _drugsX = 40;

        [ObservableProperty]
        private double _drugsY = 200;

        [ObservableProperty]
        private double _fontSize = 14;

        [ObservableProperty]
        private int _activeTab = 0; // 0=Print, 1=Clinic, 2=Database, 3=Staff, 4=Templates

        [ObservableProperty]
        private string _selectedCategory = "Prescription"; // "Prescription", "Investigation", "Imaging"

        // ------------------ Backing Store for Categories ------------------
        private string _rxBackingBackgroundPath = string.Empty;
        private bool _rxBackingPrintBackground;
        private double _rxPatientNameX = 40;
        private double _rxPatientNameY = 100;
        private double _rxPatientAgeGenderX = 40;
        private double _rxPatientAgeGenderY = 125;
        private double _rxPatientDateX = 230;
        private double _rxPatientDateY = 100;
        private double _rxDrugsX = 40;
        private double _rxDrugsY = 200;
        private double _rxFontSize = 14;

        private string _invBackgroundPath = string.Empty;
        private bool _printInvBackground;
        private double _invPatientNameX = 40;
        private double _invPatientNameY = 100;
        private double _invPatientAgeGenderX = 40;
        private double _invPatientAgeGenderY = 125;
        private double _invPatientDateX = 230;
        private double _invPatientDateY = 100;
        private double _invContentX = 40;
        private double _invContentY = 200;
        private double _invFontSize = 14;

        private string _imgBackgroundPath = string.Empty;
        private bool _printImgBackground;
        private double _imgPatientNameX = 40;
        private double _imgPatientNameY = 100;
        private double _imgPatientAgeGenderX = 40;
        private double _imgPatientAgeGenderY = 125;
        private double _imgPatientDateX = 230;
        private double _imgPatientDateY = 100;
        private double _imgContentX = 40;
        private double _imgContentY = 200;
        private double _imgFontSize = 14;

        // Swapping Active Values
        partial void OnSelectedCategoryChanging(string? oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(oldValue)) return;
            SaveActiveCategoryValues(oldValue);
        }

        partial void OnSelectedCategoryChanged(string value)
        {
            LoadActiveCategoryValues(value);
            OnPropertyChanged(nameof(IsRxCategory));
            OnPropertyChanged(nameof(ShowRxSymbolPreview));
            OnPropertyChanged(nameof(PreviewContentText1));
            OnPropertyChanged(nameof(PreviewContentText2));
        }

        partial void OnShowRxSymbolChanged(bool value)
        {
            OnPropertyChanged(nameof(ShowRxSymbolPreview));
        }

        public bool IsRxCategory => SelectedCategory == "Prescription";
        public bool ShowRxSymbolPreview => IsRxCategory && ShowRxSymbol;

        public string PreviewContentText1 => SelectedCategory switch
        {
            "Investigation" => "1. Complete Blood Count (CBC) - Attached Image",
            "Imaging" => "1. Chest X-Ray - Attached Image",
            _ => "1. Amoxicillin 500mg (1x3 Daily)"
        };

        public string PreviewContentText2 => SelectedCategory switch
        {
            "Investigation" => "2. Thyroid Profile (TSH, Free T3, Free T4)",
            "Imaging" => "2. Pelvic Ultrasound",
            _ => "2. Paracetamol 1000mg (As needed for pain)"
        };

        private void SaveActiveCategoryValues(string category)
        {
            if (category == "Prescription")
            {
                _rxBackingBackgroundPath = RxBackgroundPath;
                _rxBackingPrintBackground = PrintBackground;
                _rxPatientNameX = PatientNameX;
                _rxPatientNameY = PatientNameY;
                _rxPatientAgeGenderX = PatientAgeGenderX;
                _rxPatientAgeGenderY = PatientAgeGenderY;
                _rxPatientDateX = PatientDateX;
                _rxPatientDateY = PatientDateY;
                _rxDrugsX = DrugsX;
                _rxDrugsY = DrugsY;
                _rxFontSize = FontSize;
            }
            else if (category == "Investigation")
            {
                _invBackgroundPath = RxBackgroundPath;
                _printInvBackground = PrintBackground;
                _invPatientNameX = PatientNameX;
                _invPatientNameY = PatientNameY;
                _invPatientAgeGenderX = PatientAgeGenderX;
                _invPatientAgeGenderY = PatientAgeGenderY;
                _invPatientDateX = PatientDateX;
                _invPatientDateY = PatientDateY;
                _invContentX = DrugsX;
                _invContentY = DrugsY;
                _invFontSize = FontSize;
            }
            else if (category == "Imaging")
            {
                _imgBackgroundPath = RxBackgroundPath;
                _printImgBackground = PrintBackground;
                _imgPatientNameX = PatientNameX;
                _imgPatientNameY = PatientNameY;
                _imgPatientAgeGenderX = PatientAgeGenderX;
                _imgPatientAgeGenderY = PatientAgeGenderY;
                _imgPatientDateX = PatientDateX;
                _imgPatientDateY = PatientDateY;
                _imgContentX = DrugsX;
                _imgContentY = DrugsY;
                _imgFontSize = FontSize;
            }
        }

        private void LoadActiveCategoryValues(string category)
        {
            if (category == "Prescription")
            {
                RxBackgroundPath = _rxBackingBackgroundPath;
                PrintBackground = _rxBackingPrintBackground;
                PatientNameX = _rxPatientNameX;
                PatientNameY = _rxPatientNameY;
                PatientAgeGenderX = _rxPatientAgeGenderX;
                PatientAgeGenderY = _rxPatientAgeGenderY;
                PatientDateX = _rxPatientDateX;
                PatientDateY = _rxPatientDateY;
                DrugsX = _rxDrugsX;
                DrugsY = _rxDrugsY;
                FontSize = _rxFontSize;
            }
            else if (category == "Investigation")
            {
                RxBackgroundPath = _invBackgroundPath;
                PrintBackground = _printInvBackground;
                PatientNameX = _invPatientNameX;
                PatientNameY = _invPatientNameY;
                PatientAgeGenderX = _invPatientAgeGenderX;
                PatientAgeGenderY = _invPatientAgeGenderY;
                PatientDateX = _invPatientDateX;
                PatientDateY = _invPatientDateY;
                DrugsX = _invContentX;
                DrugsY = _invContentY;
                FontSize = _invFontSize;
            }
            else if (category == "Imaging")
            {
                RxBackgroundPath = _imgBackgroundPath;
                PrintBackground = _printImgBackground;
                PatientNameX = _imgPatientNameX;
                PatientNameY = _imgPatientNameY;
                PatientAgeGenderX = _imgPatientAgeGenderX;
                PatientAgeGenderY = _imgPatientAgeGenderY;
                PatientDateX = _imgPatientDateX;
                PatientDateY = _imgPatientDateY;
                DrugsX = _imgContentX;
                DrugsY = _imgContentY;
                FontSize = _imgFontSize;
            }
        }

        // Clinic Profile Settings
        [ObservableProperty]
        private string _clinicNameAr = "عيادتي التخصصية";

        [ObservableProperty]
        private string _clinicNameEn = "My Specialty Clinic";

        [ObservableProperty]
        private string _clinicPhone = "+964 770 123 4567";

        [ObservableProperty]
        private string _clinicAddress = "Baghdad, Iraq";

        [ObservableProperty]
        private string _clinicSpecialty = "Gynecology & Obstetrics | التوليد وأمراض النساء";

        // Database & Backups
        [ObservableProperty]
        private string _dbBackupPath = @"C:\Myapps\Backups";

        [ObservableProperty]
        private string _dbBackupInterval = "Daily | يومي";

        [ObservableProperty]
        private bool _dbAutoBackupEnabled = true;

        // Staff Settings
        [ObservableProperty]
        private string _adminPassword = "••••••••";

        [ObservableProperty]
        private bool _requireLogin = false;

        [RelayCommand]
        public void SwitchTab(string tabIndex)
        {
            if (int.TryParse(tabIndex, out int index))
            {
                ActiveTab = index;
            }
        }

        public PrintSettingsViewModel(
            IServiceProvider serviceProvider,
            IDbContextFactory<AppDbContext> contextFactory,
            ISharedStateService sharedStateService)
        {
            _serviceProvider = serviceProvider;
            _contextFactory = contextFactory;
            _sharedStateService = sharedStateService;
            LoadSettings();
        }

        public void LoadSettings()
        {
            try
            {
                PrintSettings? settings = null;
                var activeDocName = _sharedStateService.ActiveDoctorName;
                if (!string.IsNullOrEmpty(activeDocName))
                {
                    using var context = _contextFactory.CreateDbContext();
                    var record = context.DoctorSettings.FirstOrDefault(s => s.DoctorName == activeDocName);
                    if (record != null)
                    {
                        settings = JsonSerializer.Deserialize<PrintSettings>(record.SettingsJson);
                    }
                }

                if (settings == null)
                {
                    if (File.Exists(SettingsFile))
                    {
                        string json = File.ReadAllText(SettingsFile);
                        settings = JsonSerializer.Deserialize<PrintSettings>(json);
                    }
                }

                if (settings != null)
                {
                    // Rx settings
                    _rxBackingBackgroundPath = settings.RxBackgroundPath ?? string.Empty;
                    _rxBackingPrintBackground = settings.PrintBackground;
                    _rxPatientNameX = settings.PatientNameX;
                    _rxPatientNameY = settings.PatientNameY;
                    _rxPatientAgeGenderX = settings.PatientAgeGenderX;
                    _rxPatientAgeGenderY = settings.PatientAgeGenderY;
                    _rxPatientDateX = settings.PatientDateX;
                    _rxPatientDateY = settings.PatientDateY;
                    _rxDrugsX = settings.DrugsX;
                    _rxDrugsY = settings.DrugsY;
                    _rxFontSize = settings.FontSize;

                    // Inv settings
                    _invBackgroundPath = settings.InvBackgroundPath ?? string.Empty;
                    _printInvBackground = settings.PrintInvBackground;
                    _invPatientNameX = settings.InvPatientNameX;
                    _invPatientNameY = settings.InvPatientNameY;
                    _invPatientAgeGenderX = settings.InvPatientAgeGenderX;
                    _invPatientAgeGenderY = settings.InvPatientAgeGenderY;
                    _invPatientDateX = settings.InvPatientDateX;
                    _invPatientDateY = settings.InvPatientDateY;
                    _invContentX = settings.InvContentX;
                    _invContentY = settings.InvContentY;
                    _invFontSize = settings.InvFontSize;

                    // Img settings
                    _imgBackgroundPath = settings.ImgBackgroundPath ?? string.Empty;
                    _printImgBackground = settings.PrintImgBackground;
                    _imgPatientNameX = settings.ImgPatientNameX;
                    _imgPatientNameY = settings.ImgPatientNameY;
                    _imgPatientAgeGenderX = settings.ImgPatientAgeGenderX;
                    _imgPatientAgeGenderY = settings.ImgPatientAgeGenderY;
                    _imgPatientDateX = settings.ImgPatientDateX;
                    _imgPatientDateY = settings.ImgPatientDateY;
                    _imgContentX = settings.ImgContentX;
                    _imgContentY = settings.ImgContentY;
                    _imgFontSize = settings.ImgFontSize;

                    // Clinic Profile
                    ClinicNameAr = settings.ClinicNameAr ?? "عيادتي التخصصية";
                    ClinicNameEn = settings.ClinicNameEn ?? "My Specialty Clinic";
                    ClinicPhone = settings.ClinicPhone ?? "+964 770 123 4567";
                    ClinicAddress = settings.ClinicAddress ?? "Baghdad, Iraq";
                    ClinicSpecialty = settings.ClinicSpecialty ?? "Gynecology & Obstetrics | التوليد وأمراض النساء";

                    // Database & Backups
                    DbBackupPath = settings.DbBackupPath ?? @"C:\Myapps\Backups";
                    DbBackupInterval = settings.DbBackupInterval ?? "Daily | يومي";
                    DbAutoBackupEnabled = settings.DbAutoBackupEnabled;

                    // Staff Settings
                    AdminPassword = settings.AdminPassword ?? "••••••••";
                    RequireLogin = settings.RequireLogin;

                    // Populate Active values
                    LoadActiveCategoryValues(SelectedCategory);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load print settings: {ex.Message}", "Settings Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        [RelayCommand]
        public void UploadImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = $"Select {SelectedCategory} Background Image Template"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string ext = Path.GetExtension(openFileDialog.FileName);
                    var activeDocName = _sharedStateService.ActiveDoctorName;
                    var suffix = string.IsNullOrEmpty(activeDocName) ? "default" : activeDocName.Replace(" ", "_").Replace(".", "");
                    string categoryPrefix = SelectedCategory.ToLower();
                    string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{categoryPrefix}_template_{suffix}{ext}");

                    // If file already exists, delete it first to overwrite cleanly
                    if (File.Exists(destPath))
                    {
                        File.Delete(destPath);
                    }

                    File.Copy(openFileDialog.FileName, destPath, true);
                    RxBackgroundPath = destPath; // This updates active property
                    MessageBox.Show($"{SelectedCategory} background template uploaded successfully!", "Template Uploaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to copy template image: {ex.Message}", "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        public void SaveSettings()
        {
            try
            {
                SaveActiveCategoryValues(SelectedCategory);

                var settings = new PrintSettings
                {
                    RxBackgroundPath = _rxBackingBackgroundPath,
                    PrintBackground = _rxBackingPrintBackground,
                    PatientNameX = _rxPatientNameX,
                    PatientNameY = _rxPatientNameY,
                    PatientAgeGenderX = _rxPatientAgeGenderX,
                    PatientAgeGenderY = _rxPatientAgeGenderY,
                    PatientDateX = _rxPatientDateX,
                    PatientDateY = _rxPatientDateY,
                    RxSymbolX = RxSymbolX,
                    RxSymbolY = RxSymbolY,
                    ShowRxSymbol = ShowRxSymbol,
                    DrugsX = _rxDrugsX,
                    DrugsY = _rxDrugsY,
                    FontSize = _rxFontSize,

                    InvBackgroundPath = _invBackgroundPath,
                    PrintInvBackground = _printInvBackground,
                    InvPatientNameX = _invPatientNameX,
                    InvPatientNameY = _invPatientNameY,
                    InvPatientAgeGenderX = _invPatientAgeGenderX,
                    InvPatientAgeGenderY = _invPatientAgeGenderY,
                    InvPatientDateX = _invPatientDateX,
                    InvPatientDateY = _invPatientDateY,
                    InvContentX = _invContentX,
                    InvContentY = _invContentY,
                    InvFontSize = _invFontSize,

                    ImgBackgroundPath = _imgBackgroundPath,
                    PrintImgBackground = _printImgBackground,
                    ImgPatientNameX = _imgPatientNameX,
                    ImgPatientNameY = _imgPatientNameY,
                    ImgPatientAgeGenderX = _imgPatientAgeGenderX,
                    ImgPatientAgeGenderY = _imgPatientAgeGenderY,
                    ImgPatientDateX = _imgPatientDateX,
                    ImgPatientDateY = _imgPatientDateY,
                    ImgContentX = _imgContentX,
                    ImgContentY = _imgContentY,
                    ImgFontSize = _imgFontSize,

                    ClinicNameAr = ClinicNameAr,
                    ClinicNameEn = ClinicNameEn,
                    ClinicPhone = ClinicPhone,
                    ClinicAddress = ClinicAddress,
                    ClinicSpecialty = ClinicSpecialty,

                    DbBackupPath = DbBackupPath,
                    DbBackupInterval = DbBackupInterval,
                    DbAutoBackupEnabled = DbAutoBackupEnabled,

                    AdminPassword = AdminPassword,
                    RequireLogin = RequireLogin
                };

                // Save to database for active doctor
                var activeDocName = _sharedStateService.ActiveDoctorName;
                if (!string.IsNullOrEmpty(activeDocName))
                {
                    using var context = _contextFactory.CreateDbContext();
                    var record = context.DoctorSettings.FirstOrDefault(s => s.DoctorName == activeDocName);
                    string serializedJson = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                    if (record != null)
                    {
                        record.SettingsJson = serializedJson;
                        context.DoctorSettings.Update(record);
                    }
                    else
                    {
                        var newRecord = new DoctorSetting
                        {
                            DoctorName = activeDocName,
                            SettingsJson = serializedJson
                        };
                        context.DoctorSettings.Add(newRecord);
                    }
                    context.SaveChanges();
                }

                // Fallback: save to local file
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json);
                MessageBox.Show("Print calibration settings saved successfully!", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save print settings: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public void NavigateBack()
        {
            var mainVm = _serviceProvider.GetRequiredService<MainViewModel>();
            mainVm.NavigateToHome();
        }
    }
}
