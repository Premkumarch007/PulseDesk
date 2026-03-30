using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseDesk.Application.Interfaces.Repositories;
using PulseDesk.Application.Interfaces.Services;
using PulseDesk.Infrastructure.Persistence;
using PulseDesk.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PulseDeskDbContext>(options =>

                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "PulseDesk")));

            // Repos
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();

            // ── Services ──────────────────────────────────────────
            services.AddScoped<ITokenService, TokenService>();
            services.AddSingleton<IPasswordService, PasswordService>();
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

            return services;
        }
    }
}
