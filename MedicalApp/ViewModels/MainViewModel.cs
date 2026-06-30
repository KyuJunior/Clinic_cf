using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalApp.Models;
using MedicalApp.Services;

namespace MedicalApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISharedStateService _sharedStateService;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private Patient? _selectedPatient;

        // Use Lazy resolving to optimize startup time and instantiate only on-demand
        private readonly Lazy<PatientRegistrationViewModel> _patientRegistrationVm;
        private readonly Lazy<ClinicalExamViewModel> _clinicalExamVm;
        private readonly Lazy<EchoUploadViewModel> _echoUploadVm;

        public MainViewModel(IServiceProvider serviceProvider, ISharedStateService sharedStateService)
        {
            _serviceProvider = serviceProvider;
            _sharedStateService = sharedStateService;

            // Header patient info synchronization
            SelectedPatient = _sharedStateService.CurrentPatient;
            _sharedStateService.CurrentPatientChanged += (patient) => SelectedPatient = patient;

            _patientRegistrationVm = new Lazy<PatientRegistrationViewModel>(() => 
                (PatientRegistrationViewModel)_serviceProvider.GetService(typeof(PatientRegistrationViewModel))!);
            _clinicalExamVm = new Lazy<ClinicalExamViewModel>(() => 
                (ClinicalExamViewModel)_serviceProvider.GetService(typeof(ClinicalExamViewModel))!);
            _echoUploadVm = new Lazy<EchoUploadViewModel>(() => 
                (EchoUploadViewModel)_serviceProvider.GetService(typeof(EchoUploadViewModel))!);

            // Set default view on load
            NavigateToPatientRegistration();
        }

        [RelayCommand]
        public void NavigateToPatientRegistration()
        {
            CurrentView = _patientRegistrationVm.Value;
        }

        [RelayCommand]
        public void NavigateToClinicalExam()
        {
            CurrentView = _clinicalExamVm.Value;
        }

        [RelayCommand]
        public void NavigateToEchoUpload()
        {
            CurrentView = _echoUploadVm.Value;
        }
    }
}
