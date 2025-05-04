using api_backend.Models;

namespace api_backend.Interfaces
{
    public interface ICustomerRepository
    {
        Task RegisterCustomer(CustomerEntity customer);
        Task <IEnumerable<CustomerEntity>>GetAllCustomers();
        Task <CustomerEntity>GetCustomerById(int id);
        Task UpdateCustomer(CustomerEntity customer);
        Task DeleteCustomer(int id);

    }
}
