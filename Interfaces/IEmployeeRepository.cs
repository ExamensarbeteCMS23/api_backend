using api_backend.Models;

namespace api_backend.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<EmployeeEntity?> GetByIdAsync(int id);
        Task<IEnumerable<EmployeeEntity>> GetAllAsync();
        Task UpdateAsync(EmployeeEntity entity);
        Task SaveAsync();

    }
}
