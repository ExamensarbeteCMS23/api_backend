using api_backend.Dtos;
using api_backend.Models;
using api_backend.Results;

namespace api_backend.Interface
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerEntity>> GetAllCustomersAsync();
        Task<ServiceResult<CustomerDto>> GetCustomerAsync(int id);
       
        Task<ServiceResult<UpdateCustomerDto?>> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
        Task<ServiceResult> RemoveCustomerAsync(int id);
        Task<ServiceResult<CustomerDto?>> RegisterCustomerAsync (CreateCustomerRequestDto dto);
    }
}
