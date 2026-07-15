using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicalApp.Models;

namespace MedicalApp.ViewModels
{
    public partial class ClinicalExamViewModel
    {
        // ==========================================
        // Sidebar Navigation
        // ==========================================
        [ObservableProperty]
        private bool _isSectionsTabActive;

        // ==========================================
        // DialogHost Settings
        // ==========================================
        [ObservableProperty]
        private bool _isDialogOpen;

        [ObservableProperty]
        private string _dialogHeader = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _dialogTemplateItems = new();

        [ObservableProperty]
        private string _activeDialogTargetField = string.Empty;

        [ObservableProperty]
        private bool _isTemplateDialog = true;

        [ObservableProperty]
        private string _activeSettingsTitle = string.Empty;

        public bool IsObstetricsSettingsActive => ActiveSettingsTitle == "Obstetrics";
        public bool IsComplaintsSettingsActive => ActiveSettingsTitle == "Complaints";
        public bool IsNutritionalSettingsActive => ActiveSettingsTitle == "Nutritional";
        public bool IsSummerySettingsActive => ActiveSettingsTitle == "Summery";
        public bool IsDentistSettingsActive => ActiveSettingsTitle == "Dentist";
        public bool IsVitalSignsSettingsActive => ActiveSettingsTitle == "Vital Signs";
        public bool IsSystemReviewSettingsActive => ActiveSettingsTitle == "System Review";
        public bool IsExaminationSettingsActive => ActiveSettingsTitle == "Examination";
        public bool IsLabsSettingsActive => ActiveSettingsTitle == "Labs Section";
        public bool IsDiagnosisSettingsActive => ActiveSettingsTitle == "Diagnosis";
        public bool IsRxSettingsActive => ActiveSettingsTitle == "Medications (Rx)";

        partial void OnActiveSettingsTitleChanged(string value)
        {
            OnPropertyChanged(nameof(IsObstetricsSettingsActive));
            OnPropertyChanged(nameof(IsComplaintsSettingsActive));
            OnPropertyChanged(nameof(IsNutritionalSettingsActive));
            OnPropertyChanged(nameof(IsSummerySettingsActive));
            OnPropertyChanged(nameof(IsDentistSettingsActive));
            OnPropertyChanged(nameof(IsVitalSignsSettingsActive));
            OnPropertyChanged(nameof(IsSystemReviewSettingsActive));
            OnPropertyChanged(nameof(IsExaminationSettingsActive));
            OnPropertyChanged(nameof(IsLabsSettingsActive));
            OnPropertyChanged(nameof(IsDiagnosisSettingsActive));
            OnPropertyChanged(nameof(IsRxSettingsActive));
        }

        // ==========================================
        // Sub-field visibility switches (default: true)
        // ==========================================
        [ObservableProperty] private bool _obShowObstetricsInfo = true;
        [ObservableProperty] private bool _obShowLMP = true;
        [ObservableProperty] private bool _obShowLastUltrasound = true;
        [ObservableProperty] private bool _obShowGravidity = true;
        [ObservableProperty] private bool _obShowEDD = true;
        [ObservableProperty] private bool _obShowParity = true;
        [ObservableProperty] private bool _obShowGestationalAge = true;
        [ObservableProperty] private bool _obShowAbortion = true;
        [ObservableProperty] private bool _obShowAbortionNote = true;
        [ObservableProperty] private bool _obShowLastUltrasoundNote = true;

        [ObservableProperty] private bool _nutriShowWeight = true;
        [ObservableProperty] private bool _nutriShowHeight = true;
        [ObservableProperty] private bool _nutriShowBMI = true;
        [ObservableProperty] private bool _nutriShowDietNotes = true;

        [ObservableProperty] private bool _compShowChiefComplaint = true;
        [ObservableProperty] private bool _compShowHPI = true;
        [ObservableProperty] private bool _compShowTreatmentPlan = true;

        [ObservableProperty] private bool _summeryShowVisitSummary = true;
        [ObservableProperty] private bool _dentistShowNotes = true;

        [ObservableProperty] private bool _vitalsShowBP = true;
        [ObservableProperty] private bool _vitalsShowHR = true;
        [ObservableProperty] private bool _vitalsShowSpO2 = true;
        [ObservableProperty] private bool _vitalsShowTemp = true;
        [ObservableProperty] private bool _vitalsShowBS = true;

