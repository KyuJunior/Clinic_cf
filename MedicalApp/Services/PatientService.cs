using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedicalApp.Data;
using MedicalApp.Models;

namespace MedicalApp.Services
{
    public class PatientService : IPatientService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public PatientService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Patients.ToListAsync();
        }

        public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await context.Patients.ToListAsync();
            }

            return await context.Patients
                .Where(p => p.Name.Contains(searchTerm) || p.Phone.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Patients
                .Include(p => p.Visits)
                .FirstOrDefaultAsync(p => p.PatientId == id);
        }

        public async Task AddPatientAsync(Patient patient)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Patients.AddAsync(patient);
            await context.SaveChangesAsync();
        }

        public async Task UpdatePatientAsync(Patient patient)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Patients.Update(patient);
            await context.SaveChangesAsync();
        }

        public async Task DeletePatientAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var patient = await context.Patients.FindAsync(id);
            if (patient != null)
            {
                context.Patients.Remove(patient);
                await context.SaveChangesAsync();
            }
        }
    }
}
