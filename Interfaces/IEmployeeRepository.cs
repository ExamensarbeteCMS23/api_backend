using api_backend.Models;

namespace api_backend.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<EmployeeEntity?> GetByIdAsync(int id);
        Task<IEnumerable<EmployeeEntity>> GetAllAsync();
        Task UpdateAsync(EmployeeEntity entity);
        Task SaveAsync();
        Task AddEmployeeAsync(EmployeeEntity entity);
        Task RemoveEmployeeAsync(EmployeeEntity employee);
        Task<string> GetRoleNameById(int id);
    }
}