        [ObservableProperty] private bool _labsShowInvestigations = true;
        [ObservableProperty] private bool _labsShowImaging = true;
        [ObservableProperty] private bool _labsShowInterventions = true;

        [ObservableProperty] private bool _diagShowNotes = true;
        [ObservableProperty] private bool _diagShowDiseases = true;

        [ObservableProperty] private bool _rxShowListing = true;
        [ObservableProperty] private bool _rxShowAddForm = true;

        [ObservableProperty] private bool _sysShowCVS = true;
        [ObservableProperty] private bool _sysShowResp = true;
        [ObservableProperty] private bool _sysShowGI = true;
        [ObservableProperty] private bool _sysShowCNS = true;
        [ObservableProperty] private bool _sysShowOther = true;

        [ObservableProperty] private bool _examShowPositive = true;
        [ObservableProperty] private bool _examShowNegative = true;
        [ObservableProperty] private bool _examShowGeneral = true;

        // ==========================================
        // New clinical data fields
        // ==========================================
        [ObservableProperty] private DateTime? _obLastUltrasound;
        [ObservableProperty] private DateTime? _obEDD;
        [ObservableProperty] private string _obGestationalAge = string.Empty;
        [ObservableProperty] private string _obAbortionNote = string.Empty;
        [ObservableProperty] private string _obLastUltrasoundNote = string.Empty;

        [ObservableProperty] private string _vitalBS = string.Empty;
        [ObservableProperty] private string _diagnosisNotes = string.Empty;


        // ==========================================
        // Group Visibility Settings (Show/Hide groups)
        // ==========================================
        [ObservableProperty]
        private bool _showOverviewSection = true;

        [ObservableProperty]
        private bool _showReportsSection = true;

        [ObservableProperty]
        private bool _showDentistSection = false;

        [ObservableProperty]
        private bool _showVitalsSection = true;

        [ObservableProperty]
        private bool _showSystemReviewSection = false;

        [ObservableProperty]
        private bool _showExaminationSection = false;

        [ObservableProperty]
        private bool _showLabsSection = true;

        // ==========================================
        // Sub-section Visibility Settings (Show/Hide cards)
        // ==========================================
        [ObservableProperty]
        private bool _showComplaintsSub = true;

        [ObservableProperty]
        private bool _showNutritionalSub = true;

        [ObservableProperty]
        private bool _showObstetricsSub = true;

        [ObservableProperty]
        private bool _showSummarySub = true;

        [ObservableProperty]
        private bool _showCustomFieldsSub = true;

        [ObservableProperty]
        private bool _showReportsSub = true;

        [ObservableProperty]
        private bool _showDentistSub = true;

        [ObservableProperty]
        private bool _showSystemReviewSub = true;

        [ObservableProperty]
        private bool _showPositiveSignsSub = true;

        [ObservableProperty]
        private bool _showExamNotesSub = true;

        [ObservableProperty]
        private bool _showNegativeSignsSub = true;

        [ObservableProperty]
        private bool _showInvestigationSub = true;

        [ObservableProperty]
        private bool _showImagingSub = true;

        [ObservableProperty]
        private bool _showInterventionSub = true;

        // ==========================================
        // Clinical Data Fields
        // ==========================================
        [ObservableProperty]
        private string _nutritionalWeight = string.Empty;

        [ObservableProperty]
        private string _nutritionalHeight = string.Empty;

        [ObservableProperty]
        private string _nutritionalDietNotes = string.Empty;

        [ObservableProperty]
        private double? _nutritionalBMI;

        [ObservableProperty]
        private string _bmiCategory = "N/A";

        [ObservableProperty]
        private string _bmiColor = "#64748B";

        [ObservableProperty]
        private string _obGravida = string.Empty;

        [ObservableProperty]
        private string _obPara = string.Empty;

        [ObservableProperty]
        private string _obAbortion = string.Empty;

        [ObservableProperty]
        private DateTime? _obLMP;

        public bool ShowObstetricsCard => ShowObstetrics && CurrentPatient?.Gender == "Female";

        [ObservableProperty]
        private string _visitSummaryText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<CustomFieldEntry> _customFields = new();

