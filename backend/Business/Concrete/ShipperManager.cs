using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Dtos;

namespace Business.Concrete
{
    public class ShipperManager : IShipperService
    {
        private readonly IShipperDal _shipperDal;

        public ShipperManager(IShipperDal shipperDal)
        {
            _shipperDal = shipperDal;
        }

        public IDataResult<List<ShipperDto>> GetAllActive()
        {
            var shippers = _shipperDal.GetList(s => s.IsActive == true);

            var shipperDtos = shippers.Select(s => new ShipperDto
            {
                ShipperId = s.ShipperId,
                CompanyName = s.CompanyName
            }).ToList();

            return new SuccessDataResult<List<ShipperDto>>(shipperDtos, "Aktif kargo firmaları başarıyla listelendi.");
        }
    }
}