using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IOrderService
    {
        IDataResult<List<OrderDto>> GetAll();
        IDataResult<OrderDto> GetById(int orderId);
        IDataResult<OrderDto> Add(OrderAddDto dto);
        IResult UpdateShippingInfo(OrderUpdateShippingDto dto);
        IResult UpdateStatus(OrderUpdateStatusDto dto);
        IResult CancelOrder(int orderId);
    }
}
