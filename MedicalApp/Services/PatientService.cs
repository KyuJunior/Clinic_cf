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
                .Where(p => p.Name.Contains(searchTerm) || p.Phone.Contains(searchTerm) || p.Job.Contains(searchTerm) || p.Governorate.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<Patient>> SearchPatientsAdvancedAsync(
            string searchTerm,
            string gender = null,
            int? minAge = null,
            int? maxAge = null,
            string governorate = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string sortBy = null,
            bool isDescending = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Patients.Include(p => p.Visits).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Phone.Contains(searchTerm) || p.Job.Contains(searchTerm) || p.Governorate.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(gender) && gender != "الكل" && gender != "All")
            {
                query = query.Where(p => p.Gender == gender);
            }

            if (minAge.HasValue)
            {
                query = query.Where(p => p.Age >= minAge.Value);
            }

            if (maxAge.HasValue)
            {
                query = query.Where(p => p.Age <= maxAge.Value);
            }

            if (!string.IsNullOrWhiteSpace(governorate) && governorate != "الكل" && governorate != "All")
            {
                query = query.Where(p => p.Governorate == governorate);
            }

            if (startDate.HasValue)
            {
                var startVal = startDate.Value.Date;
                query = query.Where(p => p.Visits.Any(v => v.VisitDate >= startVal));
            }

            if (endDate.HasValue)
            {
                var endVal = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.Visits.Any(v => v.VisitDate <= endVal));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "Name")
                {
                    query = isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name);
                }
                else if (sortBy == "Age")
                {
                    query = isDescending ? query.OrderByDescending(p => p.Age) : query.OrderBy(p => p.Age);
                }
                else if (sortBy == "RegistrationDate")
                {
                    query = isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt);
                }
                else if (sortBy == "LastVisitDate")
                {
                    query = isDescending 
                        ? query.OrderByDescending(p => p.Visits.Max(v => (DateTime?)v.VisitDate) ?? DateTime.MinValue)
                        : query.OrderBy(p => p.Visits.Max(v => (DateTime?)v.VisitDate) ?? DateTime.MinValue);
                }
            }
            else
            {
                query = query.OrderBy(p => p.Name);
            }

            return await query.ToListAsync();
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

        public async Task AddVisitForCheckInAsync(Visit visit)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Visits.AddAsync(visit);
            await context.SaveChangesAsync();
        }
    }
}
