using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MedicalApp.Services;
using MedicalApp.Models;

namespace MedicalApp.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IThemeService _themeService;
        private readonly IPatientService _patientService;
        private readonly ISharedStateService _sharedStateService;
        private readonly IQueueService _queueService;
        private DispatcherTimer? _pollingTimer;

        public bool IsDarkMode => _themeService.IsDarkMode;

        [ObservableProperty]
        private ObservableCollection<Doctor> _doctors = new();

        [ObservableProperty]
        private Doctor? _selectedDoctor;

        [ObservableProperty]
        private string _doctorPasswordAttempt = string.Empty;

        [ObservableProperty]
        private bool _showDoctorLoginModal = false;

        [ObservableProperty]
        private string _loginErrorMessage = string.Empty;

        // Dashboard Properties
        [ObservableProperty]
        private string? _activeDoctorName;

        [ObservableProperty]
        private bool _isDoctorDashboardVisible = false;

        [ObservableProperty]
        private ObservableCollection<QueueEntry> _waitingPatients = new();

        [ObservableProperty]
        private ObservableCollection<QueueEntry> _notFinishedPatients = new();

        [ObservableProperty]
        private int _waitingCount;

        [ObservableProperty]
        private int _inExamCount;

        [ObservableProperty]
        private int _completedCountToday;

        public bool HasHoldPatients => NotFinishedPatients.Count > 0;
        public bool HasNoHoldPatients => NotFinishedPatients.Count == 0;
        public bool HasWaitingPatients => WaitingPatients.Count > 0;
        public bool HasNoWaitingPatients => WaitingPatients.Count == 0;

        public HomeViewModel(
            IServiceProvider serviceProvider, 
            IThemeService themeService, 
            IPatientService patientService, 
            ISharedStateService sharedStateService,
            IQueueService queueService)
        {
            _serviceProvider = serviceProvider;
            _themeService = themeService;
            _patientService = patientService;
            _sharedStateService = sharedStateService;
            _queueService = queueService;

            System.Windows.WeakEventManager<IThemeService, EventArgs>.AddHandler(_themeService, nameof(IThemeService.ThemeChanged), (s, ev) => OnPropertyChanged(nameof(IsDarkMode)));

            // Sync with shared state
            ActiveDoctorName = _sharedStateService.ActiveDoctorName;
            IsDoctorDashboardVisible = !string.IsNullOrEmpty(ActiveDoctorName);

            SetupTimer();
        }

        private void SetupTimer()
        {
            _pollingTimer = new DispatcherTimer();
            _pollingTimer.Interval = TimeSpan.FromSeconds(2);
            _pollingTimer.Tick += async (s, e) => await PollQueueAsync();
        }

        public void StartPolling()
        {
            if (IsDoctorDashboardVisible)
            {
                int intervalSeconds = 2;
                try
                {
                    var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "print_settings.json");
                    if (System.IO.File.Exists(path))
                    {
                        var json = System.IO.File.ReadAllText(path);
                        var settings = System.Text.Json.JsonSerializer.Deserialize<PrintSettings>(json);
                        if (settings != null)
                        {
                            intervalSeconds = settings.QueuePollingInterval;
                        }
                    }
                }
                catch { /* fallback */ }

                if (intervalSeconds < 1) intervalSeconds = 2;

                if (_pollingTimer != null)
                {
                    _pollingTimer.Interval = TimeSpan.FromSeconds(intervalSeconds);
                    _pollingTimer.Start();
                }
                _ = PollQueueAsync();
            }
        }

        public void StopPolling()
        {
            _pollingTimer?.Stop();
        }

        private async Task PollQueueAsync()
        {
            if (string.IsNullOrEmpty(ActiveDoctorName)) return;
            try
            {
                var activeQueue = await _queueService.GetActiveQueueAsync();
                var completedCount = await _queueService.GetCompletedCountTodayAsync(ActiveDoctorName);

                var doctorQueue = activeQueue.Where(q => q.DoctorName == ActiveDoctorName).ToList();

                var waiting = doctorQueue.Where(q => q.Status == "Pending").ToList();
                var inExam = doctorQueue.Where(q => q.Status == "InExam").ToList();

                WaitingPatients = new ObservableCollection<QueueEntry>(waiting);
                NotFinishedPatients = new ObservableCollection<QueueEntry>(inExam);
                CompletedCountToday = completedCount;

                WaitingCount = waiting.Count;
                InExamCount = inExam.Count;

                OnPropertyChanged(nameof(HasHoldPatients));
                OnPropertyChanged(nameof(HasNoHoldPatients));
                OnPropertyChanged(nameof(HasWaitingPatients));
                OnPropertyChanged(nameof(HasNoWaitingPatients));
            }
            catch
            {
                // Ignore transient background db errors
            }
        }

        [RelayCommand]
        public void ToggleTheme()
        {
            _themeService.ToggleTheme();
        }

        [RelayCommand]
        public void NavigateToRegistry()
        {
            var mainVm = _serviceProvider.GetRequiredService<MainViewModel>();
            mainVm.NavigateToPatientRegistration();
        }

        [RelayCommand]
        public void NavigateToExam()
        {
            if (!string.IsNullOrEmpty(_sharedStateService.ActiveDoctorName) &&
                _sharedStateService.IsDoctorAuthenticated(_sharedStateService.ActiveDoctorName))
            {
                ActiveDoctorName = _sharedStateService.ActiveDoctorName;
                IsDoctorDashboardVisible = true;
                StartPolling();
            }
            else
            {
                _ = OpenDoctorLoginModal();
            }
        }

        [RelayCommand]
        public void ExitDoctorMode()
        {
            StopPolling();
            _sharedStateService.ActiveDoctorName = null;
            ActiveDoctorName = null;
            IsDoctorDashboardVisible = false;
        }

        [RelayCommand]
        public async Task StartSessionAsync(object? parameter)
        {
            if (parameter is not QueueEntry entry) return;
            try
            {
                StopPolling();
                // Set InExam in DB queue
                await _queueService.UpdateQueueStatusAsync(entry.PatientId, "InExam");

                // Get full Patient Model
                var patient = await _patientService.GetPatientByIdAsync(entry.PatientId);
                _sharedStateService.CurrentPatient = patient;

                // Navigate to Clinical Exam
                var mainVm = _serviceProvider.GetRequiredService<MainViewModel>();
                mainVm.NavigateToClinicalExam();
            }
            catch (Exception)
            {
                // Handle or log error
            }
        }

        [RelayCommand]
        public async Task OpenDoctorLoginModal()
        {
            try
            {
                var docList = await _patientService.GetAllDoctorsAsync();
                Doctors.Clear();
                foreach (var doc in docList)
                {
                    Doctors.Add(doc);
                }
                if (Doctors.Count > 0)
                {
                    SelectedDoctor = Doctors[0];
                }
                DoctorPasswordAttempt = string.Empty;
                LoginErrorMessage = string.Empty;
                ShowDoctorLoginModal = true;
            }
            catch (Exception ex)
            {
                LoginErrorMessage = $"Failed to load doctors: {ex.Message}";
            }
        }

        [RelayCommand]
        public void CloseDoctorLoginModal()
        {
            ShowDoctorLoginModal = false;
            DoctorPasswordAttempt = string.Empty;
            LoginErrorMessage = string.Empty;
        }

        [RelayCommand]
        public void ConfirmDoctorLogin()
        {
            if (SelectedDoctor == null)
            {
                LoginErrorMessage = "Please select a doctor | الرجاء اختيار طبيب";
                return;
            }

            bool isAlreadyAuth = _sharedStateService.IsDoctorAuthenticated(SelectedDoctor.Name);

            if (isAlreadyAuth || SelectedDoctor.Password == DoctorPasswordAttempt)
            {
                if (!isAlreadyAuth)
                {
                    _sharedStateService.AuthenticateDoctor(SelectedDoctor.Name);
                }

                _sharedStateService.ActiveDoctorName = SelectedDoctor.Name;
                ActiveDoctorName = SelectedDoctor.Name;
                IsDoctorDashboardVisible = true;
                ShowDoctorLoginModal = false;
                DoctorPasswordAttempt = string.Empty;
                LoginErrorMessage = string.Empty;

                StartPolling();
            }
            else
            {
                LoginErrorMessage = "Incorrect Password! | كلمة المرور غير صحيحة!";
            }
        }
    }
}
