using api_backend.Interface;
using api_backend.Interfaces;
using api_backend.Models;
using api_backend.Results;

namespace api_backend.Services
{
    public class CustomerService(ICustomerRepository customerRepository) : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository = customerRepository;

        public Task<IEnumerable<CustomerEntity>> GetAllCustomersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<CustomerEntity>> GetCustomer(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<CustomerEntity>> RegisterCustomer(CustomerEntity customer)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> RemoveCustomerAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<CustomerEntity>> UpdateCustomerAsync(CustomerEntity customer)
        {
            throw new NotImplementedException();
        }
    }
}
