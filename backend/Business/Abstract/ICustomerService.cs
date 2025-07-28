using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ICustomerService
    {
        IDataResult<CustomerDto> Add(CustomerAddDto customerAddDto);
        IDataResult<List<CustomerDto>> GetAll();
        IDataResult<CustomerDto> GetById(int customerId);
        IDataResult<CustomerDto> Update(CustomerUpdateDto customerUpdateDto);
        IResult Delete(int customerId);
    }
}
