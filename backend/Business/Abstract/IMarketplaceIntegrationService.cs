using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IMarketplaceIntegrationService
    {
        IDataResult<List<MarketplaceIntegrationDto>> GetAll();
        IDataResult<MarketplaceIntegrationDto> GetById(int id);
        IDataResult<MarketplaceIntegrationDto> Add(MarketplaceIntegrationAddDto dto);
        IResult Update(MarketplaceIntegrationUpdateDto dto);
        IResult Delete(int id);
    }
}
