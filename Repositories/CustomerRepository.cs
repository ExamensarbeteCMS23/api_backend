using api_backend.Contexts;
using api_backend.Interfaces;
using api_backend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace api_backend.Repositories
{
    public class CustomerRepository(DataContext context) : ICustomerRepository
    {
        private readonly DataContext _context = context;

        public Task DeleteCustomer(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<CustomerEntity>> GetAllCustomers() =>       
            await _context.Customers.ToListAsync();

        public async Task<CustomerEntity> GetCustomerById(int id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);
            if (customer == null)
            {
                return null!;
            }
            return customer;
        }

        public async Task <CustomerEntity?> RegisterCustomerAsync(CustomerEntity customer, CustomerAddressEntity address)
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

        public Task UpdateCustomer(CustomerEntity customer)
        {
            throw new NotImplementedException();
        }
    }
}
