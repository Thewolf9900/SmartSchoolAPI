using SmartSchoolAPI.Entities;
using SmartSchoolAPI.Enums;

namespace SmartSchoolAPI.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SmartSchoolDbContext>();

                // التأكد من أن قاعدة البيانات موجودة ومحدثة
                   //  await context.Database.EnsureCreatedAsync();

                // بذر حساب المدير

                if (!context.Users.Any(u => u.Role == UserRole.Administrator))
                {
                    var adminUser = new User
                    {
                        FirstName = "Admin",
                        LastName = "User",
                        Email = "user1@example.com",
                        Role = UserRole.Administrator,
                        // نقوم بتجزئة كلمة مرور افتراضية
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456789"),
                        NationalId = "11111111111",
                        CreatedAt = DateTime.UtcNow
                    };

                    // إضافة المستخدم إلى الـ context وحفظ التغييرات
                    await context.Users.AddAsync(adminUser);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
