using api_backend.Models;

namespace api_backend.Interfaces
{
    public interface IEmployeeService
    {
        Task<object> RegisterEmployeeAsync(RegisterCleanerDto dto);
        Task<ServiceResult<EmployeeEntity>> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto);
        Task<IEnumerable<EmployeeEntity>> GetAllAsync();
        Task<EmployeeEntity?> GetByIdAsync(int id);
    }
}
