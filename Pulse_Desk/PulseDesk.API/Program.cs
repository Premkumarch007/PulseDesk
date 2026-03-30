using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PulseDesk.API.Hubs;
using PulseDesk.API.Middleware;
using PulseDesk.Application;
using PulseDesk.Application.Interfaces.Hubs;
using PulseDesk.Infrastructure;
using PulseDesk.Infrastructure.Persistence;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.
    ReadFrom.Configuration(context.Configuration).
    Enrich.FromLogContext().
    Enrich.WithMachineName().
    Enrich.WithThreadId().
    WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
            "{Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
            path: "logs/pulsedesk-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30);
});

// jwt
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero // No grace period on expiry
    };

    // SignalR sends JWT as query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            // Only applying to SignalR hub path
            if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs")) context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

// ── Policy-Based Authorization ────────────────────────────────
// Each policy maps directly to a row in our Policies table
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanAssignTickets", policy =>
        policy.RequireClaim("policy", "CanAssignTickets"));

    options.AddPolicy("CanCloseTickets", policy =>
        policy.RequireClaim("policy", "CanCloseTickets"));

    options.AddPolicy("CanViewReports", policy =>
        policy.RequireClaim("policy", "CanViewReports"));

    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireClaim("policy", "CanManageUsers"));

    options.AddPolicy("CanManageAssets", policy =>
        policy.RequireClaim("policy", "CanManageAssets"));

    options.AddPolicy("CanManageRoles", policy =>
        policy.RequireClaim("policy", "CanManageRoles"));
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddScoped<IDashboardHubContext, DashboardHubContext>();
builder.Services.AddControllers();

// ── Swagger ───────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PulseDesk API",
        Version = "v1"
    });

    // Add JWT input to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ──────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("PulseDeskCors", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://gourav-d.github.io")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PulseDeskCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<DashboardHub>("/hubs/dashboard");
app.Run();
