using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalApp.Models;
using MedicalApp.Services;

using Microsoft.Extensions.Configuration;

namespace MedicalApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISharedStateService _sharedStateService;
        private readonly IThemeService _themeService;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private Patient? _selectedPatient;

        [ObservableProperty]
        private string _connectionInfo = "Offline";

        // Use Lazy resolving to optimize startup time and instantiate only on-demand
        private readonly Lazy<HomeViewModel> _homeVm;
        private readonly Lazy<PatientRegistrationViewModel> _patientRegistrationVm;
        private readonly Lazy<ClinicalExamViewModel> _clinicalExamVm;
        private readonly Lazy<PrintSettingsViewModel> _printSettingsVm;
        private object? _lastView;

        public bool IsDarkMode => _themeService.IsDarkMode;

        [RelayCommand]
        public void ToggleTheme()
        {
            _themeService.ToggleTheme();
        }

        public MainViewModel(IServiceProvider serviceProvider, ISharedStateService sharedStateService, IConfiguration configuration, IThemeService themeService)
        {
            _serviceProvider = serviceProvider;
            _sharedStateService = sharedStateService;
            _themeService = themeService;

            System.Windows.WeakEventManager<IThemeService, EventArgs>.AddHandler(_themeService, nameof(IThemeService.ThemeChanged), (s, ev) => OnPropertyChanged(nameof(IsDarkMode)));

            // Parse connection string to display actual server/database source dynamically
            var connString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            if (connString.Contains("Data Source="))
            {
                string dbFile = connString.Split("Data Source=")[1].Split(';')[0];
                ConnectionInfo = $"SQLite ({dbFile})";
            }
            else if (connString.Contains("Server="))
            {
                string server = connString.Split("Server=")[1].Split(';')[0];
                ConnectionInfo = $"Server: {server}";
            }
            else
            {
                ConnectionInfo = "Local DB Mode";
            }

            // Header patient info synchronization
            SelectedPatient = _sharedStateService.CurrentPatient;
            _sharedStateService.CurrentPatientChanged += (patient) => SelectedPatient = patient;

            _homeVm = new Lazy<HomeViewModel>(() => 
                (HomeViewModel)_serviceProvider.GetService(typeof(HomeViewModel))!);
            _patientRegistrationVm = new Lazy<PatientRegistrationViewModel>(() => 
                (PatientRegistrationViewModel)_serviceProvider.GetService(typeof(PatientRegistrationViewModel))!);
            _clinicalExamVm = new Lazy<ClinicalExamViewModel>(() => 
                (ClinicalExamViewModel)_serviceProvider.GetService(typeof(ClinicalExamViewModel))!);
            _printSettingsVm = new Lazy<PrintSettingsViewModel>(() => 
                (PrintSettingsViewModel)_serviceProvider.GetService(typeof(PrintSettingsViewModel))!);

            // Default view will be overridden by App.xaml.cs, but fallback to PatientRegistration
            NavigateToPatientRegistration();
        }

        partial void OnCurrentViewChanged(object? value)
        {
            if (_lastView is ClinicalExamViewModel oldExamVm)
            {
                oldExamVm.StopPolling();
            }

            if (_lastView is HomeViewModel oldHomeVm)
            {
                oldHomeVm.StopPolling();
            }

            if (value is ClinicalExamViewModel newExamVm)
            {
                newExamVm.StartPolling();
            }

            if (value is HomeViewModel newHomeVm)
            {
                newHomeVm.StartPolling();
            }

            _lastView = value;

            OnPropertyChanged(nameof(IsHomeActive));
            OnPropertyChanged(nameof(IsPatientRegistryActive));
            OnPropertyChanged(nameof(IsClinicalExamActive));
            OnPropertyChanged(nameof(IsPrintSettingsActive));
        }

        public bool IsHomeActive => CurrentView is HomeViewModel || CurrentView is PrintSettingsViewModel; // Keep sidebar collapsed during settings as well
        public bool IsPatientRegistryActive => CurrentView is PatientRegistrationViewModel;
        public bool IsClinicalExamActive => CurrentView is ClinicalExamViewModel;
        public bool IsPrintSettingsActive => CurrentView is PrintSettingsViewModel;

        [RelayCommand]
        public void NavigateToPrintSettings()
        {
            CurrentView = _printSettingsVm.Value;
        }

        [RelayCommand]
        public void NavigateToHome()
        {
            CurrentView = _homeVm.Value;
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
    }
}
