using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalApp.Models;
using MedicalApp.Services;

namespace MedicalApp.ViewModels
{
    public partial class PatientRegistrationViewModel : ObservableObject
    {
        private readonly IPatientService _patientService;
        private readonly ISharedStateService _sharedStateService;
        private readonly IQueueService _queueService;
        private bool _isAutofilling = false;

        [ObservableProperty]
        private string _searchTerm = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Patient> _patients = new();

        [ObservableProperty]
        private Patient? _selectedPatient;

        // Registration form fields
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private int _age;

        [ObservableProperty]
        private string _gender = "Male";

        [ObservableProperty]
        private string _address = string.Empty;

        [ObservableProperty]
        private string _phone = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Patient> _nameSuggestions = new();

        [ObservableProperty]
        private bool _isSuggestionsOpen = false;

        [ObservableProperty]
        private string _registrationButtonText = "Register & Send to Queue";

        [ObservableProperty]
        private Patient? _activeEditingPatient;

        public PatientRegistrationViewModel(IPatientService patientService, ISharedStateService sharedStateService, IQueueService queueService)
        {
            _patientService = patientService;
            _sharedStateService = sharedStateService;
            _queueService = queueService;
            
            // Sync with current selection
            SelectedPatient = _sharedStateService.CurrentPatient;
            
            // Load initial patients asynchronously
            _ = LoadPatientsAsync();
        }

        // When selection changes, update the shared singleton state
        partial void OnSelectedPatientChanged(Patient? value)
        {
            _sharedStateService.CurrentPatient = value;
        }

        [RelayCommand]
        public async Task LoadPatientsAsync()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                Patients = new ObservableCollection<Patient>(patients);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading patients: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            try
            {
                var results = await _patientService.SearchPatientsAsync(SearchTerm);
                Patients = new ObservableCollection<Patient>(results);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching: {ex.Message}";
            }
        }

        partial void OnNameChanged(string value)
        {
            if (_isAutofilling) return;

            if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
            {
                NameSuggestions.Clear();
                IsSuggestionsOpen = false;
            }
            else
            {
                _ = QueryNameSuggestionsAsync(value);
            }
        }

        private async Task QueryNameSuggestionsAsync(string query)
        {
            try
            {
                var results = await _patientService.SearchPatientsAsync(query);
                var list = System.Linq.Enumerable.ToList(results);
                
                // Show suggestions only if they don't match our active editing patient name
                if (list.Count > 0 && (ActiveEditingPatient == null || ActiveEditingPatient.Name != query))
                {
                    NameSuggestions = new ObservableCollection<Patient>(list);
                    IsSuggestionsOpen = true;
                }
                else
                {
                    NameSuggestions.Clear();
                    IsSuggestionsOpen = false;
                }
            }
            catch
            {
                // Suppress background query errors
            }
        }

        [RelayCommand]
        public void LoadExistingPatient(Patient patient)
        {
            if (patient == null) return;

            _isAutofilling = true;
            try
            {
                Name = patient.Name;
                Age = patient.Age;
                Gender = patient.Gender;
                Address = patient.Address;
                Phone = patient.Phone;
                
                ActiveEditingPatient = patient;
                RegistrationButtonText = "Update & Send to Queue";
                IsSuggestionsOpen = false;
                NameSuggestions.Clear();
                StatusMessage = $"Loaded existing patient details: '{patient.Name}'";
            }
            finally
            {
                _isAutofilling = false;
            }
        }

        [RelayCommand]
        public void CancelEdit()
        {
            _isAutofilling = true;
            try
            {
                Name = string.Empty;
                Age = 0;
                Gender = "Male";
                Address = string.Empty;
                Phone = string.Empty;
                
                ActiveEditingPatient = null;
                RegistrationButtonText = "Register & Send to Queue";
                IsSuggestionsOpen = false;
                NameSuggestions.Clear();
                StatusMessage = "Cleared fields. Switched to new registration.";
            }
            finally
            {
                _isAutofilling = false;
            }
        }

        [RelayCommand]
        public async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                StatusMessage = "Patient Name is required.";
                return;
            }

            try
            {
                if (ActiveEditingPatient != null)
                {
                    // Update existing patient
                    ActiveEditingPatient.Name = Name;
                    ActiveEditingPatient.Age = Age;
                    ActiveEditingPatient.Gender = Gender;
                    ActiveEditingPatient.Address = Address;
                    ActiveEditingPatient.Phone = Phone;

                    await _patientService.UpdatePatientAsync(ActiveEditingPatient);
                    
                    // Add/refresh in daily queue
                    await _queueService.AddToQueueAsync(ActiveEditingPatient.PatientId, ActiveEditingPatient.Name);
                    
                    StatusMessage = $"Patient '{ActiveEditingPatient.Name}' details updated & added to queue!";
                }
                else
                {
                    // Create new patient
                    var patient = new Patient
                    {
                        Name = Name,
                        Age = Age,
                        Gender = Gender,
                        Address = Address,
                        Phone = Phone,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _patientService.AddPatientAsync(patient);
                    
                    // Add registered patient to daily queue automatically
                    await _queueService.AddToQueueAsync(patient.PatientId, patient.Name);
                    
                    StatusMessage = $"Patient '{Name}' registered & added to waitlist queue!";
                }
                
                // Clear Form
                _isAutofilling = true;
                try
                {
                    Name = string.Empty;
                    Age = 0;
                    Address = string.Empty;
                    Phone = string.Empty;
                    ActiveEditingPatient = null;
                    RegistrationButtonText = "Register & Send to Queue";
                    IsSuggestionsOpen = false;
                    NameSuggestions.Clear();
                }
                finally
                {
                    _isAutofilling = false;
                }

                await LoadPatientsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving patient: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task SendToQueueAsync()
        {
            if (SelectedPatient == null)
            {
                StatusMessage = "Please select a patient to queue.";
                return;
            }

            try
            {
                await _queueService.AddToQueueAsync(SelectedPatient.PatientId, SelectedPatient.Name);
                StatusMessage = $"Patient '{SelectedPatient.Name}' added to waitlist queue!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error queueing patient: {ex.Message}";
            }
        }
    }
}
