using api_backend.Contexts;
using api_backend.Dtos;
using api_backend.Interface;
using api_backend.Interfaces;
using api_backend.Models;
using api_backend.Results;
using AutoMapper;

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
            return result ?? Enumerable.Empty<CustomerEntity>();
        }

        public async Task<ServiceResult<CustomerDto?>> GetCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetCustomerById(id);
            if (customer == null)
            {
                return ServiceResult<CustomerDto>.Fail("Kund saknas");
            }
            var address = await _customerRepository.GetCustomerAddressById(customer.Id);
            if (address == null)
            {
                return ServiceResult<CustomerDto>.Fail("Address saknas");
            }
            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                CustomerFirstName = customer.CustomerFirstName,
                CustomerLastName = customer.CustomerLastName,
                CustomerEmail = customer.CustomerEmail,
                CustomerStreetName = address.CustomerStreetName,
                CustomerCity = address.CustomerCity,
                CustomerPostalCode = address.CustomerPostalCode,
            };

            return ServiceResult<CustomerDto>.Ok(customerDto, "Anställd hittad");

        }

        public async Task<ServiceResult<CustomerDto?>> RegisterCustomerAsync(CreateCustomerRequestDto dto)
        {
            var customerInDb = await _customerRepository.CustomerExistByEmail(dto.CustomerEmail);

            if (!customerInDb)
            {

                var address = _mapper.Map<CustomerAddressEntity>(dto);
                var customer = _mapper.Map<CustomerEntity>(dto);

                var result = await _customerRepository.RegisterCustomerAsync(customer, address);

                var customerDto = new CustomerDto
                {
                    Id = result.Id,
                    CustomerFirstName = result.CustomerFirstName,
                    CustomerLastName = result.CustomerLastName,
                    CustomerEmail = result.CustomerEmail,
                    CustomerStreetName = address.CustomerStreetName,
                    CustomerCity = address.CustomerCity,
                    CustomerPostalCode = address.CustomerPostalCode,
                };

                return ServiceResult<CustomerDto>.Ok(customerDto, "Kunden är skapad");
            }
            return ServiceResult<CustomerDto>.Fail("Kunden finns redan i databasen");

        }

        public async Task<ServiceResult> RemoveCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetCustomerById(id);

            if (customer == null)
            {
                return ServiceResult.Fail("Kunden kunde inte hittas");
            }

            var result = await _customerRepository.DeleteCustomerAsync(customer);
            if (!result)
                return ServiceResult.Fail("Kunden finns inte i databasen");
            return ServiceResult.Ok("Kunden är raderad från databasen");
        }

        public async Task<ServiceResult<UpdateCustomerDto?>> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
        {
            var customer = await _customerRepository.GetCustomerById(id);
            if (customer == null)
            {
                return ServiceResult<UpdateCustomerDto>.Fail("Kunden finns inte i systemet");
            }

            if (!string.IsNullOrEmpty(dto.CustomerFirstName))
                customer.CustomerFirstName = dto.CustomerFirstName;

            if (!string.IsNullOrEmpty(dto.CustomerLastName))
                customer.CustomerLastName = dto.CustomerLastName;

            if (!string.IsNullOrEmpty(dto.CustomerEmail))
            {
                if (await _customerRepository.CustomerExistByEmail(dto.CustomerEmail))
                {
                    return ServiceResult<UpdateCustomerDto>.Fail("Användare med den eposten finns redan");
                }
                    customer.CustomerEmail = dto.CustomerEmail;
            }

            var adress = await _customerRepository.GetCustomerAddressById(customer.AddressId);
            if (customer.AddressId != null)
            {
                if (adress != null)
                {
                    if (!string.IsNullOrEmpty(dto.CustomerAddress))
                        adress.CustomerStreetName = dto.CustomerAddress;
                    if (!string.IsNullOrEmpty(dto.CustomerCity))
                        adress.CustomerCity = dto.CustomerCity;
                    if (!string.IsNullOrEmpty(dto.CustomerPostalCode))
                        adress.CustomerPostalCode = dto.CustomerPostalCode;
                }
            }

            await _customerRepository.SaveCustomerAsync();

            var resultDto = new UpdateCustomerDto
            {
                CustomerFirstName = customer.CustomerFirstName,
                CustomerLastName = customer.CustomerLastName,
                CustomerEmail = customer.CustomerEmail,
                CustomerCity = adress.CustomerCity,
                CustomerPostalCode = adress.CustomerPostalCode,
                CustomerAddress = adress.CustomerStreetName
            };

            return ServiceResult<UpdateCustomerDto>.Ok(resultDto);

        }
    }
}
