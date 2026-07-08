using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedicalApp.Data;
using MedicalApp.Models;

namespace MedicalApp.Services
{
    public class VisitService : IVisitService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public VisitService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Visit>> GetVisitsByPatientIdAsync(int patientId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Visits
                .Where(v => v.PatientId == patientId)
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();
        }

        public async Task AddVisitAsync(Visit visit)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Visits.AddAsync(visit);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Visit>> GetTodayVisitsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var today = DateTime.Today;
            return await context.Visits
                .Include(v => v.Patient)
                .Where(v => v.VisitDate >= today)
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Visit>> GetUpcomingAppointmentsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var today = DateTime.Today;
            return await context.Visits
                .Include(v => v.Patient)
                .Where(v => v.ReturnDate != null && v.ReturnDate >= today)
                .OrderBy(v => v.ReturnDate)
                .ToListAsync();
        }
    }
}
