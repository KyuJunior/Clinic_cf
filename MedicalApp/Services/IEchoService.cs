using System.Collections.Generic;
using System.Threading.Tasks;
using MedicalApp.Models;

namespace MedicalApp.Services
{
    public interface IEchoService
    {
        Task<IEnumerable<EchoRecord>> GetEchoRecordsByPatientIdAsync(int patientId);
        Task AddEchoRecordAsync(EchoRecord record);
    }
}
