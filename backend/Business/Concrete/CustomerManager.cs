using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CustomerManager : ICustomerService
    {
        private readonly ICustomerDal _customerDal;

        public CustomerManager(ICustomerDal customerDal)
        {
            _customerDal = customerDal;
        }

        public IDataResult<CustomerDto> Add(CustomerAddDto dto)
        {
            var existingCustomer = _customerDal.Get(c => c.Email == dto.Email);
            if (existingCustomer != null)
            {
                return new ErrorDataResult<CustomerDto>(null, "Bu e-posta adresi zaten kayıtlı.");
            }

            var customer = new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _customerDal.Add(customer);

            var resultDto = MapToDto(customer);
            return new SuccessDataResult<CustomerDto>(resultDto, "Müşteri başarıyla eklendi.");
        }

        public IDataResult<List<CustomerDto>> GetAll()
        {
            var list = _customerDal.GetList(c => c.IsActive)
                .Select(c => MapToDto(c))
                .ToList();

            return new SuccessDataResult<List<CustomerDto>>(list, "Müşteriler listelendi.");
        }

        public IDataResult<CustomerDto> GetById(int customerId)
        {
            var customer = _customerDal.Get(c => c.CustomerId == customerId && c.IsActive);
            if (customer == null)
                return new ErrorDataResult<CustomerDto>(null, "Müşteri bulunamadı.");

            var dto = MapToDto(customer);
            return new SuccessDataResult<CustomerDto>(dto, "Müşteri getirildi.");
        }

        public IDataResult<CustomerDto> Update(CustomerUpdateDto dto)
        {
            var existing = _customerDal.Get(c => c.CustomerId == dto.CustomerId && c.IsActive);
            if (existing == null)
                return new ErrorDataResult<CustomerDto>(null, "Güncellenecek müşteri bulunamadı.");

            var emailCheck = _customerDal.Get(c => c.Email == dto.Email && c.CustomerId != dto.CustomerId);
            if (emailCheck != null)
            {
                return new ErrorDataResult<CustomerDto>(null, "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");
            }

            existing.FirstName = dto.FirstName;
            existing.LastName = dto.LastName;
            existing.Email = dto.Email;
            existing.PhoneNumber = dto.PhoneNumber;
            existing.Address = dto.Address;

            _customerDal.Update(existing);

            var updatedDto = MapToDto(existing);
            return new SuccessDataResult<CustomerDto>(updatedDto, "Müşteri bilgileri güncellendi.");
        }

        public IResult Delete(int customerId)
        {
            var customer = _customerDal.Get(c => c.CustomerId == customerId && c.IsActive);
            if (customer == null)
                return new Result(false, "Silinecek müşteri bulunamadı.");

            customer.IsActive = false;
            _customerDal.Update(customer);
            return new SuccessResult("Müşteri başarıyla silindi.");
        }

        private CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                CreatedDate = customer.CreatedDate
            };
        }
    }
}
