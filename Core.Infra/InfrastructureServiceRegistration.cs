using Core.Domain.Interfaces;
using Core.Domain.Models;
using Core.Infra.DataContexts;
using Core.Infra.Repo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infra
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Đăng ký Repository
            services.AddScoped<IDbContext, AppDbContext>();

            services.AddScoped<IBaseRepo<User>, BaseRepo<User>>();
            services.AddScoped<IBaseRepo<ConfirmEmail>, BaseRepo<ConfirmEmail>>();
            services.AddScoped<IBaseRepo<RefreshToken>, BaseRepo<RefreshToken>>();

            services.AddScoped<IUserRepo, UserRepo>();

            return services;
        }
    }
}
