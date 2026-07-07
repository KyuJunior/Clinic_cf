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
        public string Job { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public int AgeMonths { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? SpouseBirthDate { get; set; }
        public string HasChildren { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string PatientFiles { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string Height { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        public string SpouseName { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string Smoking { get; set; } = string.Empty;
        public DateTime? LastChildBirthDate { get; set; }
        public string Alcohol { get; set; } = string.Empty;
        public DateTime? MarriageDate { get; set; }
        public string ReferredBy { get; set; } = string.Empty;
        public string SpouseBloodGroup { get; set; } = string.Empty;
        public string Allergy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    }
}
