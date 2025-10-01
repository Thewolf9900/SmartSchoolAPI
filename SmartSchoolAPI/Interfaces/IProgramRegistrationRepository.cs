using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Interfaces
{
    public interface IProgramRegistrationRepository
    {
        Task<IEnumerable<ProgramRegistration>> GetRegistrationsByStatusAsync(RegistrationStatus? status);
        Task<ProgramRegistration> GetRegistrationByIdAsync(int registrationId);
        Task<ProgramRegistration> GetRegistrationByEmailAndProgramAsync(string email, int programId);
        Task<ProgramRegistration> GetPendingRegistrationByEmailAsync(string email);

        Task<ProgramRegistration> GetActiveRegistrationByEmailAsync(string email);
        Task<ProgramRegistration> GetActiveRegistrationByNationalIdAsync(string nationalId);

        void DeleteRegistration(ProgramRegistration registration);

        Task AddRegistrationAsync(ProgramRegistration registration);
        Task<bool> SaveChangesAsync();
    }
}