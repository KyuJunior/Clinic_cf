namespace MedicalApp.Models
{
    public class PrintSettings
    {
        // ------------------ Prescription (Rx) Settings ------------------
        public string RxBackgroundPath { get; set; } = string.Empty;
        public bool PrintBackground { get; set; } = false;
        
        public double PatientNameX { get; set; } = 40;
        public double PatientNameY { get; set; } = 100;

        public double PatientAgeGenderX { get; set; } = 40;
        public double PatientAgeGenderY { get; set; } = 125;

        public double PatientDateX { get; set; } = 230;
        public double PatientDateY { get; set; } = 100;

        public double RxSymbolX { get; set; } = 40;
        public double RxSymbolY { get; set; } = 160;
        public bool ShowRxSymbol { get; set; } = true;

        public double DrugsX { get; set; } = 40;
        public double DrugsY { get; set; } = 200;

        public double FontSize { get; set; } = 14;

        // ------------------ Investigation (Inv) Settings ------------------
        public string InvBackgroundPath { get; set; } = string.Empty;
        public bool PrintInvBackground { get; set; } = false;

        public double InvPatientNameX { get; set; } = 40;
        public double InvPatientNameY { get; set; } = 100;

        public double InvPatientAgeGenderX { get; set; } = 40;
        public double InvPatientAgeGenderY { get; set; } = 125;

        public double InvPatientDateX { get; set; } = 230;
        public double InvPatientDateY { get; set; } = 100;

        public double InvContentX { get; set; } = 40;
        public double InvContentY { get; set; } = 200;

        public double InvFontSize { get; set; } = 14;

        // ------------------ Imaging (Img) Settings ------------------
        public string ImgBackgroundPath { get; set; } = string.Empty;
        public bool PrintImgBackground { get; set; } = false;

        public double ImgPatientNameX { get; set; } = 40;
        public double ImgPatientNameY { get; set; } = 100;

        public double ImgPatientAgeGenderX { get; set; } = 40;
        public double ImgPatientAgeGenderY { get; set; } = 125;

        public double ImgPatientDateX { get; set; } = 230;
        public double ImgPatientDateY { get; set; } = 100;

        public double ImgContentX { get; set; } = 40;
        public double ImgContentY { get; set; } = 200;

        public double ImgFontSize { get; set; } = 14;

        // Clinic Profile Settings (Placeholders)
        public string ClinicNameAr { get; set; } = "عيادتي التخصصية";
        public string ClinicNameEn { get; set; } = "My Specialty Clinic";
        public string ClinicPhone { get; set; } = "+964 770 123 4567";
        public string ClinicAddress { get; set; } = "Baghdad, Iraq";
        public string ClinicSpecialty { get; set; } = "Gynecology & Obstetrics | التوليد وأمراض النساء";

        // Database & Backups (Placeholders)
        public string DbBackupPath { get; set; } = @"C:\Myapps\Backups";
        public string DbBackupInterval { get; set; } = "Daily | يومي";
        public bool DbAutoBackupEnabled { get; set; } = true;

        // Staff Settings (Placeholders)
        public string AdminPassword { get; set; } = "••••••••";
        public bool RequireLogin { get; set; } = false;

        // Home Dashboard Polling and Customizations
        public int QueuePollingInterval { get; set; } = 3;
        public bool ShowTotalPatientsCard { get; set; } = true;
        public bool ShowNewPatientsCard { get; set; } = true;
        public bool ShowWaitingPatientsCard { get; set; } = true;
        public bool ShowActiveConsultationsCard { get; set; } = true;
        public bool ShowExamRoomCard { get; set; } = true;
        public bool ShowNextPatientCard { get; set; } = true;
        public bool ShowTotalVisitsCard { get; set; } = true;
        public bool ShowCompletedConsultationsCard { get; set; } = true;
    }
}
