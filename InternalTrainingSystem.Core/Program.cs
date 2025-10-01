using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Extensions;
using InternalTrainingSystem.Core.DB;
using Microsoft.EntityFrameworkCore;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Identity;
using InternalTrainingSystem.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InternalTrainingSystem.Core.Middleware;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using InternalTrainingSystem.Core.Services.Implement;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
builder.Configuration.LoadEnvironmentVariables();

// Override configuration with environment variables
builder.Services.OverrideWithEnvironmentVariables(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// Configure Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Register Services
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

// Configure JWT Authentication
// Read from configuration instead of environment variables directly
var jwtSecretKey = builder.Configuration["JWT_SECRET_KEY"];
var jwtIssuer = builder.Configuration["JWT_ISSUER"];
var jwtAudience = builder.Configuration["JWT_AUDIENCE"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.Configure<ExternalApiKeys>(builder.Configuration.GetSection(ExternalApiKeys.SectionName));
builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection(FileUploadSettings.SectionName));
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection(ApplicationSettings.SectionName));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Internal Training System API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Internal Training System API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Add token blacklist middleware before authentication
app.UseMiddleware<TokenBlacklistMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
