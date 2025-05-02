using api_backend.Interfaces;
using api_backend.Models;
using api_backend.Repositories;
using Microsoft.AspNetCore.Identity;

namespace api_backend.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly EmployeeRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeService(EmployeeRepository repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        public async Task<IEnumerable<EmployeeEntity>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public Task<EmployeeEntity?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<object> RegisterEmployeeAsync(RegisterCleanerDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
