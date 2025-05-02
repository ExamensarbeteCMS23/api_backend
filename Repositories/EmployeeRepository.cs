using api_backend.Contexts;
using api_backend.Interfaces;
using api_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace api_backend.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DataContext _context;

        public EmployeeRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeEntity>> GetAllAsync() =>
            await _context.Employees.ToListAsync();

        public async Task<EmployeeEntity?> GetByIdAsync(int id) =>
             await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);

        public async Task SaveAsync() =>
            await _context.SaveChangesAsync();

        public async Task UpdateAsync(EmployeeEntity employee)
        {
            _context.Employees.Update(employee);
        }
    }
}
