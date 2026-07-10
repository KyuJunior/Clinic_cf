using System;
using MedicalApp.Models;

namespace MedicalApp.Services
{
    public interface ISharedStateService
    {
        Patient? CurrentPatient { get; set; }
        string? ActiveDoctorName { get; set; }
        event Action<Patient?>? CurrentPatientChanged;
    }
}
