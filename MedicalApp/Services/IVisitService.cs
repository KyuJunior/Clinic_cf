using System.Collections.Generic;
using System.Threading.Tasks;
using MedicalApp.Models;

namespace MedicalApp.Services
{
    public interface IVisitService
    {
        Task<IEnumerable<Visit>> GetVisitsByPatientIdAsync(int patientId);
        Task AddVisitAsync(Visit visit);
        Task<IEnumerable<Visit>> GetTodayVisitsAsync();
        Task<IEnumerable<Visit>> GetUpcomingAppointmentsAsync();
    }
}
