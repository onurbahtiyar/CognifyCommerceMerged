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
    public class MarketplaceIntegrationManager : IMarketplaceIntegrationService
    {
        private readonly IMarketplaceIntegrationDal _marketplaceDal;

        public MarketplaceIntegrationManager(IMarketplaceIntegrationDal marketplaceDal)
        {
            _marketplaceDal = marketplaceDal;
        }

        public IDataResult<MarketplaceIntegrationDto> Add(MarketplaceIntegrationAddDto dto)
        {
            var existing = _marketplaceDal.Get(m => m.MarketplaceName.ToLower() == dto.MarketplaceName.ToLower());
            if (existing != null)
            {
                return new ErrorDataResult<MarketplaceIntegrationDto>(null, "Bu pazar yeri zaten mevcut.");
            }

            var integration = new MarketplaceIntegration
            {
                MarketplaceName = dto.MarketplaceName,
                ApiKey = dto.ApiKey,
                ApiSecret = dto.ApiSecret,
                ApiUrl = dto.ApiUrl,
                Description = dto.Description,
                IsActive = dto.IsActive,
            };

            _marketplaceDal.Add(integration);

            return new SuccessDataResult<MarketplaceIntegrationDto>(null, "Pazar yeri entegrasyonu başarıyla eklendi.");
        }

        public IResult Delete(int id)
        {
            var integration = _marketplaceDal.Get(m => m.Id == id);
            if (integration == null)
            {
                return new Result(false, "Entegrasyon bulunamadı.");
            }
            _marketplaceDal.Delete(integration);
            return new SuccessResult("Entegrasyon başarıyla silindi.");
        }

        public IDataResult<List<MarketplaceIntegrationDto>> GetAll()
        {
            var list = _marketplaceDal.GetList().Select(m => new MarketplaceIntegrationDto
            {
                Id = m.Id,
                MarketplaceName = m.MarketplaceName,
                ApiKey = "********",
                ApiSecret = "********",
                ApiUrl = m.ApiUrl,
                Description = m.Description,
                IsActive = m.IsActive
            }).ToList();

            return new SuccessDataResult<List<MarketplaceIntegrationDto>>(list, "Entegrasyonlar listelendi.");
        }

        public IDataResult<MarketplaceIntegrationDto> GetById(int id)
        {
            var m = _marketplaceDal.Get(m => m.Id == id);
            if (m == null)
            {
                return new ErrorDataResult<MarketplaceIntegrationDto>("Entegrasyon bulunamadı.");
            }

            var dto = new MarketplaceIntegrationDto
            {
                Id = m.Id,
                MarketplaceName = m.MarketplaceName,
                ApiKey = m.ApiKey,
                ApiSecret = m.ApiSecret,
                ApiUrl = m.ApiUrl,
                Description = m.Description,
                IsActive = m.IsActive
            };

            return new SuccessDataResult<MarketplaceIntegrationDto>(dto, "Entegrasyon detayları getirildi.");
        }

        public IResult Update(MarketplaceIntegrationUpdateDto dto)
        {
            var existing = _marketplaceDal.Get(m => m.Id == dto.Id);
            if (existing == null)
            {
                return new Result(false, "Güncellenecek entegrasyon bulunamadı.");
            }

            existing.MarketplaceName = dto.MarketplaceName;
            existing.ApiKey = dto.ApiKey;
            existing.ApiSecret = dto.ApiSecret;
            existing.ApiUrl = dto.ApiUrl;
            existing.Description = dto.Description;
            existing.IsActive = dto.IsActive;

            _marketplaceDal.Update(existing);
            return new SuccessResult("Entegrasyon başarıyla güncellendi.");
        }
    }
}
