using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalApp.Models;
using MedicalApp.Services;

using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace MedicalApp.ViewModels
{
    public partial class ClinicalExamViewModel : ObservableObject, IDisposable
    {
        private readonly IVisitService _visitService;
        private readonly IPatientService _patientService;
        private readonly ISharedStateService _sharedStateService;
        private readonly IQueueService _queueService;
        private readonly IPrintService _printService;
        private readonly IDbContextFactory<Data.AppDbContext> _dbContextFactory;
        private readonly System.Windows.Threading.DispatcherTimer _pollingTimer;
        private readonly IThemeService _themeService;
        private readonly IScannerService _scannerService;

        public bool IsDarkMode => _themeService.IsDarkMode;

        [ObservableProperty]
        private string _activeDoctorName = "Dr. Yaser";

        [ObservableProperty]
        private ObservableCollection<Doctor> _doctors = new();

        [ObservableProperty]
        private bool _showSwitchDoctorPasswordModal = false;

        [ObservableProperty]
        private string _switchDoctorPasswordAttempt = string.Empty;

        [ObservableProperty]
        private Doctor? _selectedSwitchDoctor = null;

        [ObservableProperty]
        private string _switchDoctorErrorMessage = string.Empty;

        [ObservableProperty]
        private bool _showAddPatientModal = false;

        [ObservableProperty]
        private string _newPatientName = string.Empty;

        [ObservableProperty]
        private int _newPatientAge = 0;

        [ObservableProperty]
        private int _newPatientAgeMonths = 0;

        [ObservableProperty]
        private string _newPatientGender = "ذكر";

        [ObservableProperty]
        private string _newPatientPhone = string.Empty;

        [ObservableProperty]
        private DateTime? _newPatientBirthDate = null;

        [ObservableProperty]
        private string _newPatientGovernorate = "بغداد";

        [ObservableProperty]
        private string _newPatientFiles = string.Empty;

        [ObservableProperty]
        private bool _newPatientIsPaidVisit = true;

        [ObservableProperty]
        private bool _newPatientIsFreeVisit = false;

        [ObservableProperty]
        private string _newPatientVisitPrice = "25000";

        [ObservableProperty]
        private string _addPatientValidationMessage = string.Empty;

        public string NewPatientFileName => string.IsNullOrEmpty(NewPatientFiles) ? "لا يوجد ملف" : System.IO.Path.GetFileName(NewPatientFiles);

        partial void OnNewPatientFilesChanged(string value)
        {
            OnPropertyChanged(nameof(NewPatientFileName));
        }

        [RelayCommand]
        public void ToggleTheme()
        {
            _themeService.ToggleTheme();
        }

        [RelayCommand]
        public void OpenStartNewVisitWindow()
        {
            var window = new MedicalApp.Views.StartNewVisitWindow(_patientService)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            
            if (window.ShowDialog() == true)
            {
                _ = PollQueueAsync();
            }
        }

        private string DraftsFile
        {
            get
            {
                var suffix = string.IsNullOrEmpty(ActiveDoctorName) ? "default" : ActiveDoctorName.Replace(" ", "_").Replace(".", "");
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"session_drafts_{suffix}.json");
            }
        }
        private bool _isSavingOrLoadingDraft;

        [ObservableProperty]
        private Patient? _currentPatient;

        [ObservableProperty]
        private ObservableCollection<Visit> _visitHistory = new();

        [ObservableProperty]
        private ObservableCollection<QueueEntry> _activeQueue = new();

        [ObservableProperty]
        private ObservableCollection<QueueEntry> _waitingPatients = new();

        [ObservableProperty]
        private ObservableCollection<QueueEntry> _notFinishedPatients = new();

        [ObservableProperty]
        private int _completedCountToday;

        [ObservableProperty]
        private bool _isQueueTabActive = true;

        [ObservableProperty]
        private bool _isSoonTabActive;

        // Standalone Patient Lookup fields
        [ObservableProperty]
        private string _searchTerm = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Patient> _patients = new();

        [ObservableProperty]
        private Patient? _selectedPatientLookup;

        // Visit Form Fields
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasChiefComplaint))]
        private string _chiefComplaint = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasHPI))]
        private string _historyOfPresentIllness = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPhysicalExam))]
        private string _physicalExamination = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasDiagnosis))]
        private string _diagnosis = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasTreatmentPlan))]
        private string _treatmentPlan = string.Empty;

        [ObservableProperty]
        private string _drugSearchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _drugSuggestions = new();

        [ObservableProperty]
        private bool _isDrugSuggestionsOpen;

        [ObservableProperty]
        private ObservableCollection<PrescribedMedication> _prescribedDrugs = new();

        public bool HasNoPrescribedDrugs => PrescribedDrugs == null || PrescribedDrugs.Count == 0;
        public bool HasPrescribedDrugs => PrescribedDrugs != null && PrescribedDrugs.Count > 0;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        // Sidebar State
        [ObservableProperty]
        private bool _isSidebarCollapsed = false;

        // Auto-save indicator
        [ObservableProperty]
        private bool _showSavedBadge = false;

        // Patient panel computed helpers
        public string PatientInitials => CurrentPatient != null
            ? string.Join("", CurrentPatient.Name?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2).Select(w => w[0].ToString().ToUpper()) ?? Array.Empty<string>())
            : "?";

        public string PatientVisitDate => DateTime.Now.ToString("MMMM dd, yyyy");

        // Computed indicators for sidebar badge dots
        public bool HasChiefComplaint => !string.IsNullOrWhiteSpace(ChiefComplaint);
        public bool HasHPI => !string.IsNullOrWhiteSpace(HistoryOfPresentIllness);
        public bool HasPhysicalExam => !string.IsNullOrWhiteSpace(PhysicalExamination) || 
                                       !string.IsNullOrWhiteSpace(PhysicalExamPositive) || 
                                       !string.IsNullOrWhiteSpace(PhysicalExamNegative);
        public bool HasProcedures => PerformedProcedures != null && PerformedProcedures.Count > 0;
        public bool HasDiagnosis => !string.IsNullOrWhiteSpace(Diagnosis);
        public bool HasTreatmentPlan => !string.IsNullOrWhiteSpace(TreatmentPlan);
        public bool HasVitals => !string.IsNullOrWhiteSpace(VitalHR) ||
                                 !string.IsNullOrWhiteSpace(VitalSBP) ||
                                 !string.IsNullOrWhiteSpace(VitalDBP) ||
                                 !string.IsNullOrWhiteSpace(VitalRR) ||
                                 !string.IsNullOrWhiteSpace(VitalSPO2) ||
                                 !string.IsNullOrWhiteSpace(VitalTemp);
        public bool HasPrescription => (PrescribedDrugs != null && PrescribedDrugs.Count > 0) || !string.IsNullOrWhiteSpace(MedicalInstructions);
        public bool HasInvestigations => AddedInvestigations != null && AddedInvestigations.Count > 0;
        public bool HasImaging => AddedImagings != null && AddedImagings.Count > 0;
        public bool HasHistory => CurrentPatient != null && (
                                  !string.IsNullOrWhiteSpace(CurrentPatient.Allergy) ||
                                  !string.IsNullOrWhiteSpace(CurrentPatient.Smoking) ||
                                  !string.IsNullOrWhiteSpace(CurrentPatient.Alcohol) ||
                                  !string.IsNullOrWhiteSpace(CurrentPatient.PastMedicalHistory) ||
                                  !string.IsNullOrWhiteSpace(CurrentPatient.PastSurgicalHistory) ||
                                  !string.IsNullOrWhiteSpace(CurrentPatient.PastDrugHistory) ||
                                  !string.IsNullOrWhiteSpace(CurrentPatient.PastFamilyHistory) ||
                                  !string.IsNullOrWhiteSpace(CurrentPatient.SmokingCigarettesPerDay) ||
                                  !string.IsNullOrWhiteSpace(CurrentPatient.SmokingYears) ||
                                  !string.IsNullOrWhiteSpace(CurrentPatient.AlcoholType) ||
                                  !string.IsNullOrWhiteSpace(CurrentPatient.AlcoholConcentration) ||
                                  !string.IsNullOrWhiteSpace(CurrentPatient.AlcoholVolume));

        public bool HasObstetrics => !string.IsNullOrEmpty(ObAbortionNote) || !string.IsNullOrEmpty(ObLastUltrasoundNote);
        public bool HasSystemReview => false;
        public bool HasNutrition => !string.IsNullOrEmpty(NutritionalWeight) || !string.IsNullOrEmpty(NutritionalHeight);
        public bool HasDentistNotes => false;
        public bool HasVisitSummary => false;

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
        }

        // View Toggling Settings (Show/Hide sections)
        // Vital Signs Fields
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasVitals))]
        private string _vitalHR = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasVitals))]
        private string _vitalSBP = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasVitals))]
        private string _vitalDBP = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasVitals))]
        private string _vitalRR = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasVitals))]
        private string _vitalSPO2 = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasVitals))]
        private string _vitalTemp = string.Empty;



        [ObservableProperty]
        private bool _isVitallyStable;

        // Investigation & Imaging Fields
        [ObservableProperty]
        private string _selectedInvestigation = string.Empty;

        [ObservableProperty]
        private string _selectedImaging = string.Empty;

        [ObservableProperty]
        private DateTime? _returnDate;

        [ObservableProperty]
        private string _investigationAttachmentPath = string.Empty;

        [ObservableProperty]
        private string _imagingAttachmentPath = string.Empty;

        public string InvestigationAttachmentName => string.IsNullOrWhiteSpace(InvestigationAttachmentPath) 
            ? string.Empty 
            : Path.GetFileName(InvestigationAttachmentPath);

        public string ImagingAttachmentName => string.IsNullOrWhiteSpace(ImagingAttachmentPath) 
            ? string.Empty 
            : Path.GetFileName(ImagingAttachmentPath);

        [ObservableProperty]
        private ObservableCollection<ClinicalAttachment> _addedInvestigations = new();

        [ObservableProperty]
        private ObservableCollection<ClinicalAttachment> _addedImagings = new();

        [ObservableProperty] private int _complaintsRow = 0;
        [ObservableProperty] private int _vitalsRow = 1;
        [ObservableProperty] private int _examinationRow = 2;
        [ObservableProperty] private int _hpiRow = 3;
        [ObservableProperty] private int _proceduresRow = 4;
        [ObservableProperty] private int _diagnosisRow = 5;
        [ObservableProperty] private int _prescriptionRow = 6;
        [ObservableProperty] private int _investigationsRow = 7;
        [ObservableProperty] private int _imagingRow = 8;
        [ObservableProperty] private int _obstetricsRow = 9;
        [ObservableProperty] private int _systemReviewRow = 10;
        [ObservableProperty] private int _nutritionRow = 11;
        [ObservableProperty] private int _historyRow = 12;
        [ObservableProperty] private int _dentistNotesRow = 13;
        [ObservableProperty] private int _visitSummaryRow = 14;
        [ObservableProperty] private int _treatmentPlanRow = 15;

        [ObservableProperty] private bool _showComplaint = true;
        [ObservableProperty] private bool _showVitals = true;
        [ObservableProperty] private bool _showExamination = true;
        [ObservableProperty] private bool _showPlan = true;
        [ObservableProperty] private bool _showHpi = true;
        [ObservableProperty] private bool _showProcedures = true;
        [ObservableProperty] private bool _showDiagnosis = true;
        [ObservableProperty] private bool _showPrescription = true;
        [ObservableProperty] private bool _showInvestigations = true;
        [ObservableProperty] private bool _showImaging = true;
        [ObservableProperty] private bool _showObstetrics = true;
        [ObservableProperty] private bool _showSystemReview = true;
        [ObservableProperty] private bool _showNutrition = true;
        [ObservableProperty] private bool _showHistory = true;
        [ObservableProperty] private bool _showDentistNotes = true;
        [ObservableProperty] private bool _showVisitSummary = true;

        public ObservableCollection<SectionOrderItem> SectionOrderList { get; } = new();

        [ObservableProperty]
        private ObservableCollection<PerformedProcedure> _performedProcedures = new();

        [ObservableProperty]
        private string _selectedProcedureName = string.Empty;

        [ObservableProperty]
        private string _procedureCostText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _procedureSuggestions = new()
        {
            "Ultrasound (Sounar) / سونار",
            "ECG / تخطيط قلب",
            "Wound Dressing / غيار جرح",
            "Nebulizer / جهاز تبخير",
            "Minor Stitching / خياطة جرح",
            "Injection / زرق إبرة"
        };

        [ObservableProperty]
        private string _physicalExamPositive = string.Empty;

        [ObservableProperty]
        private string _physicalExamNegative = string.Empty;

        [ObservableProperty]
        private string _medicalInstructions = string.Empty;

        [ObservableProperty]
        private ObservableCollection<InstructionPreset> _instructionPresetsList = new();

        [ObservableProperty]
        private InstructionPreset? _selectedInstructionPreset;

        [ObservableProperty] private string _pastMedicalHistory = string.Empty;
        [ObservableProperty] private string _pastSurgicalHistory = string.Empty;
        [ObservableProperty] private string _pastDrugHistory = string.Empty;
        [ObservableProperty] private string _pastFamilyHistory = string.Empty;
        [ObservableProperty] private string _smokingCigarettesPerDay = string.Empty;
        [ObservableProperty] private string _smokingYears = string.Empty;
        [ObservableProperty] private string _alcoholType = string.Empty;
        [ObservableProperty] private string _alcoholConcentration = string.Empty;
        [ObservableProperty] private string _alcoholVolume = string.Empty;


        [ObservableProperty]
        private ObservableCollection<string> _investigationList = new()
        {
            "CBC (Complete Blood Count)",
            "HbA1c (Glycated Hemoglobin)",
            "Lipid Profile (Cholesterol)",
            "Kidney Function Test (KFT)",
            "Liver Function Test (LFT)",
            "Thyroid Profile (TSH)",
            "Urine Analysis",
            "None"
        };

        [ObservableProperty]
        private ObservableCollection<string> _imagingList = new()
        {
            "Echocardiography",
            "Chest X-Ray",
            "Electrocardiogram (ECG)",
            "Abdominal Ultrasound",
            "Cardiac MRI",
            "CT Angiography",
            "None"
        };

        public ClinicalExamViewModel(
            IVisitService visitService, 
            IPatientService patientService, 
            ISharedStateService sharedStateService, 
            IQueueService queueService,
            IPrintService printService,
            IDbContextFactory<Data.AppDbContext> dbContextFactory,
            IThemeService themeService,
            IScannerService scannerService)
        {
            _visitService = visitService;
            _patientService = patientService;
            _sharedStateService = sharedStateService;
            _queueService = queueService;
            _printService = printService;
            _dbContextFactory = dbContextFactory;
            _themeService = themeService;
            _scannerService = scannerService;

            System.Windows.WeakEventManager<IThemeService, EventArgs>.AddHandler(_themeService, nameof(IThemeService.ThemeChanged), (s, ev) => OnPropertyChanged(nameof(IsDarkMode)));

            _prescribedDrugs.CollectionChanged += (s, e) => {
                TriggerAutoSave();
                OnPropertyChanged(nameof(HasNoPrescribedDrugs));
                OnPropertyChanged(nameof(HasPrescribedDrugs));
                OnPropertyChanged(nameof(HasPrescription));
            };
            _addedInvestigations.CollectionChanged += (s, e) => {
                TriggerAutoSave();
                OnPropertyChanged(nameof(HasInvestigations));
            };
            _addedImagings.CollectionChanged += (s, e) => {
                TriggerAutoSave();
                OnPropertyChanged(nameof(HasImaging));
            };

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

            // Set up 2-second queue polling timer
            // Set up queue polling timer using dynamic interval from settings
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

            _pollingTimer = new System.Windows.Threading.DispatcherTimer();
            _pollingTimer.Interval = TimeSpan.FromSeconds(intervalSeconds);
            _pollingTimer.Tick += OnPollingTimerTick;
            _pollingTimer.Start();
            if (!string.IsNullOrWhiteSpace(_sharedStateService.ActiveDoctorName))
            {
                _activeDoctorName = _sharedStateService.ActiveDoctorName;
            }

            _ = PollQueueAsync();

            // Load doctors
            _ = LoadDoctorsAsync();

            InitializeSectionsOrder();
            _ = LoadInstructionPresetsAsync();
            _ = LoadDatabaseSuggestionsAsync();
        }

        private async void OnPollingTimerTick(object? sender, EventArgs e)
        {
            _pollingTimer.Stop();
            await PollQueueAsync();
            _pollingTimer.Start();
        }

        private async Task PollQueueAsync()
        {
            try
            {
                var activeTask = _queueService.GetActiveQueueAsync();
                var completedTask = _queueService.GetCompletedCountTodayAsync(ActiveDoctorName);
                await Task.WhenAll(activeTask, completedTask);

                var active = activeTask.Result.Where(q => q.DoctorName == ActiveDoctorName).ToList();
                ActiveQueue = new ObservableCollection<QueueEntry>(active);
                WaitingPatients = new ObservableCollection<QueueEntry>(active.Where(q => q.Status == "Pending"));
                NotFinishedPatients = new ObservableCollection<QueueEntry>(active.Where(q => q.Status == "InExam" || q.Status == "HoldExam"));
                CompletedCountToday = completedTask.Result;
            }
            catch
            {
                // Ignore background transient query errors
            }
        }

        [RelayCommand]
        public void SwitchActiveDoctor(string doctorName)
        {
            if (string.IsNullOrWhiteSpace(doctorName)) return;

            // Find the doctor object
            foreach (var doc in Doctors)
            {
                if (doc.Name == doctorName)
                {
                    if (_sharedStateService.IsDoctorAuthenticated(doc.Name))
                    {
                        ExitToDashboard();
                        ActiveDoctorName = doc.Name;
                        _sharedStateService.ActiveDoctorName = doc.Name;
                        _ = PollQueueAsync();
                        return;
                    }

                    SelectedSwitchDoctor = doc;
                    SwitchDoctorPasswordAttempt = string.Empty;
                    SwitchDoctorErrorMessage = string.Empty;
                    ShowSwitchDoctorPasswordModal = true;
                    return;
                }
            }
        }

        [RelayCommand]
        public void ConfirmSwitchDoctorPassword()
        {
            if (SelectedSwitchDoctor == null) return;

            if (SelectedSwitchDoctor.Password == SwitchDoctorPasswordAttempt)
            {
                _sharedStateService.AuthenticateDoctor(SelectedSwitchDoctor.Name);

                ExitToDashboard();
                ActiveDoctorName = SelectedSwitchDoctor.Name;
                _sharedStateService.ActiveDoctorName = SelectedSwitchDoctor.Name;
                ShowSwitchDoctorPasswordModal = false;
                SwitchDoctorPasswordAttempt = string.Empty;
                SwitchDoctorErrorMessage = string.Empty;
                _ = PollQueueAsync();
            }
            else
            {
                SwitchDoctorErrorMessage = "Incorrect Password! | كلمة المرور غير صحيحة!";
            }
        }

        [RelayCommand]
        public void CancelSwitchDoctorPassword()
        {
            ShowSwitchDoctorPasswordModal = false;
            SelectedSwitchDoctor = null;
            SwitchDoctorPasswordAttempt = string.Empty;
            SwitchDoctorErrorMessage = string.Empty;
        }

        public async Task LoadDoctorsAsync()
        {
            try
            {
                var docList = await _patientService.GetAllDoctorsAsync();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Doctors.Clear();
                    foreach (var doc in docList)
                    {
                        Doctors.Add(doc);
                    }
                });
            }
            catch
            {
                // Ignore transient db errors
            }
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

        partial void OnCurrentPatientChanged(Patient? value)
        {
            if (value != null)
            {
                _ = LoadDraftForPatientAsync(value.PatientId);
            }
            else
            {
                ClearFormFieldsWithoutAutoSave();
            }
            OnPropertyChanged(nameof(ShowObstetricsCard));
            OnPropertyChanged(nameof(HasHistory));
            OnPropertyChanged(nameof(PatientAllergy));
            OnPropertyChanged(nameof(PatientSmoking));
            OnPropertyChanged(nameof(PatientAlcohol));
        }

        async partial void OnSearchTermChanged(string value)
        {
            await SearchPatientsAsync();
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
                var doctorVisits = visits.Where(v => string.Equals(v.DoctorName, ActiveDoctorName, StringComparison.OrdinalIgnoreCase)).ToList();
                VisitHistory = new ObservableCollection<Visit>(doctorVisits);
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

            SyncPhysicalExamination();
            SyncTreatmentPlan();

            if (string.IsNullOrWhiteSpace(ChiefComplaint) && string.IsNullOrWhiteSpace(Diagnosis))
            {
                StatusMessage = "Chief Complaint or Diagnosis is required to log a visit.";
                return;
            }

            try
            {
                var rxText = string.Join(Environment.NewLine, PrescribedDrugs.Select(d => d.ToString()));

                using (var db = await _dbContextFactory.CreateDbContextAsync())
                {
                    // Update patient demographics/history in DB
                    var patientInDb = await db.Patients.FirstOrDefaultAsync(p => p.PatientId == CurrentPatient.PatientId);
                    if (patientInDb != null)
                    {
                        patientInDb.Notes = CurrentPatient.Notes ?? string.Empty;
                        patientInDb.Allergy = CurrentPatient.Allergy ?? string.Empty;
                        patientInDb.Smoking = CurrentPatient.Smoking ?? string.Empty;
                        patientInDb.Alcohol = CurrentPatient.Alcohol ?? string.Empty;
                        patientInDb.BloodGroup = CurrentPatient.BloodGroup ?? string.Empty;

                        patientInDb.PastMedicalHistory = PastMedicalHistory ?? string.Empty;
                        patientInDb.PastSurgicalHistory = PastSurgicalHistory ?? string.Empty;
                        patientInDb.PastDrugHistory = PastDrugHistory ?? string.Empty;
                        patientInDb.PastFamilyHistory = PastFamilyHistory ?? string.Empty;
                        patientInDb.SmokingCigarettesPerDay = SmokingCigarettesPerDay ?? string.Empty;
                        patientInDb.SmokingYears = SmokingYears ?? string.Empty;
                        patientInDb.AlcoholType = AlcoholType ?? string.Empty;
                        patientInDb.AlcoholConcentration = AlcoholConcentration ?? string.Empty;
                        patientInDb.AlcoholVolume = AlcoholVolume ?? string.Empty;

                        db.Patients.Update(patientInDb);
                    }

                    var today = DateTime.UtcNow.Date;
                    var existingVisit = await db.Visits
                        .FirstOrDefaultAsync(v => v.PatientId == CurrentPatient.PatientId && v.VisitDate >= today);

                    var proceduresText = string.Join(";", PerformedProcedures.Select(p => $"{p.Name}|{p.Cost}"));

                    if (existingVisit != null)
                    {
                        existingVisit.ChiefComplaint = ChiefComplaint;
                        existingVisit.HistoryOfPresentIllness = HistoryOfPresentIllness;
                        existingVisit.PhysicalExamination = PhysicalExamination;
                        existingVisit.Diagnosis = Diagnosis;
                        existingVisit.TreatmentPlan = TreatmentPlan;
                        existingVisit.Prescription = rxText;
                        existingVisit.VitalsHR = VitalHR;
                        existingVisit.VitalsSBP = VitalSBP;
                        existingVisit.VitalsDBP = VitalDBP;
                        existingVisit.VitalsRR = VitalRR;
                        existingVisit.VitalsSPO2 = VitalSPO2;
                        existingVisit.VitalsTemp = VitalTemp;
                        existingVisit.Investigation = string.Join(", ", AddedInvestigations.Select(x => x.Name));
                        existingVisit.Imaging = string.Join(", ", AddedImagings.Select(x => x.Name));
                        existingVisit.ReturnDate = ReturnDate;
                        existingVisit.InvestigationAttachmentPath = string.Join(";", AddedInvestigations.Select(x => x.AttachmentPath));
                        existingVisit.ImagingAttachmentPath = string.Join(";", AddedImagings.Select(x => x.AttachmentPath));

                        existingVisit.ProceduresPerformed = proceduresText;
                        existingVisit.PhysicalExamPositive = PhysicalExamPositive ?? string.Empty;
                        existingVisit.PhysicalExamNegative = PhysicalExamNegative ?? string.Empty;
                        existingVisit.MedicalInstructions = MedicalInstructions ?? string.Empty;

                        db.Visits.Update(existingVisit);
                    }
                    else
                    {
                        var visit = new Visit
                        {
                            PatientId = CurrentPatient.PatientId,
                            ChiefComplaint = ChiefComplaint,
                            HistoryOfPresentIllness = HistoryOfPresentIllness,
                            PhysicalExamination = PhysicalExamination,
                            Diagnosis = Diagnosis,
                            TreatmentPlan = TreatmentPlan,
                            Prescription = rxText,
                            VitalsHR = VitalHR,
                            VitalsSBP = VitalSBP,
                            VitalsDBP = VitalDBP,
                            VitalsRR = VitalRR,
                            VitalsSPO2 = VitalSPO2,
                            VitalsTemp = VitalTemp,
                            Investigation = string.Join(", ", AddedInvestigations.Select(x => x.Name)),
                            Imaging = string.Join(", ", AddedImagings.Select(x => x.Name)),
                            ReturnDate = ReturnDate,
                            InvestigationAttachmentPath = string.Join(";", AddedInvestigations.Select(x => x.AttachmentPath)),
                            ImagingAttachmentPath = string.Join(";", AddedImagings.Select(x => x.AttachmentPath)),
                            ProceduresPerformed = proceduresText,
                            PhysicalExamPositive = PhysicalExamPositive ?? string.Empty,
                            PhysicalExamNegative = PhysicalExamNegative ?? string.Empty,
                            MedicalInstructions = MedicalInstructions ?? string.Empty,
                            VisitDate = DateTime.UtcNow
                        };
                        await db.Visits.AddAsync(visit);
                    }
                    await db.SaveChangesAsync();
                }

                // Save new drugs to drug dictionary for autocomplete
                using (var db = await _dbContextFactory.CreateDbContextAsync())
                {
                    foreach (var drug in PrescribedDrugs)
                    {
                        var exists = await db.Drugs.AnyAsync(d => d.Name == drug.Name);
                        if (!exists)
                        {
                            db.Drugs.Add(new Drug { Name = drug.Name });
                        }
                    }
                    await db.SaveChangesAsync();
                }

                // Delete local draft
                await DeleteDraftForPatientAsync(CurrentPatient.PatientId);

                StatusMessage = "Visit log saved successfully!";

                // Clear Form Fields
                ClearFormFieldsWithoutAutoSave();

                await LoadVisitHistoryAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving visit: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task StartSessionAsync(object? parameter)
        {
            if (parameter is not QueueEntry entry) return;
            try
            {
                // Update queue status to InExam
                await _queueService.UpdateQueueStatusAsync(entry.PatientId, "InExam");

                // Get patient from DB
                var patient = await _patientService.GetPatientByIdAsync(entry.PatientId);
                SelectedPatientLookup = patient;
                StatusMessage = $"Exam session started for {entry.PatientName}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error starting exam session: {ex.Message}";
            }
        }

        public async Task MoveToQueueStatusAsync(QueueEntry entry, string newStatus)
        {
            if (entry == null || string.IsNullOrEmpty(newStatus)) return;
            try
            {
                await _queueService.UpdateQueueStatusAsync(entry.PatientId, newStatus);
                StatusMessage = $"Patient {entry.PatientName} status changed to {newStatus}.";
                await PollQueueAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error moving patient: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task CompleteSessionDoneAsync()
        {
            if (CurrentPatient == null)
            {
                StatusMessage = "No active patient to complete.";
                return;
            }

            try
            {
                // Save visit first if complaint/diagnosis is populated
                if (!string.IsNullOrWhiteSpace(ChiefComplaint) || !string.IsNullOrWhiteSpace(Diagnosis))
                {
                    await SaveVisitAsync();
                }

                // Set queue entry as Completed
                await _queueService.CompleteQueueEntryAsync(CurrentPatient.PatientId);
                StatusMessage = $"Exam session for '{CurrentPatient.Name}' completed and removed from queue.";

                // Exit to dashboard (clears state and navigates home)
                ExitToDashboard();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error completing exam session: {ex.Message}";
            }
        }

        [RelayCommand]
        public void ExitToDashboard()
        {
            if (CurrentPatient != null)
            {
                var patientId = CurrentPatient.PatientId;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _queueService.UpdateQueueStatusAsync(patientId, "HoldExam");
                    }
                    catch { }
                });
            }

            SelectedPatientLookup = null;
            CurrentPatient = null;
            _sharedStateService.CurrentPatient = null; // Clear shared state
            VisitHistory.Clear();
            ClearFormFieldsWithoutAutoSave();

            // Navigate back to Home view
            var mainVm = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<MainViewModel>(App.ServiceProvider);
            mainVm.NavigateToHome();
        }

        [RelayCommand]
        public async Task HoldSessionAsync()
        {
            if (CurrentPatient == null)
            {
                StatusMessage = "No active patient to hold.";
                return;
            }

            try
            {
                // Save current visit details first if any complaint/diagnosis is entered
                if (!string.IsNullOrWhiteSpace(ChiefComplaint) || !string.IsNullOrWhiteSpace(Diagnosis))
                {
                    await SaveVisitAsync();
                }

                // Explicitly update queue status to HoldExam (on hold)
                await _queueService.UpdateQueueStatusAsync(CurrentPatient.PatientId, "HoldExam");
                StatusMessage = $"Exam session for '{CurrentPatient.Name}' put on hold (Not Finished).";

                // Exit to dashboard (clears state and navigates home)
                ExitToDashboard();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error holding exam session: {ex.Message}";
            }
        }

        [RelayCommand]
        public void AppendText(string targetAndValue)
        {
            if (string.IsNullOrEmpty(targetAndValue)) return;
            var parts = targetAndValue.Split(':');
            if (parts.Length < 2) return;
            
            var target = parts[0];
            var val = parts[1];
            
            if (target == "Complaint")
            {
                ChiefComplaint = string.IsNullOrWhiteSpace(ChiefComplaint) ? val : ChiefComplaint + ", " + val;
            }
            else if (target == "HPI")
            {
                HistoryOfPresentIllness = string.IsNullOrWhiteSpace(HistoryOfPresentIllness) ? val : HistoryOfPresentIllness + ", " + val;
            }
            else if (target == "Exam")
            {
                PhysicalExamination = string.IsNullOrWhiteSpace(PhysicalExamination) ? val : PhysicalExamination + ", " + val;
            }
            else if (target == "Plan")
            {
                TreatmentPlan = string.IsNullOrWhiteSpace(TreatmentPlan) ? val : TreatmentPlan + ", " + val;
            }
            else if (target == "Dx")
            {
                Diagnosis = string.IsNullOrWhiteSpace(Diagnosis) ? val : Diagnosis + ", " + val;
            }
        }

        // SPO2 Alert
        public string SPO2AlertText
        {
            get
            {
                if (double.TryParse(VitalSPO2, out double val))
                {
                    if (val < 95) return "Low / منخفض";
                }
                return string.Empty;
            }
        }
        public string SPO2AlertBackground => "#EF4444"; // Red
        public bool IsSPO2AlertVisible => !string.IsNullOrWhiteSpace(VitalSPO2) && double.TryParse(VitalSPO2, out double val) && val < 95;

        // Temp Alert
        public string TempAlertText
        {
            get
            {
                if (double.TryParse(VitalTemp, out double val))
                {
                    if (val > 37.5) return "Fever / حرارة";
                    if (val < 35.0) return "Low / منخفض";
                }
                return string.Empty;
            }
        }
        public string TempAlertBackground => (TempAlertText ?? "").StartsWith("Fever") ? "#EF4444" : "#F59E0B";
        public bool IsTempAlertVisible => !string.IsNullOrWhiteSpace(VitalTemp) && double.TryParse(VitalTemp, out double val) && (val > 37.5 || val < 35.0);

        // HR Alert
        public string HRAlertText
        {
            get
            {
                if (double.TryParse(VitalHR, out double val))
                {
                    if (val > 100) return "High / مرتفع";
                    if (val < 60) return "Low / منخفض";
                }
                return string.Empty;
            }
        }
        public string HRAlertBackground => "#EF4444";
        public bool IsHRAlertVisible => !string.IsNullOrWhiteSpace(VitalHR) && double.TryParse(VitalHR, out double val) && (val > 100 || val < 60);

        // RR Alert
        public string RRAlertText
        {
            get
            {
                if (double.TryParse(VitalRR, out double val))
                {
                    if (val > 20) return "High / مرتفع";
                    if (val < 12) return "Low / منخفض";
                }
                return string.Empty;
            }
        }
        public string RRAlertBackground => "#EF4444";
        public bool IsRRAlertVisible => !string.IsNullOrWhiteSpace(VitalRR) && double.TryParse(VitalRR, out double val) && (val > 20 || val < 12);

        // BP Alert (SBP/DBP combined status)
        public string BPAlertText
        {
            get
            {
                double sbp = 0, dbp = 0;
                bool hasSbp = double.TryParse(VitalSBP, out sbp);
                bool hasDbp = double.TryParse(VitalDBP, out dbp);
                if (hasSbp || hasDbp)
                {
                    if (sbp > 130 || dbp > 80) return "High BP / مرتفع";
                    if (sbp < 90 || dbp < 60) return "Low BP / منخفض";
                }
                return string.Empty;
            }
        }
        public string BPAlertBackground => "#EF4444";
        public bool IsBPAlertVisible => !string.IsNullOrWhiteSpace(BPAlertText);

        // Blood Sugar Alert
        public string BSAlertText
        {
            get
            {
                if (double.TryParse(VitalBS, out double val))
                {
                    if (val > 140) return "High / مرتفع";
                    if (val < 70) return "Low / منخفض";
                }
                return string.Empty;
            }
        }
        public string BSAlertBackground => "#EF4444";
        public bool IsBSAlertVisible => !string.IsNullOrWhiteSpace(VitalBS) && double.TryParse(VitalBS, out double val) && (val > 140 || val < 70);

        async partial void OnDrugSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
            {
                DrugSuggestions.Clear();
                IsDrugSuggestionsOpen = false;
                return;
            }

            try
            {
                using var db = await _dbContextFactory.CreateDbContextAsync();
                var matches = await db.Drugs
                    .Where(d => d.Name.StartsWith(value))
                    .Select(d => d.Name)
                    .Take(8)
                    .ToListAsync();

                DrugSuggestions.Clear();
                foreach (var match in matches)
                {
                    DrugSuggestions.Add(match);
                }
                IsDrugSuggestionsOpen = DrugSuggestions.Count > 0;
            }
            catch
            {
                // Ignore search DB errors silently
            }
        }

        [RelayCommand]
        public void AddDrug()
        {
            if (!string.IsNullOrWhiteSpace(DrugSearchText))
            {
                string drugName = DrugSearchText.Trim();
                if (!PrescribedDrugs.Any(d => d.Name.Equals(drugName, StringComparison.OrdinalIgnoreCase)))
                {
                    var med = new PrescribedMedication { Name = drugName };
                    PrescribedDrugs.Add(med);
                }
                DrugSearchText = string.Empty;
                IsDrugSuggestionsOpen = false;
            }
        }

        [RelayCommand]
        public void AddPresetDrug(string presetInfo)
        {
            if (string.IsNullOrWhiteSpace(presetInfo)) return;
            
            // Format: Name|Dose|Type|Time|Note
            var parts = presetInfo.Split('|');
            if (parts.Length > 0)
            {
                string name = parts[0].Trim();
                string dose = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                string type = parts.Length > 2 ? parts[2].Trim() : string.Empty;
                string time = parts.Length > 3 ? parts[3].Trim() : string.Empty;
                string note = parts.Length > 4 ? parts[4].Trim() : string.Empty;

                if (!PrescribedDrugs.Any(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    var med = new PrescribedMedication
                    {
                        Name = name,
                        Dose = dose,
                        Type = type,
                        Time = time,
                        Note = note
                    };
                    PrescribedDrugs.Add(med);
                }
            }
        }

        [ObservableProperty]
        private string? _selectedSuggestedDrug;

        partial void OnSelectedSuggestedDrugChanged(string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                SelectSuggestedDrug(value);
                SelectedSuggestedDrug = null;
            }
        }

        [RelayCommand]
        public void SelectSuggestedDrug(string drugName)
        {
            if (!string.IsNullOrEmpty(drugName))
            {
                DrugSearchText = drugName;
                AddDrug();
            }
        }

        // Direct bindings to support real-time HasHistory green dots without model subscriptions
        public string PatientAllergy
        {
            get => CurrentPatient?.Allergy ?? string.Empty;
            set
            {
                if (CurrentPatient != null && CurrentPatient.Allergy != value)
                {
                    CurrentPatient.Allergy = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasHistory));
                    TriggerAutoSave();
                }
            }
        }

        public string PatientSmoking
        {
            get => CurrentPatient?.Smoking ?? string.Empty;
            set
            {
                if (CurrentPatient != null && CurrentPatient.Smoking != value)
                {
                    CurrentPatient.Smoking = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasHistory));
                    TriggerAutoSave();
                }
            }
        }

        public string PatientAlcohol
        {
            get => CurrentPatient?.Alcohol ?? string.Empty;
            set
            {
                if (CurrentPatient != null && CurrentPatient.Alcohol != value)
                {
                    CurrentPatient.Alcohol = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasHistory));
                    TriggerAutoSave();
                }
            }
        }

        [RelayCommand]
        public async Task HandleDrugSearchEnterAsync()
        {
            if (string.IsNullOrWhiteSpace(DrugSearchText))
                return;

            string drugName = DrugSearchText.Trim();

            // Add to active session Rx if not already there
            if (!PrescribedDrugs.Any(d => d.Name.Equals(drugName, StringComparison.OrdinalIgnoreCase)))
            {
                PrescribedDrugs.Add(new PrescribedMedication { Name = drugName });
            }

            // Save to database if it doesn't exist
            try
            {
                using var db = await _dbContextFactory.CreateDbContextAsync();
                var exists = await db.Drugs.AnyAsync(d => d.Name.ToLower() == drugName.ToLower());
                if (!exists)
                {
                    db.Drugs.Add(new Drug { Name = drugName });
                    await db.SaveChangesAsync();
                }
            }
            catch
            {
                // Silent catch
            }

            DrugSearchText = string.Empty;
            IsDrugSuggestionsOpen = false;
        }

        private async Task LoadDatabaseSuggestionsAsync()
        {
            try
            {
                using var db = await _dbContextFactory.CreateDbContextAsync();
                
                var dbInvestigations = await db.Investigations.Select(i => i.Name).ToListAsync();
                if (dbInvestigations.Count > 0)
                {
                    InvestigationList.Clear();
                    foreach (var inv in dbInvestigations)
                    {
                        InvestigationList.Add(inv);
                    }
                    if (!InvestigationList.Contains("None"))
                    {
                        InvestigationList.Add("None");
                    }
                }

                var dbImagings = await db.Imagings.Select(i => i.Name).ToListAsync();
                if (dbImagings.Count > 0)
                {
                    ImagingList.Clear();
                    foreach (var img in dbImagings)
                    {
                        ImagingList.Add(img);
                    }
                    if (!ImagingList.Contains("None"))
                    {
                        ImagingList.Add("None");
                    }
                }
            }
            catch
            {
                // Fallback silently (defaults already set in property initializers)
            }
        }

        [RelayCommand]
        public void RemoveDrug(object? parameter)
        {
            if (parameter is PrescribedMedication drug && PrescribedDrugs.Contains(drug))
            {
                PrescribedDrugs.Remove(drug);
            }
        }

        [RelayCommand]
        public void PrintActiveRx()
        {
            if (CurrentPatient == null)
            {
                StatusMessage = "No patient selected.";
                return;
            }
            if (PrescribedDrugs.Count == 0 && string.IsNullOrWhiteSpace(DrugSearchText))
            {
                StatusMessage = "No prescription added yet.";
                return;
            }

            string rxText = string.Join(Environment.NewLine, PrescribedDrugs.Select(d => d.ToString()));
            if (string.IsNullOrWhiteSpace(rxText))
            {
                rxText = DrugSearchText.Trim();
            }

            _printService.PrintPrescription(CurrentPatient, rxText);
        }

        [RelayCommand]
        public void PrintVisitRx(object? parameter)
        {
            if (CurrentPatient == null || parameter is not Visit visit) return;
            if (string.IsNullOrWhiteSpace(visit.Prescription))
            {
                MessageBox.Show("No prescription was recorded for this visit.", "No Prescription", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _printService.PrintPrescription(CurrentPatient, visit.Prescription);
        }

        [RelayCommand]
        public void PrintVisitReport(object? parameter)
        {
            if (CurrentPatient == null || parameter is not Visit visit) return;
            _printService.PrintReport(CurrentPatient, visit);
        }

        [RelayCommand]
        public void PrintActiveVisitReport()
        {
            if (CurrentPatient == null)
            {
                StatusMessage = "No patient selected.";
                return;
            }

            var mockVisit = new Visit
            {
                VisitDate = DateTime.UtcNow,
                ChiefComplaint = ChiefComplaint,
                HistoryOfPresentIllness = HistoryOfPresentIllness,
                PhysicalExamination = PhysicalExamination,
                Diagnosis = Diagnosis,
                TreatmentPlan = TreatmentPlan,
                Prescription = string.Join(Environment.NewLine, PrescribedDrugs)
            };

            _printService.PrintReport(CurrentPatient, mockVisit);
        }

        partial void OnChiefComplaintChanged(string value) => TriggerAutoSave();
        partial void OnHistoryOfPresentIllnessChanged(string value) => TriggerAutoSave();
        partial void OnPhysicalExaminationChanged(string value) => TriggerAutoSave();
        partial void OnDiagnosisChanged(string value) => TriggerAutoSave();
        partial void OnTreatmentPlanChanged(string value) => TriggerAutoSave();

        private void TriggerAutoSave()
        {
            if (_isSavingOrLoadingDraft || CurrentPatient == null) return;
            ShowSavedBadge = false;
            _ = AutoSaveDraftAsync();
        }

        private async Task AutoSaveDraftAsync()
        {
            if (CurrentPatient == null) return;
            
            _isSavingOrLoadingDraft = true;
            try
            {
                var drafts = new Dictionary<int, PatientVisitDraft>();
                if (File.Exists(DraftsFile))
                {
                    try
                    {
                        string json = await File.ReadAllTextAsync(DraftsFile);
                        drafts = JsonSerializer.Deserialize<Dictionary<int, PatientVisitDraft>>(json) ?? drafts;
                    }
                    catch
                    {
                        // File corrupted or empty, start fresh
                    }
                }

                SyncPhysicalExamination();
                SyncTreatmentPlan();

                drafts[CurrentPatient.PatientId] = new PatientVisitDraft
                {
                    PatientId = CurrentPatient.PatientId,
                    ChiefComplaint = ChiefComplaint ?? string.Empty,
                    HistoryOfPresentIllness = HistoryOfPresentIllness ?? string.Empty,
                    PhysicalExamination = PhysicalExamination ?? string.Empty,
                    Diagnosis = Diagnosis ?? string.Empty,
                    TreatmentPlan = TreatmentPlan ?? string.Empty,
                    PrescribedDrugs = new System.Collections.Generic.List<PrescribedMedication>(PrescribedDrugs),
                    VitalsHR = VitalHR ?? string.Empty,
                    VitalsSBP = VitalSBP ?? string.Empty,
                    VitalsDBP = VitalDBP ?? string.Empty,
                    VitalsRR = VitalRR ?? string.Empty,
                    VitalsSPO2 = VitalSPO2 ?? string.Empty,
                    VitalsTemp = VitalTemp ?? string.Empty,
                    IsVitallyStable = IsVitallyStable,
                    Investigation = SelectedInvestigation ?? string.Empty,
                    Imaging = SelectedImaging ?? string.Empty,
                    Investigations = new List<ClinicalAttachment>(AddedInvestigations),
                    Imagings = new List<ClinicalAttachment>(AddedImagings),
                    ReturnDate = ReturnDate,
                    InvestigationAttachmentPath = InvestigationAttachmentPath ?? string.Empty,
                    ImagingAttachmentPath = ImagingAttachmentPath ?? string.Empty,
                    PatientNotes = CurrentPatient.Notes ?? string.Empty,
                    PatientAllergy = CurrentPatient.Allergy ?? string.Empty,
                    PatientSmoking = CurrentPatient.Smoking ?? string.Empty,
                    PatientAlcohol = CurrentPatient.Alcohol ?? string.Empty,
                    PatientBloodGroup = CurrentPatient.BloodGroup ?? string.Empty,

                    // Structured History
                    PatientPastMedicalHistory = PastMedicalHistory ?? string.Empty,
                    PatientPastSurgicalHistory = PastSurgicalHistory ?? string.Empty,
                    PatientPastDrugHistory = PastDrugHistory ?? string.Empty,
                    PatientPastFamilyHistory = PastFamilyHistory ?? string.Empty,
                    PatientSmokingCigarettesPerDay = SmokingCigarettesPerDay ?? string.Empty,
                    PatientSmokingYears = SmokingYears ?? string.Empty,
                    PatientAlcoholType = AlcoholType ?? string.Empty,
                    PatientAlcoholConcentration = AlcoholConcentration ?? string.Empty,
                    PatientAlcoholVolume = AlcoholVolume ?? string.Empty,

                    // Procedures, Split Exam & Instructions
                    ProceduresPerformed = string.Join(";", PerformedProcedures.Select(p => $"{p.Name}|{p.Cost}")),
                    PhysicalExamPositive = PhysicalExamPositive ?? string.Empty,
                    PhysicalExamNegative = PhysicalExamNegative ?? string.Empty,
                    MedicalInstructions = MedicalInstructions ?? string.Empty
                };

                string outputJson = JsonSerializer.Serialize(drafts, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(DraftsFile, outputJson);
                ShowSavedBadge = true;
            }
            catch
            {
                // Silently ignore disk IO errors
            }
            finally
            {
                _isSavingOrLoadingDraft = false;
            }
        }

        private async Task LoadDraftForPatientAsync(int patientId)
        {
            _isSavingOrLoadingDraft = true;
            try
            {
                if (File.Exists(DraftsFile))
                {
                    string json = await File.ReadAllTextAsync(DraftsFile);
                    var drafts = JsonSerializer.Deserialize<Dictionary<int, PatientVisitDraft>>(json);
                    if (drafts != null && drafts.TryGetValue(patientId, out var draft))
                    {
                        ChiefComplaint = draft.ChiefComplaint;
                        HistoryOfPresentIllness = draft.HistoryOfPresentIllness;
                        PhysicalExamination = draft.PhysicalExamination;
                        Diagnosis = draft.Diagnosis;
                        TreatmentPlan = draft.TreatmentPlan;
                        ParsePhysicalExamination(PhysicalExamination ?? string.Empty);
                        ParseTreatmentPlan(TreatmentPlan ?? string.Empty);
                        VitalHR = draft.VitalsHR;
                        VitalSBP = draft.VitalsSBP;
                        VitalDBP = draft.VitalsDBP;
                        VitalRR = draft.VitalsRR;
                        VitalSPO2 = draft.VitalsSPO2;
                        VitalTemp = draft.VitalsTemp;
                        IsVitallyStable = draft.IsVitallyStable;
                        SelectedInvestigation = draft.Investigation;
                        SelectedImaging = draft.Imaging;
                        ReturnDate = draft.ReturnDate;
                        InvestigationAttachmentPath = draft.InvestigationAttachmentPath;
                        ImagingAttachmentPath = draft.ImagingAttachmentPath;

                        // Legacy draft parsing
                        if ((draft.Investigations == null || draft.Investigations.Count == 0) && !string.IsNullOrEmpty(draft.Investigation))
                        {
                            var names = draft.Investigation.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                            var paths = (draft.InvestigationAttachmentPath ?? string.Empty).Split(new[] { ";" }, StringSplitOptions.None);
                            draft.Investigations = new List<ClinicalAttachment>();
                            for (int i = 0; i < names.Length; i++)
                            {
                                draft.Investigations.Add(new ClinicalAttachment 
                                { 
                                    Name = names[i], 
                                    AttachmentPath = i < paths.Length ? paths[i] : string.Empty 
                                });
                            }
                        }

                        if ((draft.Imagings == null || draft.Imagings.Count == 0) && !string.IsNullOrEmpty(draft.Imaging))
                        {
                            var names = draft.Imaging.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                            var paths = (draft.ImagingAttachmentPath ?? string.Empty).Split(new[] { ";" }, StringSplitOptions.None);
                            draft.Imagings = new List<ClinicalAttachment>();
                            for (int i = 0; i < names.Length; i++)
                            {
                                draft.Imagings.Add(new ClinicalAttachment 
                                { 
                                    Name = names[i], 
                                    AttachmentPath = i < paths.Length ? paths[i] : string.Empty 
                                });
                            }
                        }

                        var newInvestigations = new ObservableCollection<ClinicalAttachment>(draft.Investigations ?? new List<ClinicalAttachment>());
                        newInvestigations.CollectionChanged += (s, e) => TriggerAutoSave();
                        AddedInvestigations = newInvestigations;

                        var newImagings = new ObservableCollection<ClinicalAttachment>(draft.Imagings ?? new List<ClinicalAttachment>());
                        newImagings.CollectionChanged += (s, e) => TriggerAutoSave();
                        AddedImagings = newImagings;
                        
                        var newCollection = new ObservableCollection<PrescribedMedication>(draft.PrescribedDrugs);
                        newCollection.CollectionChanged += (s, e) => TriggerAutoSave();
                        PrescribedDrugs = newCollection;

                        // Load procedures
                        var newProcs = new ObservableCollection<PerformedProcedure>();
                        if (!string.IsNullOrEmpty(draft.ProceduresPerformed))
                        {
                            var procs = draft.ProceduresPerformed.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var p in procs)
                            {
                                var parts = p.Split('|');
                                if (parts.Length > 0)
                                {
                                    decimal cost = 0;
                                    if (parts.Length > 1) decimal.TryParse(parts[1], out cost);
                                    newProcs.Add(new PerformedProcedure { Name = parts[0], Cost = cost });
                                }
                            }
                        }
                        PerformedProcedures = newProcs;
                        
                        PhysicalExamPositive = draft.PhysicalExamPositive ?? string.Empty;
                        PhysicalExamNegative = draft.PhysicalExamNegative ?? string.Empty;
                        MedicalInstructions = draft.MedicalInstructions ?? string.Empty;

                        PastMedicalHistory = draft.PatientPastMedicalHistory ?? string.Empty;
                        PastSurgicalHistory = draft.PatientPastSurgicalHistory ?? string.Empty;
                        PastDrugHistory = draft.PatientPastDrugHistory ?? string.Empty;
                        PastFamilyHistory = draft.PatientPastFamilyHistory ?? string.Empty;
                        SmokingCigarettesPerDay = draft.PatientSmokingCigarettesPerDay ?? string.Empty;
                        SmokingYears = draft.PatientSmokingYears ?? string.Empty;
                        AlcoholType = draft.PatientAlcoholType ?? string.Empty;
                        AlcoholConcentration = draft.PatientAlcoholConcentration ?? string.Empty;
                        AlcoholVolume = draft.PatientAlcoholVolume ?? string.Empty;

                        if (CurrentPatient != null)
                        {
                            CurrentPatient.Notes = draft.PatientNotes ?? string.Empty;
                            CurrentPatient.Allergy = draft.PatientAllergy ?? string.Empty;
                            CurrentPatient.Smoking = draft.PatientSmoking ?? string.Empty;
                            CurrentPatient.Alcohol = draft.PatientAlcohol ?? string.Empty;
                            CurrentPatient.BloodGroup = draft.PatientBloodGroup ?? string.Empty;

                            CurrentPatient.PastMedicalHistory = draft.PatientPastMedicalHistory ?? string.Empty;
                            CurrentPatient.PastSurgicalHistory = draft.PatientPastSurgicalHistory ?? string.Empty;
                            CurrentPatient.PastDrugHistory = draft.PatientPastDrugHistory ?? string.Empty;
                            CurrentPatient.PastFamilyHistory = draft.PatientPastFamilyHistory ?? string.Empty;
                            CurrentPatient.SmokingCigarettesPerDay = draft.PatientSmokingCigarettesPerDay ?? string.Empty;
                            CurrentPatient.SmokingYears = draft.PatientSmokingYears ?? string.Empty;
                            CurrentPatient.AlcoholType = draft.PatientAlcoholType ?? string.Empty;
                            CurrentPatient.AlcoholConcentration = draft.PatientAlcoholConcentration ?? string.Empty;
                            CurrentPatient.AlcoholVolume = draft.PatientAlcoholVolume ?? string.Empty;
                        }

                        // Fire property notifications to update green dots and controls
                        OnPropertyChanged(nameof(PatientAllergy));
                        OnPropertyChanged(nameof(PatientSmoking));
                        OnPropertyChanged(nameof(PatientAlcohol));
                        OnPropertyChanged(nameof(HasHistory));

                        ShowSavedBadge = true;
                        return;
                    }
                }
                
                // Clear fields if no draft exists
                ClearFormFieldsWithoutAutoSave();
                ShowSavedBadge = true; // Clean form starts as "saved" (no unsaved changes)
            }
            catch
            {
                // Ignore load errors and fallback to clean form
                ClearFormFieldsWithoutAutoSave();
            }
            finally
            {
                _isSavingOrLoadingDraft = false;
            }
        }

        private async Task DeleteDraftForPatientAsync(int patientId)
        {
            try
            {
                if (File.Exists(DraftsFile))
                {
                    string json = await File.ReadAllTextAsync(DraftsFile);
                    var drafts = JsonSerializer.Deserialize<Dictionary<int, PatientVisitDraft>>(json);
                    if (drafts != null && drafts.Remove(patientId))
                    {
                        string outputJson = JsonSerializer.Serialize(drafts, new JsonSerializerOptions { WriteIndented = true });
                        await File.WriteAllTextAsync(DraftsFile, outputJson);
                    }
                }
            }
            catch
            {
                // Ignore delete errors silently
            }
        }

        private void ClearFormFieldsWithoutAutoSave()
        {
            ChiefComplaint = string.Empty;
            HistoryOfPresentIllness = string.Empty;
            PhysicalExamination = string.Empty;
            Diagnosis = string.Empty;
            TreatmentPlan = string.Empty;
            VitalHR = string.Empty;
            VitalSBP = string.Empty;
            VitalDBP = string.Empty;
            VitalRR = string.Empty;
            VitalSPO2 = string.Empty;
            VitalTemp = string.Empty;
            VitalBS = string.Empty;
            IsVitallyStable = false;
            SelectedInvestigation = string.Empty;
            SelectedImaging = string.Empty;
            ReturnDate = null;
            InvestigationAttachmentPath = string.Empty;
            ImagingAttachmentPath = string.Empty;

            // Clear new clinical fields
            PerformedProcedures.Clear();
            PhysicalExamPositive = string.Empty;
            PhysicalExamNegative = string.Empty;
            MedicalInstructions = string.Empty;

            // Clear structured history fields
            PastMedicalHistory = string.Empty;
            PastSurgicalHistory = string.Empty;
            PastDrugHistory = string.Empty;
            PastFamilyHistory = string.Empty;
            SmokingCigarettesPerDay = string.Empty;
            SmokingYears = string.Empty;
            AlcoholType = string.Empty;
            AlcoholConcentration = string.Empty;
            AlcoholVolume = string.Empty;
            
            var newCollection = new ObservableCollection<PrescribedMedication>();
            newCollection.CollectionChanged += (s, e) => TriggerAutoSave();
            PrescribedDrugs = newCollection;

            var newInvestigations = new ObservableCollection<ClinicalAttachment>();
            newInvestigations.CollectionChanged += (s, e) => TriggerAutoSave();
            AddedInvestigations = newInvestigations;

            var newImagings = new ObservableCollection<ClinicalAttachment>();
            newImagings.CollectionChanged += (s, e) => TriggerAutoSave();
            AddedImagings = newImagings;
        }

        [RelayCommand]
        public void SimulateVoiceInput(string fieldName)
        {
            string simulatedText = fieldName switch
            {
                "Complaint" => "Patient complains of chest tightness and shortness of breath.",
                "HPI" => "Symptoms started 3 days ago, worsening with physical activity.",
                "Exam" => "Blood pressure 135/85 mmHg, pulse 80 bpm, lungs clear to auscultation.",
                "Diagnosis" => "Mild essential hypertension.",
                "Plan" => "Instructed patient on low-sodium diet and scheduled follow-up in two weeks.",
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(simulatedText)) return;

            switch (fieldName)
            {
                case "Complaint":
                    ChiefComplaint = string.IsNullOrEmpty(ChiefComplaint) ? simulatedText : $"{ChiefComplaint} {simulatedText}";
                    break;
                case "HPI":
                    HistoryOfPresentIllness = string.IsNullOrEmpty(HistoryOfPresentIllness) ? simulatedText : $"{HistoryOfPresentIllness} {simulatedText}";
                    break;
                case "Exam":
                    PhysicalExamination = string.IsNullOrEmpty(PhysicalExamination) ? simulatedText : $"{PhysicalExamination} {simulatedText}";
                    break;
                case "Diagnosis":
                    Diagnosis = string.IsNullOrEmpty(Diagnosis) ? simulatedText : $"{Diagnosis} {simulatedText}";
                    break;
                case "Plan":
                    TreatmentPlan = string.IsNullOrEmpty(TreatmentPlan) ? simulatedText : $"{TreatmentPlan} {simulatedText}";
                    break;
            }

            StatusMessage = $"Captured voice input for {fieldName}.";
        }

        // Changed handlers for auto-saving view/draft fields

        partial void OnVitalHRChanged(string value)
        {
            OnPropertyChanged(nameof(HRAlertText));
            OnPropertyChanged(nameof(HRAlertBackground));
            OnPropertyChanged(nameof(IsHRAlertVisible));
            TriggerAutoSave();
        }

        partial void OnVitalSBPChanged(string value)
        {
            OnPropertyChanged(nameof(BPAlertText));
            OnPropertyChanged(nameof(BPAlertBackground));
            OnPropertyChanged(nameof(IsBPAlertVisible));
            TriggerAutoSave();
        }

        partial void OnVitalDBPChanged(string value)
        {
            OnPropertyChanged(nameof(BPAlertText));
            OnPropertyChanged(nameof(BPAlertBackground));
            OnPropertyChanged(nameof(IsBPAlertVisible));
            TriggerAutoSave();
        }

        partial void OnVitalRRChanged(string value)
        {
            OnPropertyChanged(nameof(RRAlertText));
            OnPropertyChanged(nameof(RRAlertBackground));
            OnPropertyChanged(nameof(IsRRAlertVisible));
            TriggerAutoSave();
        }

        partial void OnVitalSPO2Changed(string value)
        {
            OnPropertyChanged(nameof(SPO2AlertText));
            OnPropertyChanged(nameof(SPO2AlertBackground));
            OnPropertyChanged(nameof(IsSPO2AlertVisible));
            TriggerAutoSave();
        }

        partial void OnVitalTempChanged(string value)
        {
            OnPropertyChanged(nameof(TempAlertText));
            OnPropertyChanged(nameof(TempAlertBackground));
            OnPropertyChanged(nameof(IsTempAlertVisible));
            TriggerAutoSave();
        }

        partial void OnVitalBSChanged(string value)
        {
            OnPropertyChanged(nameof(BSAlertText));
            OnPropertyChanged(nameof(BSAlertBackground));
            OnPropertyChanged(nameof(IsBSAlertVisible));
            TriggerAutoSave();
        }
        partial void OnSelectedInvestigationChanged(string value) => TriggerAutoSave();
        partial void OnSelectedImagingChanged(string value) => TriggerAutoSave();
        partial void OnReturnDateChanged(DateTime? value) => TriggerAutoSave();

        partial void OnIsVitallyStableChanged(bool value)
        {
            if (value)
            {
                VitalHR = "75";
                VitalSBP = "120";
                VitalDBP = "80";
                VitalRR = "16";
                VitalSPO2 = "98";
                VitalTemp = "37.0";
            }
            else
            {
                VitalHR = string.Empty;
                VitalSBP = string.Empty;
                VitalDBP = string.Empty;
                VitalRR = string.Empty;
                VitalSPO2 = string.Empty;
                VitalTemp = string.Empty;
            }
            TriggerAutoSave();
        }

        [RelayCommand]
        public void UploadInvestigationAttachment()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*|Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif|PDF Files (*.pdf)|*.pdf|Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sourcePath = openFileDialog.FileName;
                    var extension = Path.GetExtension(sourcePath);
                    var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VisitAttachments");
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    var uniqueName = $"investigation_{Guid.NewGuid()}{extension}";
                    var destPath = Path.Combine(destDir, uniqueName);
                    
                    File.Copy(sourcePath, destPath, overwrite: true);
                    InvestigationAttachmentPath = destPath;
                    StatusMessage = "Investigation attachment uploaded successfully!";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Failed to upload file: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        public void UploadImagingAttachment()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*|Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif|PDF Files (*.pdf)|*.pdf|Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sourcePath = openFileDialog.FileName;
                    var extension = Path.GetExtension(sourcePath);
                    var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VisitAttachments");
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    var uniqueName = $"imaging_{Guid.NewGuid()}{extension}";
                    var destPath = Path.Combine(destDir, uniqueName);
                    
                    File.Copy(sourcePath, destPath, overwrite: true);
                    ImagingAttachmentPath = destPath;
                    StatusMessage = "Imaging attachment uploaded successfully!";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Failed to upload file: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        public void RemoveInvestigationAttachment()
        {
            try
            {
                if (File.Exists(InvestigationAttachmentPath))
                {
                    File.Delete(InvestigationAttachmentPath);
                }
            }
            catch {}
            InvestigationAttachmentPath = string.Empty;
            TriggerAutoSave();
        }

        [RelayCommand]
        public void RemoveImagingAttachment()
        {
            try
            {
                if (File.Exists(ImagingAttachmentPath))
                {
                    File.Delete(ImagingAttachmentPath);
                }
            }
            catch {}
            ImagingAttachmentPath = string.Empty;
            TriggerAutoSave();
        }

        [RelayCommand]
        public void OpenAttachment(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                StatusMessage = "Attachment file not found.";
                return;
            }

            try
            {
                string ext = Path.GetExtension(filePath).ToLower();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp")
                {
                    // Open in our custom fullscreen image preview window
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var previewWin = new Views.ImagePreviewWindow(filePath);
                        previewWin.ShowDialog();
                    });
                }
                else
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Could not open attachment: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task AddInvestigationAsync()
        {
            if (!string.IsNullOrWhiteSpace(SelectedInvestigation) && SelectedInvestigation != "None")
            {
                string name = SelectedInvestigation.Trim();

                if (!AddedInvestigations.Any(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    AddedInvestigations.Add(new ClinicalAttachment { Name = name });
                }

                if (!InvestigationList.Any(i => i.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        using var db = await _dbContextFactory.CreateDbContextAsync();
                        var exists = await db.Investigations.AnyAsync(i => i.Name.ToLower() == name.ToLower());
                        if (!exists)
                        {
                            db.Investigations.Add(new Investigation { Name = name });
                            await db.SaveChangesAsync();
                        }
                    }
                    catch
                    {
                        // Silent catch
                    }
                    InvestigationList.Add(name);
                }

                SelectedInvestigation = string.Empty;
            }
        }

        [RelayCommand]
        public async Task AddImagingAsync()
        {
            if (!string.IsNullOrWhiteSpace(SelectedImaging) && SelectedImaging != "None")
            {
                string name = SelectedImaging.Trim();

                if (!AddedImagings.Any(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    AddedImagings.Add(new ClinicalAttachment { Name = name });
                }

                if (!ImagingList.Any(i => i.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        using var db = await _dbContextFactory.CreateDbContextAsync();
                        var exists = await db.Imagings.AnyAsync(i => i.Name.ToLower() == name.ToLower());
                        if (!exists)
                        {
                            db.Imagings.Add(new Imaging { Name = name });
                            await db.SaveChangesAsync();
                        }
                    }
                    catch
                    {
                        // Silent catch
                    }
                    ImagingList.Add(name);
                }

                SelectedImaging = string.Empty;
            }
        }

        [RelayCommand]
        public void RemoveInvestigation(object? parameter)
        {
            if (parameter is ClinicalAttachment item && AddedInvestigations.Contains(item))
            {
                if (!string.IsNullOrEmpty(item.AttachmentPath) && File.Exists(item.AttachmentPath))
                {
                    try { File.Delete(item.AttachmentPath); } catch {}
                }
                AddedInvestigations.Remove(item);
            }
        }

        [RelayCommand]
        public void RemoveImaging(object? parameter)
        {
            if (parameter is ClinicalAttachment item && AddedImagings.Contains(item))
            {
                if (!string.IsNullOrEmpty(item.AttachmentPath) && File.Exists(item.AttachmentPath))
                {
                    try { File.Delete(item.AttachmentPath); } catch {}
                }
                AddedImagings.Remove(item);
            }
        }

        [RelayCommand]
        public void UploadInvestigationFile(object? parameter)
        {
            if (parameter is not ClinicalAttachment item) return;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*|Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif|PDF Files (*.pdf)|*.pdf|Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sourcePath = openFileDialog.FileName;
                    var extension = Path.GetExtension(sourcePath);
                    var uploadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var uniqueName = $"investigation_{Guid.NewGuid()}{extension}";
                    var destPath = Path.Combine(uploadsDir, uniqueName);
                    File.Copy(sourcePath, destPath, true);

                    // Remove old file if it exists
                    if (!string.IsNullOrEmpty(item.AttachmentPath) && File.Exists(item.AttachmentPath))
                    {
                        try { File.Delete(item.AttachmentPath); } catch {}
                    }

                    item.AttachmentPath = destPath;
                    StatusMessage = "Investigation attachment uploaded successfully!";
                    TriggerAutoSave();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error uploading file: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        public void RemoveInvestigationFile(ClinicalAttachment item)
        {
            if (item == null) return;

            if (!string.IsNullOrEmpty(item.AttachmentPath) && File.Exists(item.AttachmentPath))
            {
                try { File.Delete(item.AttachmentPath); } catch {}
            }
            item.AttachmentPath = string.Empty;
            StatusMessage = "Investigation attachment removed.";
            TriggerAutoSave();
        }

        [RelayCommand]
        public void UploadImagingFile(object? parameter)
        {
            if (parameter is not ClinicalAttachment item) return;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*|Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif|PDF Files (*.pdf)|*.pdf"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sourcePath = openFileDialog.FileName;
                    var extension = Path.GetExtension(sourcePath);
                    var uploadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var uniqueName = $"imaging_{Guid.NewGuid()}{extension}";
                    var destPath = Path.Combine(uploadsDir, uniqueName);
                    File.Copy(sourcePath, destPath, true);

                    // Remove old file if it exists
                    if (!string.IsNullOrEmpty(item.AttachmentPath) && File.Exists(item.AttachmentPath))
                    {
                        try { File.Delete(item.AttachmentPath); } catch {}
                    }

                    item.AttachmentPath = destPath;
                    StatusMessage = "Imaging attachment uploaded successfully!";
                    TriggerAutoSave();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error uploading file: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        public void RemoveImagingFile(ClinicalAttachment item)
        {
            if (item == null) return;

            if (!string.IsNullOrEmpty(item.AttachmentPath) && File.Exists(item.AttachmentPath))
            {
                try { File.Delete(item.AttachmentPath); } catch {}
            }
            item.AttachmentPath = string.Empty;
            StatusMessage = "Imaging attachment removed.";
            TriggerAutoSave();
        }

        [RelayCommand]
        public void ScanDocument(object? parameter)
        {
            if (parameter is not ClinicalAttachment item) return;

            try
            {
                var uploadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Call scanner service
                var scannedFilePath = _scannerService.ScanFromDefaultDevice(uploadsDir);
                if (string.IsNullOrEmpty(scannedFilePath))
                {
                    StatusMessage = "Scan cancelled by user.";
                    return;
                }

                // If scanned successfully, delete old file if it exists
                if (!string.IsNullOrEmpty(item.AttachmentPath) && File.Exists(item.AttachmentPath))
                {
                    try { File.Delete(item.AttachmentPath); } catch {}
                }

                item.AttachmentPath = scannedFilePath;
                StatusMessage = "Document scanned and attached successfully!";
                TriggerAutoSave();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Scan failed: {ex.Message}";
                System.Windows.MessageBox.Show(
                    $"Failed to scan document:\n{ex.Message}", 
                    "Scanner Error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        public void OpenAddPatientModal()
        {
            NewPatientName = string.Empty;
            NewPatientAge = 0;
            NewPatientAgeMonths = 0;
            NewPatientGender = "ذكر";
            NewPatientPhone = string.Empty;
            NewPatientBirthDate = null;
            NewPatientGovernorate = "بغداد";
            NewPatientFiles = string.Empty;
            
            NewPatientIsPaidVisit = true;
            NewPatientIsFreeVisit = false;
            NewPatientVisitPrice = "25000";
            
            AddPatientValidationMessage = string.Empty;
            ShowAddPatientModal = true;
        }

        [RelayCommand]
        public void CancelAddPatient()
        {
            ShowAddPatientModal = false;
        }

        [RelayCommand]
        public void UploadNewPatientFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*|Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif|PDF Files (*.pdf)|*.pdf|Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sourcePath = openFileDialog.FileName;
                    var extension = System.IO.Path.GetExtension(sourcePath);
                    var destDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PatientFiles");
                    if (!System.IO.Directory.Exists(destDir))
                    {
                        System.IO.Directory.CreateDirectory(destDir);
                    }

                    var uniqueName = $"patient_{Guid.NewGuid()}{extension}";
                    var destPath = System.IO.Path.Combine(destDir, uniqueName);
                    
                    System.IO.File.Copy(sourcePath, destPath, overwrite: true);
                    NewPatientFiles = destPath;
                }
                catch (Exception ex)
                {
                    AddPatientValidationMessage = $"Failed to upload file: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        public void RemoveNewPatientFile()
        {
            NewPatientFiles = string.Empty;
        }

        [RelayCommand]
        public void OpenNewPatientFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            try
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                AddPatientValidationMessage = $"Failed to open file: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task SaveAddPatientAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPatientName))
            {
                AddPatientValidationMessage = "Patient Name is required! | اسم المريض مطلوب!";
                return;
            }

            try
            {
                var patient = new Patient
                {
                    Name = NewPatientName.Trim(),
                    Age = NewPatientAge,
                    AgeMonths = NewPatientAgeMonths,
                    Gender = NewPatientGender,
                    Phone = NewPatientPhone?.Trim() ?? string.Empty,
                    BirthDate = NewPatientBirthDate,
                    Governorate = NewPatientGovernorate,
                    PatientFiles = NewPatientFiles ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                await _patientService.AddPatientAsync(patient);

                decimal price = 0;
                if (NewPatientIsPaidVisit)
                {
                    decimal.TryParse(NewPatientVisitPrice, out price);
                }

                var visit = new Visit
                {
                    PatientId = patient.PatientId,
                    VisitDate = DateTime.UtcNow,
                    IsPaid = NewPatientIsPaidVisit,
                    VisitPrice = price,
                    DoctorName = ActiveDoctorName
                };

                await _patientService.AddVisitForCheckInAsync(visit);

                // Add to daily queue
                await _queueService.AddToQueueAsync(patient.PatientId, patient.Name, ActiveDoctorName);

                ShowAddPatientModal = false;

                // Sync current selection
                CurrentPatient = patient;
                SelectedPatientLookup = patient;

                _ = PollQueueAsync();
            }
            catch (Exception ex)
            {
                AddPatientValidationMessage = $"Error: {ex.Message}";
            }
        }

        public void StartPolling()
        {
            // Background waitlist polling disabled in ClinicalExamViewModel
            // The waitlist queue is now polled in HomeViewModel instead.
        }

        public void StopPolling()
        {
            _pollingTimer?.Stop();
        }

        [RelayCommand]
        public void PrintClinicalAttachment(object? parameter)
        {
            if (parameter is not ClinicalAttachment item) return;

            if (CurrentPatient == null)
            {
                StatusMessage = "No patient selected.";
                return;
            }

            try
            {
                string category = "Prescription";
                if (AddedInvestigations.Contains(item))
                {
                    category = "Investigation";
                }
                else if (AddedImagings.Contains(item))
                {
                    category = "Imaging";
                }

                _printService.PrintClinicalAttachment(CurrentPatient, item, category);
                StatusMessage = $"Clinical {category.ToLower()} print preview opened.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Print failed: {ex.Message}";
            }
        }

        private static readonly string SectionOrderFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "doctor_sections_order.json");

        private void InitializeSectionsOrder()
        {
            var defaultKeys = new List<(string Key, string En, string Ar)>
            {
                ("Complaints", "Chief Complaint", "الشكوى الرئيسية"),
                ("Vitals", "Vital Signs", "العلامات الحيوية"),
                ("Examination", "Clinical Examination", "الفحص السريري"),
                ("Hpi", "History of Present Illness", "تاريخ المرض الحالي"),
                ("Procedures", "Procedures performed", "الإجراءات الطبية"),
                ("Diagnosis", "Diagnosis", "التشخيص"),
                ("TreatmentPlan", "Treatment Plan", "خطة العلاج والملاحظات"),
                ("Prescription", "Prescriptions & Instructions", "الوصفة والإرشادات"),
                ("Investigations", "Investigations & Labs", "التحاليل والمختبر"),
                ("Imaging", "Imaging & Radiology", "الأشعة والسونار"),
                ("Obstetrics", "Obstetrics & Gynecology", "التوليد وأمراض النساء"),
                ("SystemReview", "System Review (ROS)", "مراجعة أجهزة الجسم"),
                ("Nutrition", "Nutritional Status", "الحالة الغذائية"),
                ("History", "Patient History & Lifestyle", "التاريخ المرضي ونمط الحياة"),
                ("DentistNotes", "Dentist Notes", "ملاحظات طبيب الأسنان"),
                ("VisitSummary", "Visit Summary notes", "ملخص الزيارة")
            };

            List<SectionConfig>? saved = null;
            try
            {
                if (File.Exists(SectionOrderFile))
                {
                    string json = File.ReadAllText(SectionOrderFile);
                    saved = JsonSerializer.Deserialize<List<SectionConfig>>(json);
                }
            }
            catch { /* ignore */ }

            SectionOrderList.Clear();
            int order = 0;
            if (saved != null && saved.Count > 0)
            {
                foreach (var s in saved)
                {
                    var def = defaultKeys.FirstOrDefault(dk => dk.Key == s.Key);
                    if (def != default)
                    {
                        var item = new SectionOrderItem
                        {
                            SectionKey = s.Key,
                            DisplayNameEn = def.En,
                            DisplayNameAr = def.Ar,
                            IsVisible = s.IsVisible,
                            DisplayOrder = order++
                        };
                        item.PropertyChanged += (sender, e) => {
                            if (e.PropertyName == nameof(SectionOrderItem.IsVisible))
                            {
                                ApplySectionsOrderAndVisibility();
                            }
                        };
                        SectionOrderList.Add(item);
                    }
                }
                foreach (var dk in defaultKeys)
                {
                    if (!SectionOrderList.Any(sol => sol.SectionKey == dk.Key))
                    {
                        var item = new SectionOrderItem
                        {
                            SectionKey = dk.Key,
                            DisplayNameEn = dk.En,
                            DisplayNameAr = dk.Ar,
                            IsVisible = true,
                            DisplayOrder = order++
                        };
                        item.PropertyChanged += (sender, e) => {
                            if (e.PropertyName == nameof(SectionOrderItem.IsVisible))
                            {
                                ApplySectionsOrderAndVisibility();
                            }
                        };
                        SectionOrderList.Add(item);
                    }
                }
            }
            else
            {
                foreach (var dk in defaultKeys)
                {
                    bool isVis = dk.Key != "SystemReview" && dk.Key != "DentistNotes" && dk.Key != "Nutrition";
                    var item = new SectionOrderItem
                    {
                        SectionKey = dk.Key,
                        DisplayNameEn = dk.En,
                        DisplayNameAr = dk.Ar,
                        IsVisible = isVis,
                        DisplayOrder = order++
                    };
                    item.PropertyChanged += (sender, e) => {
                        if (e.PropertyName == nameof(SectionOrderItem.IsVisible))
                        {
                            ApplySectionsOrderAndVisibility();
                        }
                    };
                    SectionOrderList.Add(item);
                }
            }

            ApplySectionsOrderAndVisibility();
        }

        private void ApplySectionsOrderAndVisibility()
        {
            for (int i = 0; i < SectionOrderList.Count; i++)
            {
                var item = SectionOrderList[i];
                item.DisplayOrder = i;

                switch (item.SectionKey)
                {
                    case "Complaints":
                        ComplaintsRow = i;
                        ShowComplaint = item.IsVisible;
                        break;
                    case "Vitals":
                        VitalsRow = i;
                        ShowVitals = item.IsVisible;
                        break;
                    case "Examination":
                        ExaminationRow = i;
                        ShowExamination = item.IsVisible;
                        break;
                    case "Hpi":
                        HpiRow = i;
                        ShowHpi = item.IsVisible;
                        break;
                    case "Procedures":
                        ProceduresRow = i;
                        ShowProcedures = item.IsVisible;
                        break;
                    case "Diagnosis":
                        DiagnosisRow = i;
                        ShowDiagnosis = item.IsVisible;
                        break;
                    case "TreatmentPlan":
                        TreatmentPlanRow = i;
                        ShowPlan = item.IsVisible;
                        break;
                    case "Prescription":
                        PrescriptionRow = i;
                        ShowPrescription = item.IsVisible;
                        break;
                    case "Investigations":
                        InvestigationsRow = i;
                        ShowInvestigations = item.IsVisible;
                        break;
                    case "Imaging":
                        ImagingRow = i;
                        ShowImaging = item.IsVisible;
                        break;
                    case "Obstetrics":
                        ObstetricsRow = i;
                        ShowObstetrics = item.IsVisible;
                        break;
                    case "SystemReview":
                        SystemReviewRow = i;
                        ShowSystemReview = item.IsVisible;
                        break;
                    case "Nutrition":
                        NutritionRow = i;
                        ShowNutrition = item.IsVisible;
                        break;
                    case "History":
                        HistoryRow = i;
                        ShowHistory = item.IsVisible;
                        break;
                    case "DentistNotes":
                        DentistNotesRow = i;
                        ShowDentistNotes = item.IsVisible;
                        break;
                    case "VisitSummary":
                        VisitSummaryRow = i;
                        ShowVisitSummary = item.IsVisible;
                        break;
                }
            }

            OnPropertyChanged(nameof(ShowObstetricsCard));
            OnPropertyChanged(nameof(ShowComplaint));
            SaveSectionsConfig();
        }

        private void SaveSectionsConfig()
        {
            try
            {
                var configs = SectionOrderList.Select(s => new SectionConfig { Key = s.SectionKey, IsVisible = s.IsVisible }).ToList();
                string json = JsonSerializer.Serialize(configs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SectionOrderFile, json);
            }
            catch { /* ignore */ }
        }

        private class SectionConfig
        {
            public string Key { get; set; } = string.Empty;
            public bool IsVisible { get; set; }
        }

        [RelayCommand]
        public void MoveSectionUp(SectionOrderItem item)
        {
            if (item == null) return;
            int idx = SectionOrderList.IndexOf(item);
            if (idx > 0)
            {
                SectionOrderList.RemoveAt(idx);
                SectionOrderList.Insert(idx - 1, item);
                ApplySectionsOrderAndVisibility();
            }
        }

        [RelayCommand]
        public void MoveSectionDown(SectionOrderItem item)
        {
            if (item == null) return;
            int idx = SectionOrderList.IndexOf(item);
            if (idx >= 0 && idx < SectionOrderList.Count - 1)
            {
                SectionOrderList.RemoveAt(idx);
                SectionOrderList.Insert(idx + 1, item);
                ApplySectionsOrderAndVisibility();
            }
        }

        [RelayCommand]
        public void AddProcedure()
        {
            if (string.IsNullOrWhiteSpace(SelectedProcedureName)) return;
            decimal cost = 0;
            decimal.TryParse(ProcedureCostText, out cost);

            var proc = new PerformedProcedure
            {
                Name = SelectedProcedureName.Trim(),
                Cost = cost
            };
            PerformedProcedures.Add(proc);

            SelectedProcedureName = string.Empty;
            ProcedureCostText = string.Empty;
            TriggerAutoSave();
        }

        [RelayCommand]
        public void RemoveProcedure(PerformedProcedure proc)
        {
            if (proc != null)
            {
                PerformedProcedures.Remove(proc);
                TriggerAutoSave();
            }
        }

        [RelayCommand]
        public async Task LoadInstructionPresetsAsync()
        {
            try
            {
                using var db = await _dbContextFactory.CreateDbContextAsync();
                var presets = await db.InstructionPresets.ToListAsync();
                InstructionPresetsList = new ObservableCollection<InstructionPreset>(presets);
            }
            catch { /* ignore */ }
        }

        [RelayCommand]
        public async Task SaveInstructionPresetAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(MedicalInstructions)) return;

            try
            {
                using var db = await _dbContextFactory.CreateDbContextAsync();
                var existing = await db.InstructionPresets.FirstOrDefaultAsync(p => p.Title == title);
                if (existing != null)
                {
                    existing.Content = MedicalInstructions;
                    db.InstructionPresets.Update(existing);
                }
                else
                {
                    var preset = new InstructionPreset { Title = title.Trim(), Content = MedicalInstructions };
                    await db.InstructionPresets.AddAsync(preset);
                }
                await db.SaveChangesAsync();
                await LoadInstructionPresetsAsync();
            }
            catch { /* ignore */ }
        }

        public void Dispose()
        {
            StopPolling();
            _sharedStateService.CurrentPatientChanged -= OnSharedPatientChanged;
            GC.SuppressFinalize(this);
        }
    }
}
