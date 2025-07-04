using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Xpress_backend_V2.Data;
using Xpress_backend_V2.Interface;
using Xpress_backend_V2.Repositories;
using Xpress_backend_V2.Models.Configuration;
using Xpress_backend_V2.Repository;
using Xpress_backend_V2.Services;
using Xpress_backend_V2.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Load settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("ApplicationSettings"));

// Add controllers
builder.Services.AddControllers();

// === DATABASE CONNECTION (Cloud + Local fallback) ===
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(connectionString));

// ===  Dependency Injection for Services ===
builder.Services.AddScoped<ITravelRequestServices, TravelRequestRepository>();
builder.Services.AddScoped<ITicketOptionServices, TicketOptionRepository>();
builder.Services.AddScoped<IUserServices, UserRepository>();
builder.Services.AddScoped<IRMTServices, RMTRepository>();
builder.Services.AddScoped<ITravelModeServices, TravelModeRepository>();
builder.Services.AddScoped<IAirlineReportRepository, AirlineReportRepository>();
builder.Services.AddScoped<IRequestStatusServices, RequestStatusRepository>();
builder.Services.AddScoped<IUserNotificationServices, UserNotificationRepository>();
builder.Services.AddScoped<IAuditLogServices, AuditLogRepository>();
builder.Services.AddScoped<IProjectRoleService, ProjectRoleService>();
builder.Services.AddScoped<ICalendarTravelRequestRepository, CalendarTravelRequestRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITravelRequestStatsRepository, TravelRequestStatsRepository>();
builder.Services.AddScoped<IDocumentService, DocumentRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IAuditLogHandlerService, AuditLogHandlerService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IProcessingTimeRepository, ProcessingTimeRepository>();
builder.Services.AddScoped<IDocumentStatusRepository, DocumentStatusRepository>();
builder.Services.AddScoped<ITravelAgencyStatRepository, TravelAgencyStatRepository>();
builder.Services.AddScoped<ITravelRequestRepo, TravelRequestRepo>();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpClient();

// === CORS POLICY ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// === JWT Authentication ===
var jwtKey = builder.Configuration["JWT:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new Exception("JWT Key not configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// === Swagger ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Xpress API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT token with Bearer prefix",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// === Build and Run ===
var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// DO NOT use HTTPS redirection on Render (already HTTPS)
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
