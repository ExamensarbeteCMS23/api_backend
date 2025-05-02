using api_backend.Interfaces;
using api_backend.Models;
using api_backend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        public async Task<EmployeeEntity?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);

        }

        public Task<object> RegisterEmployeeAsync(RegisterCleanerDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<EmployeeEntity>> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
        {
            // Hämta ut den anställda från databasen
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null)
                return ServiceResult<EmployeeEntity>.Fail($"Anställd med anställningsnummer {id} ");

            // Hämta Användaren om den finns i Identity
            var use = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == employee.Id);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == employee.Id);
            if (user == null)
                return ServiceResult<EmployeeEntity>.Fail("Anställd hittades inte i Identity");

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return ServiceResult<EmployeeEntity>.Fail("Den nya eposten används redan");
                }

                var emailResult = await _userManager.SetEmailAsync(user, dto.Email);
                var usernameResult = await _userManager.SetUserNameAsync(user, dto.Email);

                if (!usernameResult.Succeeded || !emailResult.Succeeded)
                {
                    return ServiceResult<EmployeeEntity>.Fail("Det gick inte att uppdatera e-postadressen.");
                }
                employee.EmployeeEmail = dto.Email;
            }
            // Uppdatera andra fält om de skickas
            if (!string.IsNullOrEmpty(dto.FirstName))
                employee.EmployeeFirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName))
                employee.EmployeeLastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.Phone))
                employee.EmployeePhone = dto.Phone;
            if (dto.RoleId.HasValue)
                employee.RoleId = dto.RoleId.Value;

            await _repository.SaveAsync();
            return ServiceResult<EmployeeEntity>.Ok(employee,"Anställd uppdaterad.");
        }
    }
}
