using api_backend.Contexts;
using api_backend.Dtos;
using api_backend.Interfaces;
using api_backend.Models;
using api_backend.Repositories;
using api_backend.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api_backend.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly EmployeeRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DataContext _context;

        public EmployeeService(EmployeeRepository repository, UserManager<ApplicationUser> userManager, DataContext context)
        {
            _repository = repository;
            _userManager = userManager;
            _context = context;
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var employee = await _repository.GetByIdAsync(id);
            if (employee == null)
            {
                return ServiceResult.Fail("Anställd hittades inte");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return ServiceResult.Fail("Kunde inte ta bort användaren", errors);
                }
            }
            await _repository.RemoveEmployeeAsync(employee);
            return ServiceResult.Ok("Anställd borttagen");
        }

        public async Task<IEnumerable<EmployeeEntity>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<EmployeeEntity?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);

        }

        public async Task<ServiceResult<EmployeeEntity>> RegisterEmployeeAsync(RegisterCleanerDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return ServiceResult<EmployeeEntity>.Fail("Det finns redan en användare med den e-mailen");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var employee = new EmployeeEntity
                {
                    EmployeeFirstName = dto.FirstName,
                    EmployeeLastName = dto.LastName,
                    EmployeeEmail = dto.Email,
                    EmployeePhone = dto.Phone,
                    RoleId = dto.RoleId,
                };

                await _repository.AddEmployeeAsync(employee);

                var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, EmployeeId = employee.Id };

                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult<EmployeeEntity>.Fail("Kunde inte skapa en användare");
                }
                ;

                var roleName = await _repository.GetRoleNameById(dto.RoleId);
                if (!string.IsNullOrEmpty(roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }

                await transaction.CommitAsync();
                return ServiceResult<EmployeeEntity>.Ok(employee, "Anställd registrerad");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var error = ex.InnerException != null
                    ? $"{ex.Message} (InnerExeption: {ex.InnerException.Message}) " : ex.Message;
                return ServiceResult<EmployeeEntity>.Fail("Fel vid registrering" + error);
            }
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
            return ServiceResult<EmployeeEntity>.Ok(employee, "Anställd uppdaterad.");
        }
    }
}
