namespace MedicalApp.Models
{
    public class PrintSettings
    {
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
    }
}
