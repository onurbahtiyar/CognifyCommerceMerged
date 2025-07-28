using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;

namespace Business.Concrete
{
    public class ProductManager : IProductService
    {
        private readonly IProductDal _productDal;

        public ProductManager(IProductDal productDal)
        {
            _productDal = productDal;
        }

        public IDataResult<ProductDto> Add(ProductAddDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _productDal.Add(product);

            var resultDto = new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                CreatedDate = product.CreatedDate
            };
            return new SuccessDataResult<ProductDto>(resultDto, "Ürün eklendi.");
        }

        public IDataResult<List<ProductDto>> GetAll()
        {
            var list = _productDal.GetList(
                p => p.IsActive,
                q => q.Category)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name,
                    CreatedDate = p.CreatedDate
                })
                .ToList();

            return new SuccessDataResult<List<ProductDto>>(list, "Ürünler listelendi.");
        }

        public IDataResult<ProductDto> GetById(int productId)
        {
            var p = _productDal.Get(p => p.ProductId == productId && p.IsActive);
            if (p == null)
                return new ErrorDataResult<ProductDto>(null, "Ürün bulunamadı.");

            var dto = new ProductDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CreatedDate = p.CreatedDate
            };
            return new SuccessDataResult<ProductDto>(dto, "Ürün getirildi.");
        }

        public IDataResult<ProductDto> Update(ProductUpdateDto dto)
        {
            var existing = _productDal.Get(p => p.ProductId == dto.ProductId && p.IsActive);
            if (existing == null)
                return new ErrorDataResult<ProductDto>(null, "Ürün bulunamadı.");

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.Price = dto.Price;
            existing.Stock = dto.Stock;
            existing.CategoryId = dto.CategoryId;

            _productDal.Update(existing);

            var updatedDto = new ProductDto
            {
                ProductId = existing.ProductId,
                Name = existing.Name,
                Description = existing.Description,
                Price = existing.Price,
                Stock = existing.Stock,
                CategoryId = existing.CategoryId,
                CreatedDate = existing.CreatedDate
            };
            return new SuccessDataResult<ProductDto>(updatedDto, "Ürün güncellendi.");
        }

        public IResult Delete(int productId)
        {
            var product = _productDal.Get(p => p.ProductId == productId && p.IsActive);
            if (product == null)
                return new Result(false, "Ürün bulunamadı.");

            product.IsActive = false;
            _productDal.Update(product);
            return new SuccessResult("Ürün silindi.");
        }
    }
}
