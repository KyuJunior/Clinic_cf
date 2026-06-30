using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using Microsoft.Extensions.Configuration;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalApp.Models;
using MedicalApp.Services;

namespace MedicalApp.ViewModels
{
    public partial class EchoUploadViewModel : ObservableObject, IDisposable
    {
        private readonly IEchoService _echoService;
        private readonly ISharedStateService _sharedStateService;
        private readonly IConfiguration _configuration;

        [ObservableProperty]
        private Patient? _currentPatient;

        [ObservableProperty]
        private ObservableCollection<EchoRecord> _echoRecords = new();

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public EchoUploadViewModel(IEchoService echoService, ISharedStateService sharedStateService, IConfiguration configuration)
        {
            _echoService = echoService;
            _sharedStateService = sharedStateService;
            _configuration = configuration;

            // Load initial patient context and subscribe to selection updates
            CurrentPatient = _sharedStateService.CurrentPatient;
            _sharedStateService.CurrentPatientChanged += OnSharedPatientChanged;

            if (CurrentPatient != null)
            {
                _ = LoadEchoRecordsAsync();
            }
        }

        private void OnSharedPatientChanged(Patient? patient)
        {
            CurrentPatient = patient;
            if (patient != null)
            {
                _ = LoadEchoRecordsAsync();
            }
            else
            {
                EchoRecords.Clear();
            }
            StatusMessage = string.Empty;
        }

        [RelayCommand]
        public async Task LoadEchoRecordsAsync()
        {
            if (CurrentPatient == null) return;

            try
            {
                var records = await _echoService.GetEchoRecordsByPatientIdAsync(CurrentPatient.PatientId);
                EchoRecords = new ObservableCollection<EchoRecord>(records);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading Echo records: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task UploadEchoFileAsync()
        {
            if (CurrentPatient == null)
            {
                StatusMessage = "No patient selected. Please select a patient first.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                StatusMessage = "Please enter a Title/Description for the Echo record.";
                return;
            }

            // Read the central shared folder network storage path from configuration
            string? networkSharePath = _configuration["FileStorageSettings:NetworkSharePath"];
            if (string.IsNullOrEmpty(networkSharePath))
            {
                StatusMessage = "Storage configuration error: Central network folder path not defined.";
                return;
            }

            // Open standard file explorer selection dialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Media Files (*.mp4;*.avi;*.jpg;*.png;*.dcm)|*.mp4;*.avi;*.jpg;*.png;*.dcm|All files (*.*)|*.*",
                Title = "Select Echocardiogram File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string localFilePath = openFileDialog.FileName;
                string extension = Path.GetExtension(localFilePath);
                
                // Formulate unique file name: PatientId_Timestamp_GUID.extension
                string uniqueFileName = $"{CurrentPatient.PatientId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}{extension}";
                string destinationPath = Path.Combine(networkSharePath, uniqueFileName);

                try
                {
                    StatusMessage = "Uploading file to server share...";

                    // Copy local file asynchronously to prevent locking the WPF UI thread
                    await Task.Run(() => 
                    {
                        if (!Directory.Exists(networkSharePath))
                        {
                            Directory.CreateDirectory(networkSharePath);
                        }
                        
                        File.Copy(localFilePath, destinationPath, true);
                    });

                    // Log file reference database entry
                    var record = new EchoRecord
                    {
                        PatientId = CurrentPatient.PatientId,
                        Title = Title,
                        FilePath = destinationPath,
                        Notes = Notes,
                        UploadDate = DateTime.UtcNow
                    };

                    await _echoService.AddEchoRecordAsync(record);
                    StatusMessage = "File uploaded and recorded successfully!";

                    // Clear fields
                    Title = string.Empty;
                    Notes = string.Empty;

                    await LoadEchoRecordsAsync();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"File upload failed: {ex.Message}";
                }
            }
        }

        public void Dispose()
        {
            _sharedStateService.CurrentPatientChanged -= OnSharedPatientChanged;
            GC.SuppressFinalize(this);
        }
    }
}
