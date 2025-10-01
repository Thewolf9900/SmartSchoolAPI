using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;
using SmartSchoolAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Repositories
{
    public class ProgramRegistrationRepository : IProgramRegistrationRepository
    {
        private readonly SmartSchoolDbContext _context;

        public ProgramRegistrationRepository(SmartSchoolDbContext context)
        {
            _context = context;
        }

        public async Task<ProgramRegistration> GetRegistrationByIdAsync(int registrationId)
        {
            return await _context.ProgramRegistrations
                .Include(r => r.AcademicProgram)
                .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);
        }

        public async Task<IEnumerable<ProgramRegistration>> GetRegistrationsByStatusAsync(RegistrationStatus? status)
        {
            var query = _context.ProgramRegistrations.Include(r => r.AcademicProgram).AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query.OrderByDescending(r => r.RequestDate).ToListAsync();
        }

        public async Task<ProgramRegistration> GetRegistrationByEmailAndProgramAsync(string email, int programId)
        {
            return await _context.ProgramRegistrations
                .FirstOrDefaultAsync(r => r.Email.ToLower() == email.ToLower() && r.AcademicProgramId == programId);
        }


        public async Task<ProgramRegistration> GetPendingRegistrationByEmailAsync(string email)
        {
            // الآن نبحث عن أي طلب لم تتم الموافقة عليه بعد
            return await _context.ProgramRegistrations
                .Include(r => r.AcademicProgram)
                .Where(r => r.Status != Enums.RegistrationStatus.Approved)
                .FirstOrDefaultAsync(r => r.Email.ToLower() == email.ToLower());
        }
        public async Task<ProgramRegistration> GetActiveRegistrationByEmailAsync(string email)
        {
            return await _context.ProgramRegistrations
                .FirstOrDefaultAsync(r => r.Email.ToLower() == email.ToLower()
                                     && r.Status != RegistrationStatus.Rejected);
        }

        public async Task<ProgramRegistration> GetActiveRegistrationByNationalIdAsync(string nationalId)
        {
            return await _context.ProgramRegistrations
                .FirstOrDefaultAsync(r => r.NationalId == nationalId
                                     && r.Status != RegistrationStatus.Rejected);
        }
        public void DeleteRegistration(ProgramRegistration registration)
        {
            _context.ProgramRegistrations.Remove(registration);
        }

        public async Task AddRegistrationAsync(ProgramRegistration registration)
        {
            await _context.ProgramRegistrations.AddAsync(registration);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}