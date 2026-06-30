using System;

namespace MedicalApp.Models
{
    public class EchoRecord
    {
        public int EchoRecordId { get; set; }
        public int PatientId { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty; // Network UNC path (e.g. \\192.168.1.100\EchoFiles\filename.mp4)
        public string Notes { get; set; } = string.Empty;

        // Navigation Property
        public Patient? Patient { get; set; }
    }
}
