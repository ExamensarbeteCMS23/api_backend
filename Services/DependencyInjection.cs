using api_backend.Interface;
using api_backend.Interfaces;
using api_backend.Mappings;
using api_backend.Repositories;

namespace api_backend.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationService (this IServiceCollection services)
        {
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingService, BookingService>();

            return services;
        }
    }
}
