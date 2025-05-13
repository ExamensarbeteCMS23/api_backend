using api_backend.Contexts;
using api_backend.Interfaces;
using api_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace api_backend.Repositories
{
    public class CustomerRepository(DataContext context) : ICustomerRepository
    {
        private readonly DataContext _context = context;

        public async Task<bool> DeleteCustomerAsync(CustomerEntity customer)
        {
            _context.Customers.Remove(customer);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<IEnumerable<CustomerEntity>> GetAllCustomers() =>
            await _context.Customers.ToListAsync();

        public async Task<CustomerEntity> GetCustomerById(int id)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CustomerAddressEntity> GetCustomerAddressById(int id)
        {
            return await _context.CustomerAddresses.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CustomerEntity?> RegisterCustomerAsync(CustomerEntity customer, CustomerAddressEntity address)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.CustomerAddresses.Add(address);
                await _context.SaveChangesAsync();

                customer.AddressId = address.Id;
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return customer;
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
        }

        public async Task UpdateCustomer(CustomerEntity customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CustomerExistByEmail(string email)
        {
            return await _context.Customers.AnyAsync(c => c.CustomerEmail == email);
        }

        public async Task SaveCustomerAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
