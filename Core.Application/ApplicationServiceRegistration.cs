using Core.Application.ImplementServices;
using Core.Application.InterfaceServices;
using Core.Application.Payloads.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
