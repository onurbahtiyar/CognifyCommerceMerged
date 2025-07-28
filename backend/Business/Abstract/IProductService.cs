using Core.Utilities.Result.Abstract;
using Entities.Dtos;

namespace Business.Abstract
{
    public interface IProductService
    {
        IDataResult<ProductDto> Add(ProductAddDto productAddDto);
        IDataResult<List<ProductDto>> GetAll();
        IDataResult<ProductDto> GetById(int productId);
        IDataResult<ProductDto> Update(ProductUpdateDto updateDto);
        IResult Delete(int productId);
    }
}
