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
    public class CategoryManager : ICategoryService
    {
        private readonly ICategoryDal _categoryDal;
        public CategoryManager(ICategoryDal categoryDal)
        {
            _categoryDal = categoryDal;
        }

        public IDataResult<CategoryDto> Add(CategoryAddDto dto)
        {
            var category = new Category { Name = dto.Name, Description = dto.Description };
            _categoryDal.Add(category);
            var resultDto = new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
            return new SuccessDataResult<CategoryDto>(resultDto, "Kategori eklendi.");
        }

        public IDataResult<List<CategoryDto>> GetAll()
        {
            var list = _categoryDal.GetList()
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToList();
            return new SuccessDataResult<List<CategoryDto>>(list, "Kategoriler listelendi.");
        }

        public IDataResult<CategoryDto> GetById(int categoryId)
        {
            var c = _categoryDal.Get(c => c.CategoryId == categoryId);
            if (c == null)
                return new ErrorDataResult<CategoryDto>(null, "Kategori bulunamadı.");
            var dto = new CategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description
            };
            return new SuccessDataResult<CategoryDto>(dto, "Kategori getirildi.");
        }

        public IDataResult<CategoryDto> Update(CategoryUpdateDto dto)
        {
            var existing = _categoryDal.Get(c => c.CategoryId == dto.CategoryId);
            if (existing == null)
                return new ErrorDataResult<CategoryDto>(null, "Kategori bulunamadı.");

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            _categoryDal.Update(existing);

            var updatedDto = new CategoryDto
            {
                CategoryId = existing.CategoryId,
                Name = existing.Name,
                Description = existing.Description
            };
            return new SuccessDataResult<CategoryDto>(updatedDto, "Kategori güncellendi.");
        }

        public IResult Delete(int categoryId)
        {
            var category = _categoryDal.Get(c => c.CategoryId == categoryId);
            if (category == null)
                return new Result(false, "Kategori bulunamadı.");
            _categoryDal.Delete(category);
            return new SuccessResult("Kategori silindi.");
        }
    }
}
