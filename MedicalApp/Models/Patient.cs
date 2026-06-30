using System;
using System.Collections.Generic;

namespace MedicalApp.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
        public ICollection<EchoRecord> EchoRecords { get; set; } = new List<EchoRecord>();
    }
}
