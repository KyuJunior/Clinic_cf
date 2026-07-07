using System.Collections.Generic;
using System.Threading.Tasks;
using MedicalApp.Models;

namespace MedicalApp.Services
{
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm);
        Task<Patient?> GetPatientByIdAsync(int id);
        Task AddPatientAsync(Patient patient);
        Task UpdatePatientAsync(Patient patient);
        Task DeletePatientAsync(int id);
        Task AddVisitForCheckInAsync(Visit visit);
    }
}