        [ObservableProperty]
        private string _dentalNotesText = string.Empty;

        [ObservableProperty]
        private string _sysReviewCVS = string.Empty;

        [ObservableProperty]
        private string _sysReviewResp = string.Empty;

        [ObservableProperty]
        private string _sysReviewGI = string.Empty;

        [ObservableProperty]
        private string _sysReviewCNS = string.Empty;

        [ObservableProperty]
        private string _sysReviewOther = string.Empty;

        [ObservableProperty]
        private string _examPositiveSigns = string.Empty;

        [ObservableProperty]
        private string _examNotes = string.Empty;

        [ObservableProperty]
        private string _examNegativeSigns = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _addedInterventions = new();

        [ObservableProperty]
        private string _selectedIntervention = string.Empty;

        // ==========================================
        // Calculation & Property Change Handlers
        // ==========================================
        partial void OnNutritionalWeightChanged(string value) => CalculateBMI();
        partial void OnNutritionalHeightChanged(string value) => CalculateBMI();

        private void CalculateBMI()
        {
            if (double.TryParse(NutritionalWeight, out double w) && double.TryParse(NutritionalHeight, out double h) && h > 0)
            {
                double hMeter = h / 100.0;
                double bmi = w / (hMeter * hMeter);
                NutritionalBMI = Math.Round(bmi, 1);
                if (bmi < 18.5) { BmiCategory = "Underweight"; BmiColor = "#0284C7"; } // Sky
                else if (bmi < 25) { BmiCategory = "Normal"; BmiColor = "#0D9488"; } // Teal
                else if (bmi < 30) { BmiCategory = "Overweight"; BmiColor = "#D97706"; } // Amber
                else { BmiCategory = "Obese"; BmiColor = "#EF4444"; } // Red
            }
            else
            {
                NutritionalBMI = null;
                BmiCategory = "N/A";
                BmiColor = "#64748B"; // Slate
            }
        }

        // ==========================================
        // Data Serialization & Parsing Synchronization
        // ==========================================
        public void SyncPhysicalExamination()
        {
            var lines = new List<string>();
            if (!string.IsNullOrWhiteSpace(ExamPositiveSigns)) lines.Add($"+ve Signs: {ExamPositiveSigns}");
            if (!string.IsNullOrWhiteSpace(ExamNotes)) lines.Add($"Notes: {ExamNotes}");
            if (!string.IsNullOrWhiteSpace(ExamNegativeSigns)) lines.Add($"-ve Signs: {ExamNegativeSigns}");
            if (!string.IsNullOrWhiteSpace(DentalNotesText)) lines.Add($"Dental: {DentalNotesText}");
            if (!string.IsNullOrWhiteSpace(SysReviewCVS) || !string.IsNullOrWhiteSpace(SysReviewResp) || !string.IsNullOrWhiteSpace(SysReviewGI) || !string.IsNullOrWhiteSpace(SysReviewCNS) || !string.IsNullOrWhiteSpace(SysReviewOther))
            {
                var sr = new List<string>();
                if (!string.IsNullOrWhiteSpace(SysReviewCVS)) sr.Add($"CVS: {SysReviewCVS}");
                if (!string.IsNullOrWhiteSpace(SysReviewResp)) sr.Add($"Resp: {SysReviewResp}");
                if (!string.IsNullOrWhiteSpace(SysReviewGI)) sr.Add($"GI: {SysReviewGI}");
                if (!string.IsNullOrWhiteSpace(SysReviewCNS)) sr.Add($"CNS: {SysReviewCNS}");
                if (!string.IsNullOrWhiteSpace(SysReviewOther)) sr.Add($"Other: {SysReviewOther}");
                lines.Add($"System Review [{string.Join(", ", sr)}]");
            }
            
            PhysicalExamination = string.Join(Environment.NewLine, lines);
        }

