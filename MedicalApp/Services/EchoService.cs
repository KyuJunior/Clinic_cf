using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedicalApp.Data;
using MedicalApp.Models;

namespace MedicalApp.Services
{
    public class EchoService : IEchoService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public EchoService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<EchoRecord>> GetEchoRecordsByPatientIdAsync(int patientId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.EchoRecords
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.UploadDate)
                .ToListAsync();
        }

        public async Task AddEchoRecordAsync(EchoRecord record)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.EchoRecords.AddAsync(record);
            await context.SaveChangesAsync();
        }
    }
}
