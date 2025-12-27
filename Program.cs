using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
// using Polly; // Removed due to missing package and network issues
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Interfaces;
using SmartSchoolAPI.Repositories;
using SmartSchoolAPI.Services;
using SmartSchoolAPI.Settings;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// var connectionString = "Host=..."; // Moved to appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// --- تكوين الاتصال بقاعدة البيانات (PostgreSQL) ---
builder.Services.AddDbContext<SmartSchoolDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention()
);

// تسجيل أنواع Enum بشكل عام لـ Npgsql
Npgsql.NpgsqlConnection.GlobalTypeMapper.MapEnum<SmartSchoolAPI.Enums.QuestionType>();
Npgsql.NpgsqlConnection.GlobalTypeMapper.MapEnum<SmartSchoolAPI.Enums.QuestionStatus>();
Npgsql.NpgsqlConnection.GlobalTypeMapper.MapEnum<SmartSchoolAPI.Enums.DifficultyLevel>();
Npgsql.NpgsqlConnection.GlobalTypeMapper.MapEnum<SmartSchoolAPI.Enums.RegistrationStatus>();
Npgsql.NpgsqlConnection.GlobalTypeMapper.MapEnum<SmartSchoolAPI.Enums.UserRole>();
Npgsql.NpgsqlConnection.GlobalTypeMapper.MapEnum<SmartSchoolAPI.Enums.AnnouncementScope>();
Npgsql.NpgsqlConnection.GlobalTypeMapper.MapEnum<SmartSchoolAPI.Enums.ClassroomStatus>();


// --- تسجيل المستودعات والخدمات (Dependency Injection) ---
builder.Services.AddScoped<IAcademicProgramRepository, AcademicProgramRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClassroomRepository, ClassroomRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<ILectureRepository, LectureRepository>();
builder.Services.AddScoped<IGraduationRepository, GraduationRepository>();
builder.Services.AddScoped<IArchiveRepository, ArchiveRepository>();
builder.Services.AddScoped<IArchiveService, ArchiveService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<ILectureQuizRepository, LectureQuizRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IWeeklyChallengeRepository, WeeklyChallengeRepository>();
builder.Services.AddScoped<IProgramRegistrationRepository, ProgramRegistrationRepository>();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// تسجيل خدمات الذكاء الاصطناعي
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAiService, AiService>();

// --- إعدادات رفع الملفات ---
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 209715200; // 200 MB
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 209715200; // 200 MB
});

// --- تكوين المصادقة باستخدام JWT Bearer ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        Console.WriteLine($"DEBUG: Jwt:Key from config is: '{jwtKey}'");
        
        if (string.IsNullOrEmpty(jwtKey))
        {
            Console.WriteLine("WARNING: Jwt:Key is empty! Using fallback hardcoded key.");
            jwtKey = "THIS-IS-MY-SUPER-SECRET-KEY-FOR-SMART-SCHOOL-PROJECT-AND-IT-IS-DEFINITELY-LONGER-THAN-64-CHARACTERS";
        }

        Console.WriteLine($"DEBUG: ConnectionString is: '{connectionString}'");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(jwtKey ?? "default_key_if_not_found")),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// --- إعدادات CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
 .AddJsonOptions(options =>
 {
     options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
 });

builder.Services.AddEndpointsApiExplorer();

// --- إعدادات Swagger ---
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\""
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// --- تكوين مسار الطلبات (HTTP Request Pipeline) ---
if (app.Environment.IsDevelopment() || app.Environment.IsProduction()) // تمكين Swagger في الإنتاج
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// =================================================================
//      تطبيق تحديثات قاعدة البيانات وبذر البيانات الأولية عند بدء التشغيل
// =================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Attempting to connect to the database and apply migrations/seeding...");
        
        var context = services.GetRequiredService<SmartSchoolDbContext>();

        // var retryPolicy = Policy ... (Removed simplified)
        // Direct execution without retry policy
        // استخدام MigrateAsync هو الطريقة الصحيحة للتعامل مع قواعد البيانات
        // هذا يضمن توافق النظام مع التحديثات المستقبلية (Migrations)
        await context.Database.MigrateAsync();
        await DataSeeder.SeedAdminUser(services);

        logger.LogInformation("Database migrated and seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "A critical error occurred while initializing the database.");
    }
}

app.Run();
