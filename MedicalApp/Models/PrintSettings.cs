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
    }
}
