using api_backend.Dtos;
using api_backend.Models;
using api_backend.Results;

namespace api_backend.Interface
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerEntity>> GetAllCustomersAsync();
        Task<ServiceResult<CustomerEntity>> GetCustomer(int id);
        Task<ServiceResult<CustomerEntity>> UpdateCustomerAsync(CustomerEntity customer);
        Task<ServiceResult> RemoveCustomerAsync(int id);
        Task<CustomerDto?> RegisterCustomerAsync (CreateCustomerRequestDto dto);
    }
}
