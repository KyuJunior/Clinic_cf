using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalApp.Models;
using MedicalApp.Services;

namespace MedicalApp.ViewModels
{
    public partial class ClinicalExamViewModel : ObservableObject, IDisposable
    {
        private readonly IVisitService _visitService;
        private readonly IPatientService _patientService;
        private readonly ISharedStateService _sharedStateService;

        [ObservableProperty]
        private Patient? _currentPatient;

        [ObservableProperty]
        private ObservableCollection<Visit> _visitHistory = new();

        // Standalone Patient Lookup fields
        [ObservableProperty]
        private string _searchTerm = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Patient> _patients = new();

        [ObservableProperty]
        private Patient? _selectedPatientLookup;

        // Visit Form Fields
        [ObservableProperty]
        private string _chiefComplaint = string.Empty;

        [ObservableProperty]
        private string _historyOfPresentIllness = string.Empty;

        [ObservableProperty]
        private string _physicalExamination = string.Empty;

        [ObservableProperty]
        private string _diagnosis = string.Empty;

        [ObservableProperty]
        private string _treatmentPlan = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public ClinicalExamViewModel(IVisitService visitService, IPatientService patientService, ISharedStateService sharedStateService)
        {
            _visitService = visitService;
            _patientService = patientService;
            _sharedStateService = sharedStateService;

            // Load initial patient context and subscribe to selection updates
            CurrentPatient = _sharedStateService.CurrentPatient;
            SelectedPatientLookup = CurrentPatient;
            _sharedStateService.CurrentPatientChanged += OnSharedPatientChanged;

            if (CurrentPatient != null)
            {
                _ = LoadVisitHistoryAsync();
            }

            // Load initial patient list for the search lookup dropdown
            _ = SearchPatientsAsync();
        }

        partial void OnSelectedPatientLookupChanged(Patient? value)
        {
            if (CurrentPatient != value)
            {
                CurrentPatient = value;
                _sharedStateService.CurrentPatient = value; // Keep shared state synchronized
                if (value != null)
                {
                    _ = LoadVisitHistoryAsync();
                }
                else
                {
                    VisitHistory.Clear();
                }
            }
        }

        [RelayCommand]
        public async Task SearchPatientsAsync()
        {
            try
            {
                var results = await _patientService.SearchPatientsAsync(SearchTerm);
                Patients = new ObservableCollection<Patient>(results);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching patients: {ex.Message}";
            }
        }

        private void OnSharedPatientChanged(Patient? patient)
        {
            if (SelectedPatientLookup != patient)
            {
                SelectedPatientLookup = patient;
            }
        }

        [RelayCommand]
        public async Task LoadVisitHistoryAsync()
        {
            if (CurrentPatient == null) return;

            try
            {
                var visits = await _visitService.GetVisitsByPatientIdAsync(CurrentPatient.PatientId);
                VisitHistory = new ObservableCollection<Visit>(visits);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading visit history: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task SaveVisitAsync()
        {
            if (CurrentPatient == null)
            {
                StatusMessage = "No patient selected. Please select a patient first.";
                return;
            }

            if (string.IsNullOrWhiteSpace(ChiefComplaint) && string.IsNullOrWhiteSpace(Diagnosis))
            {
                StatusMessage = "Chief Complaint or Diagnosis is required to log a visit.";
                return;
            }

            try
            {
                var visit = new Visit
                {
                    PatientId = CurrentPatient.PatientId,
                    ChiefComplaint = ChiefComplaint,
                    HistoryOfPresentIllness = HistoryOfPresentIllness,
                    PhysicalExamination = PhysicalExamination,
                    Diagnosis = Diagnosis,
                    TreatmentPlan = TreatmentPlan,
                    VisitDate = DateTime.UtcNow
                };

                await _visitService.AddVisitAsync(visit);
                StatusMessage = "Visit log saved successfully!";

                // Clear Form Fields
                ChiefComplaint = string.Empty;
                HistoryOfPresentIllness = string.Empty;
                PhysicalExamination = string.Empty;
                Diagnosis = string.Empty;
                TreatmentPlan = string.Empty;

                await LoadVisitHistoryAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving visit: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _sharedStateService.CurrentPatientChanged -= OnSharedPatientChanged;
            GC.SuppressFinalize(this);
        }
    }
}
