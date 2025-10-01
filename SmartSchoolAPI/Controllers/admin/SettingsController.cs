// هذا الكنترولر سيحتاج إلى DbContext مباشرة لإدارة جدول الإعدادات
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Entities;
using System.Threading.Tasks;

namespace SmartSchoolAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/settings")]
    [Authorize(Roles = "Administrator")]
    public class SettingsController : ControllerBase
    {
        private readonly SmartSchoolDbContext _context;

        public SettingsController(SmartSchoolDbContext context)
        {
            _context = context;
        }

        [HttpGet("payment")]
        public async Task<IActionResult> GetPaymentSettings()
        {
            var settings = await _context.PaymentSettings.FindAsync(1);
            if (settings == null) return NotFound("لم يتم العثور على إعدادات الدفع.");
            return Ok(settings);
        }

        [HttpPut("payment")]
        public async Task<IActionResult> UpdatePaymentSettings([FromBody] PaymentSettings updatedSettings)
        {
            var settings = await _context.PaymentSettings.FindAsync(1);
            if (settings == null) return NotFound("لم يتم العثور على إعدادات الدفع.");

            settings.AdminFullName = updatedSettings.AdminFullName;
            settings.PhoneNumber = updatedSettings.PhoneNumber;
            settings.Address = updatedSettings.Address;

            await _context.SaveChangesAsync();
            return Ok(settings);
        }
    }
}