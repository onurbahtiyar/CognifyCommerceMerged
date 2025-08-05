using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ICategoryService
    {
        IDataResult<CategoryDto> Add(CategoryAddDto dto);
        IDataResult<List<CategoryDto>> GetAll();
        IDataResult<CategoryDto> GetById(int categoryId);
        IDataResult<CategoryDto> Update(CategoryUpdateDto dto);
        IResult Delete(int categoryId);
    }
}
