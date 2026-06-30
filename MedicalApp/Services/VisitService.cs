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
    }
}