        public void SyncTreatmentPlan()
        {
            var lines = new List<string>();
            if (!string.IsNullOrWhiteSpace(VisitSummaryText)) lines.Add($"Summary: {VisitSummaryText}");
            if (NutritionalBMI.HasValue) lines.Add($"Nutrition: Wt: {NutritionalWeight}kg, Ht: {NutritionalHeight}cm, BMI: {NutritionalBMI} ({BmiCategory})");
            if (!string.IsNullOrWhiteSpace(NutritionalDietNotes)) lines.Add($"Diet Notes: {NutritionalDietNotes}");
            if (CurrentPatient?.Gender == "Female" && (!string.IsNullOrWhiteSpace(ObGravida) || !string.IsNullOrWhiteSpace(ObPara) || !string.IsNullOrWhiteSpace(ObAbortion)))
            {
                lines.Add($"OB/GYN: G{ObGravida} P{ObPara} A{ObAbortion}" + (ObLMP.HasValue ? $" LMP: {ObLMP:dd/MM/yyyy}" : ""));
            }
            if (AddedInterventions.Any()) lines.Add($"Interventions: {string.Join(", ", AddedInterventions)}");
            if (CustomFields.Any()) lines.Add($"Custom Fields: {string.Join(", ", CustomFields.Select(f => $"{f.Label}: {f.Value}"))}");
            
            TreatmentPlan = string.Join(Environment.NewLine, lines);
        }

