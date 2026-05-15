using AutiCare.API.Hubs;
using AutiCare.API.Middleware;
using AutiCare.Application.Interfaces;
using AutiCare.Application.Mappings;
using AutiCare.Application.Services;
using AutiCare.Domain.Entities;
using AutiCare.Infrastructure.Persistence;
using AutiCare.Infrastructure.Persistence.Repositories;
using AutiCare.Infrastructure.Security;
using AutiCare.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Environment Setup ───────────────────
builder.Configuration.AddEnvironmentVariables();

// ── Serilog ─────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/auticare-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ── Database ────────────────────────────
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
               ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (databaseUrl!.StartsWith("postgresql://") || databaseUrl.StartsWith("postgres://"))
{
    Console.WriteLine(" Using Railway DB");
    databaseUrl = ParseDatabaseUrl(databaseUrl);
}
else
{
    Console.WriteLine(" Using Connection String");
}

string connectionString = databaseUrl!;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("AutiCare.Infrastructure")));

// ── Identity ────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Password
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

    // Username / Email
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── JWT ─────────────────────────────────
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"] ?? "TemporaryFallbackSecretForDevelopmentOnly123!")
        ),
        ClockSkew = TimeSpan.FromMinutes(5)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});
 

// ── Repositories ────────────────────────
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IChildRepository, ChildRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<ITherapistRepository, TherapistRepository>();
builder.Services.AddScoped<ITreatmentPlanRepository, TreatmentPlanRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IChatRepository, AutiCare.Infrastructure.Persistence.Repositories.ChatRepository>();
builder.Services.AddScoped<IMessageRepository, AutiCare.Infrastructure.Persistence.Repositories.MessageRepository>();
builder.Services.AddScoped<IDashboardRepository, AutiCare.Infrastructure.Persistence.Repositories.DashboardRepository>();

// ── Services ───────────────────────────
builder.Services.AddScoped<ISignalRService, AutiCare.API.Services.SignalRService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IChildService, ChildService>();
builder.Services.AddScoped<ITreatmentService, TreatmentService>();

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<INotesService, NotesService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddHttpClient<IAiClientProvider, HuggingFaceAiClientProvider>(client =>
{
    var baseUrl = builder.Configuration["AI_BASE_URL"] ?? "https://moaz2545-gradpro.hf.space";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(
        int.TryParse(builder.Configuration["AI_TIMEOUT_SECONDS"], out var t) ? t : 30);
});
builder.Services.AddScoped<IScreeningService, ScreeningService>();

// ── AutoMapper ─────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ── Validation ─────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<
    AutiCare.Application.Validators.RegisterRequestValidator>();
 // ── SignalR ────────────────────────────
builder.Services.AddSignalR();

// ── CORS ───────────────────────────────
// Origins are read from config so they can be overridden via Railway env vars.
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>()
    ?? new[]
    {
        "http://localhost:3000",
        "http://localhost:5173",
        "https://auticare-frontend-main.vercel.app",
        "https://auticare-production-828c.up.railway.app"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── Controllers + Swagger ─────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent cyclic serialization if any EF navigation property leaks into a response.
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AutiCare API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat="JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
           Array.Empty<string>()
        }
    });
});

// ── Railway PORT binding ──────────────
// MUST be set BEFORE builder.Build() or it has no effect.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    Console.WriteLine($" Binding to Railway PORT: {port}");
    builder.WebHost.UseUrls($"http://*:{port}");
}

var app = builder.Build();

// ── Migrate + Roles ───────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    string[] roles = { "Parent", "Doctor", "Therapist" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    // ── Seed Specialists ──────────────────
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var doctorsToSeed = new[]
    {
        new { Name = "Omar Ahmed", Email = "omar.ahmed@auticare.com", Role = "Doctor" },
        new { Name = "Ahmed Ali", Email = "ahmed.ali@auticare.com", Role = "Doctor" },
        new { Name = "Youssef Ahmed", Email = "youssef.ahmed@auticare.com", Role = "Doctor" },
        new { Name = "Mahmoud Tarek", Email = "mahmoud.tarek@auticare.com", Role = "Doctor" },
        new { Name = "Younes Mahmoud", Email = "younes.mahmoud@auticare.com", Role = "Doctor" }
    };

    var therapistsToSeed = new[]
    {
        new { Name = "Sara Mohamed", Email = "sara.mohamed@auticare.com", Role = "Therapist", Spec = "Speech Therapist" },
        new { Name = "Shahd Mohamed", Email = "shahd.mohamed@auticare.com", Role = "Therapist", Spec = "ABA Therapist" },
        new { Name = "Malak Tamer", Email = "malak.tamer@auticare.com", Role = "Therapist", Spec = "Occupational Therapist" },
        new { Name = "Rawan Essam", Email = "rawan.essam@auticare.com", Role = "Therapist", Spec = "Behavioral Therapist" },
        new { Name = "Salma Hussein", Email = "salma.hussein@auticare.com", Role = "Therapist", Spec = "Sensory Integration Therapist" }
    };

    var password = "AutiCare123!";

    foreach (var doc in doctorsToSeed)
    {
        var existingUser = await userManager.FindByEmailAsync(doc.Email);
        if (existingUser == null)
        {
            var user = new ApplicationUser { UserName = doc.Email, Email = doc.Email, FullName = doc.Name, Role = doc.Role };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, doc.Role);
                db.Specialists.Add(new Specialist
                {
                    UserId = user.Id,
                    Name = doc.Name,
                    Specialization = "Pediatric Neurologist",
                    Bio = "Experienced Pediatric Neurologist",
                    YearsExperience = 10,
                    Email = doc.Email
                });
            }
        }
        else if (string.IsNullOrEmpty(existingUser.Role))
        {
            existingUser.Role = doc.Role;
            existingUser.FullName = doc.Name;
            await userManager.UpdateAsync(existingUser);
        }
    }

    foreach (var thr in therapistsToSeed)
    {
        var existingUser = await userManager.FindByEmailAsync(thr.Email);
        if (existingUser == null)
        {
            var user = new ApplicationUser { UserName = thr.Email, Email = thr.Email, FullName = thr.Name, Role = thr.Role };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, thr.Role);
                db.Specialists.Add(new Specialist
                {
                    UserId = user.Id,
                    Name = thr.Name,
                    Specialization = thr.Spec,
                    Bio = $"Experienced {thr.Spec}",
                    YearsExperience = 5,
                    Email = thr.Email
                });
            }
        }
        else if (string.IsNullOrEmpty(existingUser.Role))
        {
            existingUser.Role = thr.Role;
            existingUser.FullName = thr.Name;
            await userManager.UpdateAsync(existingUser);
        }
    }

    await db.SaveChangesAsync();
}

// ── Pipeline ──────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

// NOTE: HttpsRedirection is intentionally omitted.
// Railway terminates TLS at the proxy layer; redirecting inside the container
// would cause infinite redirect loops on Railway's HTTP→container path.
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutiCare API v1");
    c.RoutePrefix = string.Empty;
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();

// ── Helpers ─────────────────────────────
static string ParseDatabaseUrl(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');
    return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
}