using api_backend.Contexts;
using api_backend.Interfaces;
using api_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace api_backend.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DataContext _context;
        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(DataContext context, ILogger<EmployeeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }   

        public async Task AddEmployeeAsync(EmployeeEntity employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<EmployeeEntity>> GetAllAsync() =>
            await _context.Employees.ToListAsync();

        public async Task<EmployeeEntity?> GetByIdAsync(int id)
        {
            if (_context == null) throw new Exception("DbContext is null in repository!");
            if (_context.Employees == null) throw new Exception("Employees DbSet is null!");

            return await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task<string> GetRoleNameById(int id) =>
            await _context.Roles
            .Where(r => r.Id == id)
            .Select(r => r.Role)
            .FirstOrDefaultAsync();

        public async Task RemoveEmployeeAsync(EmployeeEntity employee)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync() =>
            await _context.SaveChangesAsync();

        public async Task UpdateAsync(EmployeeEntity employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }
    }
}
