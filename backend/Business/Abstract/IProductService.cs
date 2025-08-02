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
        Task<IDataResult<PriceAnalysisResult>> GetPriceAnalysisAsync(string productName);
        Task<IDataResult<string>> EnhanceImageTempAsync(byte[] uploadedImage, string instruction, string targetBackground = "white");
        Task<IDataResult<string>> AttachTempImageToProductAsync(int productId, string tempImageUrl);

    }
}
