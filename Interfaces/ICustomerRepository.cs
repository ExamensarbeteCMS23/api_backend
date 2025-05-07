using api_backend.Models;

namespace api_backend.Interfaces
{
    public interface ICustomerRepository
    {
        Task <CustomerEntity?>RegisterCustomerAsync(CustomerEntity customer, CustomerAddressEntity address);
        Task <IEnumerable<CustomerEntity>>GetAllCustomers();
        Task <CustomerEntity>GetCustomerById(int id);
        Task <CustomerAddressEntity>GetCustomerAddressById(int id);
        Task <bool>GetCustomerByEmail(string email);
        Task UpdateCustomer(CustomerEntity customer);
        Task<bool> DeleteCustomerAsync(CustomerEntity customer);

        Task SaveCustomerAsync();

    }
}