        public void ParsePhysicalExamination(string text)
        {
            // Reset fields
            ExamPositiveSigns = string.Empty;
            ExamNotes = string.Empty;
            ExamNegativeSigns = string.Empty;
            DentalNotesText = string.Empty;
            SysReviewCVS = string.Empty;
            SysReviewResp = string.Empty;
            SysReviewGI = string.Empty;
            SysReviewCNS = string.Empty;
            SysReviewOther = string.Empty;

            if (string.IsNullOrWhiteSpace(text)) return;
            var lines = text.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("+ve Signs: ")) ExamPositiveSigns = line.Substring(11);
                else if (line.StartsWith("Notes: ")) ExamNotes = line.Substring(7);
                else if (line.StartsWith("-ve Signs: ")) ExamNegativeSigns = line.Substring(11);
                else if (line.StartsWith("Dental: ")) DentalNotesText = line.Substring(8);
                else if (line.StartsWith("System Review [") && line.EndsWith("]"))
                {
                    var systemsStr = line.Substring(15, line.Length - 16);
                    var parts = systemsStr.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("CVS: ")) SysReviewCVS = part.Substring(5);
                        else if (part.StartsWith("Resp: ")) SysReviewResp = part.Substring(6);
                        else if (part.StartsWith("GI: ")) SysReviewGI = part.Substring(4);
                        else if (part.StartsWith("CNS: ")) SysReviewCNS = part.Substring(5);
                        else if (part.StartsWith("Other: ")) SysReviewOther = part.Substring(7);
                    }
                }
            }
        }

        public void ParseTreatmentPlan(string text)
        {
            // Reset fields
            VisitSummaryText = string.Empty;
            NutritionalWeight = string.Empty;
            NutritionalHeight = string.Empty;
            NutritionalDietNotes = string.Empty;
            ObGravida = string.Empty;
            ObPara = string.Empty;
            ObAbortion = string.Empty;
            ObLMP = null;
            AddedInterventions.Clear();
            CustomFields.Clear();

            if (string.IsNullOrWhiteSpace(text)) return;
            var lines = text.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("Summary: ")) VisitSummaryText = line.Substring(9);
                else if (line.StartsWith("Nutrition: "))
                {
                    var parts = line.Substring(11).Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("Wt: ") && part.EndsWith("kg")) NutritionalWeight = part.Substring(4, part.Length - 6);
                        else if (part.StartsWith("Ht: ") && part.EndsWith("cm")) NutritionalHeight = part.Substring(4, part.Length - 6);
                    }
                }
                else if (line.StartsWith("Diet Notes: ")) NutritionalDietNotes = line.Substring(12);
                else if (line.StartsWith("OB/GYN: "))
                {
                    var parts = line.Substring(8).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (part.StartsWith("G")) ObGravida = part.Substring(1);
                        else if (part.StartsWith("P")) ObPara = part.Substring(1);
                        else if (part.StartsWith("A")) ObAbortion = part.Substring(1);
                        else if (part.StartsWith("LMP:"))
                        {
                            if (DateTime.TryParseExact(part.Substring(4), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var lmp))
                            {
                                ObLMP = lmp;
                            }
                        }
                    }
                }
                else if (line.StartsWith("Interventions: "))
                {
                    var parts = line.Substring(15).Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in parts) AddedInterventions.Add(p);
                }
                else if (line.StartsWith("Custom Fields: "))
                {
                    var parts = line.Substring(15).Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var kv = part.Split(new[] { ": " }, StringSplitOptions.None);
                        if (kv.Length == 2)
                        {
                            CustomFields.Add(new CustomFieldEntry { Label = kv[0], Value = kv[1] });
                        }
                    }
                }
            }
        }

        // ==========================================
        // Commands
        // ==========================================
        [RelayCommand]
        public void ToggleSectionsTab()
        {
            IsSectionsTabActive = !IsSectionsTabActive;
        }

        [RelayCommand]
        public void OpenSectionSettings(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName)) return;
            IsTemplateDialog = false;
            ActiveSettingsTitle = sectionName;
            IsDialogOpen = true;
        }

        [RelayCommand]
        public void AddCustomField()
        {
            CustomFields.Add(new CustomFieldEntry());
            TriggerAutoSave();
        }

        [RelayCommand]
        public void RemoveCustomField(CustomFieldEntry item)
        {
            if (item != null)
            {
                CustomFields.Remove(item);
                TriggerAutoSave();
            }
        }

        [RelayCommand]
        public void AddIntervention()
        {
            if (!string.IsNullOrWhiteSpace(SelectedIntervention))
            {
                AddedInterventions.Add(SelectedIntervention.Trim());
                SelectedIntervention = string.Empty;
                TriggerAutoSave();
            }
        }

        [RelayCommand]
        public void RemoveIntervention(string item)
        {
            if (item != null)
            {
                AddedInterventions.Remove(item);
                TriggerAutoSave();
            }
        }

        [RelayCommand]
        public void OpenConfigureSection(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName)) return;
            DialogHeader = $"{sectionName} Quick Templates";
            ActiveDialogTargetField = sectionName;
            DialogTemplateItems.Clear();

            if (sectionName == "Complaints")
            {
                DialogTemplateItems.Add("Routine follow-up visit.");
                DialogTemplateItems.Add("Acute onset of symptoms starting 3 days ago.");
                DialogTemplateItems.Add("Chronic condition management check-up.");
            }
            else if (sectionName == "Nutritional")
            {
                DialogTemplateItems.Add("Advised low-salt, low-cholesterol diet.");
                DialogTemplateItems.Add("Advised calorie deficit and regular exercise.");
                DialogTemplateItems.Add("Recommended high-protein diet.");
            }
            else if (sectionName == "Obstetrics")
            {
                DialogTemplateItems.Add("Patient is currently pregnant.");
                DialogTemplateItems.Add("LMP date logged; patient reporting regular cycles.");
                DialogTemplateItems.Add("Routine gynecological check-up.");
            }
            else if (sectionName == "Summary")
            {
                DialogTemplateItems.Add("Patient counselled on diagnosis and treatment compliance.");
                DialogTemplateItems.Add("Advised to return immediately if symptoms worsen.");
            }
            else if (sectionName == "Custom Fields")
            {
                DialogTemplateItems.Add("Add standard custom parameters.");
            }
            else if (sectionName == "Reports")
            {
                DialogTemplateItems.Add("Previous lab report reviewed.");
                DialogTemplateItems.Add("External radiological report attached.");
            }
            else if (sectionName == "Dentist")
            {
                DialogTemplateItems.Add("Routine dental scale and polish completed.");
                DialogTemplateItems.Add("Tooth restoration completed on tooth #36.");
            }
            else if (sectionName == "System Review")
            {
                DialogTemplateItems.Add("CVS: S1 S2 normal, no murmurs.");
                DialogTemplateItems.Add("Resp: Chest clear, equal bilateral air entry.");
                DialogTemplateItems.Add("GI: Abdomen soft, non-tender, active bowel sounds.");
            }
            else if (sectionName == "Examination")
            {
                DialogTemplateItems.Add("Positive Signs: +ve localized tenderness.");
                DialogTemplateItems.Add("Negative Signs: No pallor, no peripheral edema.");
            }
            else if (sectionName == "Labs Section")
            {
                DialogTemplateItems.Add("CBC + Lipid Profile requested.");
                DialogTemplateItems.Add("Renal Profile + Liver Enzymes requested.");
            }

            IsTemplateDialog = true;
            IsDialogOpen = true;
        }

        [RelayCommand]
        public void InsertTemplateText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            IsDialogOpen = false;

            if (ActiveDialogTargetField == "Complaints")
            {
                ChiefComplaint = string.IsNullOrEmpty(ChiefComplaint) ? text : $"{ChiefComplaint} {text}";
            }
            else if (ActiveDialogTargetField == "Nutritional")
            {
                NutritionalDietNotes = string.IsNullOrEmpty(NutritionalDietNotes) ? text : $"{NutritionalDietNotes} {text}";
            }
            else if (ActiveDialogTargetField == "Summary")
            {
                VisitSummaryText = string.IsNullOrEmpty(VisitSummaryText) ? text : $"{VisitSummaryText} {text}";
            }
            else if (ActiveDialogTargetField == "Dentist")
            {
                DentalNotesText = string.IsNullOrEmpty(DentalNotesText) ? text : $"{DentalNotesText} {text}";
            }
            else if (ActiveDialogTargetField == "System Review")
            {
                SysReviewOther = string.IsNullOrEmpty(SysReviewOther) ? text : $"{SysReviewOther} {text}";
            }
            else if (ActiveDialogTargetField == "Examination")
            {
                ExamNotes = string.IsNullOrEmpty(ExamNotes) ? text : $"{ExamNotes} {text}";
            }
            
            TriggerAutoSave();
        }

        [ObservableProperty]
        private bool _isSummaryTabActive = true;

        [RelayCommand]
        public void SelectSummaryTab()
        {
            IsSummaryTabActive = true;
        }

        [RelayCommand]
        public void SelectConsultationTab()
        {
            IsSummaryTabActive = false;
        }

        partial void OnVisitHistoryChanged(System.Collections.ObjectModel.ObservableCollection<MedicalApp.Models.Visit> value)
        {
            OnPropertyChanged(nameof(HistoryDiagnoses));
            OnPropertyChanged(nameof(VitalsTrendPoints));
            OnPropertyChanged(nameof(RecentVisitsList));
        }

        public System.Collections.Generic.List<string> HistoryDiagnoses
        {
            get
            {
                if (VisitHistory == null) return new System.Collections.Generic.List<string>();
                return VisitHistory
                    .Select(v => v.Diagnosis)
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Distinct()
                    .ToList();
            }
        }

        public System.Collections.Generic.List<MedicalApp.Models.Visit> RecentVisitsList
        {
            get
            {
                if (VisitHistory == null) return new System.Collections.Generic.List<MedicalApp.Models.Visit>();
                return VisitHistory
                    .OrderByDescending(v => v.VisitDate)
                    .Take(5)
                    .ToList();
            }
        }

        public string VitalsTrendPoints
        {
            get
            {
                if (VisitHistory == null || VisitHistory.Count == 0)
                    return "10,25 110,25";

                var list = VisitHistory
                    .OrderBy(v => v.VisitDate)
                    .Select(v => {
                        double.TryParse(v.VitalsSBP, out double sbp);
                        return sbp;
                    })
                    .Where(sbp => sbp > 0)
                    .ToList();

                if (list.Count == 0)
                    return "10,25 110,25";

                if (list.Count == 1)
                    return "10,25 110,25";

                var points = new System.Collections.Generic.List<string>();
                int count = Math.Min(list.Count, 5);
                var subList = list.Skip(list.Count - count).ToList();

                double minVal = subList.Min();
                double maxVal = subList.Max();
                double valRange = maxVal - minVal;
                if (valRange == 0) valRange = 1.0;

                for (int i = 0; i < count; i++)
                {
                    double x = 10 + i * (100.0 / (count - 1));
                    double y = 45 - ((subList[i] - minVal) / valRange * 40);
                    points.Add($"{x:0.0},{y:0.0}");
                }

                return string.Join(" ", points);
            }
        }

        public string CurrentDateString => DateTime.Today.ToString("dddd, MMM dd, yyyy");

        public System.Collections.Generic.List<MedicalApp.Models.Patient> RecentDoctorPatients
        {
            get
            {
                var list = new System.Collections.Generic.List<MedicalApp.Models.Patient>();
                if (VisitHistory != null && VisitHistory.Count > 0)
                {
                    var uniquePatients = VisitHistory
                        .Select(v => v.Patient)
                        .Where(p => p != null)
                        .Select(p => p!)
                        .GroupBy(p => p.PatientId)
                        .Select(g => g.First())
                        .Take(3)
                        .ToList();
                    list.AddRange(uniquePatients);
                }
                
                if (list.Count < 3)
                {
                    if (!list.Any(p => p.Name.Contains("John Smith")))
                        list.Add(new Patient { Name = "John Smith", Phone = "JDS78941", Gender = "Male", Age = 38 });
                    if (!list.Any(p => p.Name.Contains("Maria Garcia")))
                        list.Add(new Patient { Name = "Maria Garcia", Phone = "MG88472", Gender = "Female", Age = 29 });
                    if (list.Count < 3 && !list.Any(p => p.Name.Contains("Lisa Chen")))
                        list.Add(new Patient { Name = "Lisa Chen", Phone = "LC99281", Gender = "Female", Age = 31 });
                }
                return list.Take(3).ToList();
            }
        }

        public bool HasWaitingPatients => WaitingPatients != null && WaitingPatients.Count > 0;
        public bool HasNoWaitingPatients => WaitingPatients == null || WaitingPatients.Count == 0;

        public bool HasHoldPatients => NotFinishedPatients != null && NotFinishedPatients.Count > 0;
        public bool HasNoHoldPatients => NotFinishedPatients == null || NotFinishedPatients.Count == 0;

        [ObservableProperty]
        private string _headerSearchTerm = string.Empty;

        partial void OnHeaderSearchTermChanged(string value)
        {
            OnPropertyChanged(nameof(FilteredWaitingPatients));
            OnPropertyChanged(nameof(HasFilteredWaitingPatients));
            OnPropertyChanged(nameof(HasNoFilteredWaitingPatients));
        }

        public System.Collections.Generic.IEnumerable<QueueEntry> FilteredWaitingPatients
        {
            get
            {
                if (string.IsNullOrWhiteSpace(HeaderSearchTerm))
                    return WaitingPatients;
                return WaitingPatients.Where(q => q.PatientName != null && q.PatientName.Contains(HeaderSearchTerm, StringComparison.OrdinalIgnoreCase));
            }
        }

        public bool HasFilteredWaitingPatients => FilteredWaitingPatients != null && FilteredWaitingPatients.Any();
        public bool HasNoFilteredWaitingPatients => FilteredWaitingPatients == null || !FilteredWaitingPatients.Any();

        partial void OnWaitingPatientsChanged(ObservableCollection<QueueEntry> value)
        {
            if (value != null)
            {
                value.CollectionChanged += (s, e) => {
                    OnPropertyChanged(nameof(HasWaitingPatients));
                    OnPropertyChanged(nameof(HasNoWaitingPatients));
                    OnPropertyChanged(nameof(FilteredWaitingPatients));
                    OnPropertyChanged(nameof(HasFilteredWaitingPatients));
                    OnPropertyChanged(nameof(HasNoFilteredWaitingPatients));
                };
            }
            OnPropertyChanged(nameof(HasWaitingPatients));
            OnPropertyChanged(nameof(HasNoWaitingPatients));
            OnPropertyChanged(nameof(FilteredWaitingPatients));
            OnPropertyChanged(nameof(HasFilteredWaitingPatients));
            OnPropertyChanged(nameof(HasNoFilteredWaitingPatients));
        }

        partial void OnNotFinishedPatientsChanged(ObservableCollection<QueueEntry> value)
        {
            if (value != null)
            {
                value.CollectionChanged += (s, e) => {
                    OnPropertyChanged(nameof(HasHoldPatients));
                    OnPropertyChanged(nameof(HasNoHoldPatients));
                };
            }
            OnPropertyChanged(nameof(HasHoldPatients));
            OnPropertyChanged(nameof(HasNoHoldPatients));
        }

        [RelayCommand]
        public async Task SelectRecentPatientAsync(Patient patient)
        {
            if (patient == null) return;
            var fullPatient = await _patientService.GetPatientByIdAsync(patient.PatientId);
            SelectedPatientLookup = fullPatient;
            StatusMessage = $"Exam session loaded for recent patient '{patient.Name}'.";
        }
    }
}
