using api_backend.Dtos;
using api_backend.Models;
using api_backend.Results;

namespace api_backend.Interfaces
{
    public interface IEmployeeService
    {
        Task<ServiceResult<EmployeeEntity>> RegisterEmployeeAsync(RegisterCleanerDto dto);
        Task<ServiceResult<EmployeeEntity>> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto);
        Task<IEnumerable<EmployeeEntity>> GetAllAsync();
        Task<EmployeeEntity?> GetByIdAsync(int id);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
