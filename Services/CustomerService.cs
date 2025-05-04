using api_backend.Contexts;
using api_backend.Dtos;
using api_backend.Interface;
using api_backend.Interfaces;
using api_backend.Models;
using api_backend.Results;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;

namespace api_backend.Services
{
    public class CustomerService(ICustomerRepository customerRepository, IMapper mapper, DataContext context) : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository = customerRepository;
        private readonly IMapper _mapper = mapper;
        private readonly DataContext _context = context;

       

        public async Task<IEnumerable<CustomerEntity>> GetAllCustomersAsync()
        {
            var result = await _customerRepository.GetAllCustomers();
            if (result != null) { 
                return result;
            }
            return null!;
        }

        public Task<ServiceResult<CustomerEntity>> GetCustomer(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomerDto?> RegisterCustomerAsync(CreateCustomerRequestDto dto)
        {
            var address = _mapper.Map<CustomerAddressEntity>(dto);
            var customer = _mapper.Map<CustomerEntity>(dto);

            var result = await _customerRepository.RegisterCustomerAsync(customer, address);

            return result != null ? _mapper.Map<CustomerDto>(result) : null;
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
