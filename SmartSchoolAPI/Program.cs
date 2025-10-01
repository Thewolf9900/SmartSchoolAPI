using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using SmartSchoolAPI.Data;
using SmartSchoolAPI.Interfaces;
using SmartSchoolAPI.Repositories;
using SmartSchoolAPI.Services;
using SmartSchoolAPI.Settings;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration["Jwt:Key"])),
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
if (app.Environment.IsDevelopment())
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
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<SmartSchoolDbContext>();

        // الخطوة 1: تطبيق الـ Migrations (الطريقة الاحترافية)
        context.Database.Migrate();

        // الخطوة 2: بذر بيانات المستخدم الأدمن )
        await DataSeeder.SeedAdminUser(services);

        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Database migrated and seeded successfully.");
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}
 
app.Run();