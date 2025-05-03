using api_backend.Interfaces;
using api_backend.Repositories;

namespace api_backend.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationService (this IServiceCollection services)
        {
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            return services;
        }
    }
}
