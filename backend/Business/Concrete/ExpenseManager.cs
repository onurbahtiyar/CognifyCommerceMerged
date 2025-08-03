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
    public class ExpenseManager : IExpenseService
    {
        private readonly IExpenseDal _expenseDal;
        private readonly IExpenseCategoryDal _expenseCategoryDal;

        public ExpenseManager(IExpenseDal expenseDal, IExpenseCategoryDal expenseCategoryDal)
        {
            _expenseDal = expenseDal;
            _expenseCategoryDal = expenseCategoryDal;
        }

        public IDataResult<ExpenseDto> Add(ExpenseAddDto dto)
        {
            var expense = new Expense
            {
                ExpenseCategoryId = dto.ExpenseCategoryId,
                Description = dto.Description,
                Amount = dto.Amount,
                ExpenseDate = dto.ExpenseDate,
                Notes = dto.Notes,
                CreatedDate = DateTime.Now,
                IsDeleted = false
            };
            _expenseDal.Add(expense);

            var category = _expenseCategoryDal.Get(c => c.ExpenseCategoryId == dto.ExpenseCategoryId);

            var resultDto = new ExpenseDto
            {
                ExpenseId = expense.ExpenseId,
                ExpenseCategoryId = expense.ExpenseCategoryId,
                ExpenseCategoryName = category?.Name,
                Description = expense.Description,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Notes = expense.Notes,
                CreatedDate = expense.CreatedDate
            };
            return new SuccessDataResult<ExpenseDto>(resultDto, "Masraf başarıyla eklendi.");
        }

        public IDataResult<ExpenseDto> Update(ExpenseUpdateDto dto)
        {
            var existing = _expenseDal.Get(e => e.ExpenseId == dto.ExpenseId && !e.IsDeleted);
            if (existing == null)
                return new ErrorDataResult<ExpenseDto>(null, "Güncellenecek masraf bulunamadı.");

            existing.ExpenseCategoryId = dto.ExpenseCategoryId;
            existing.Description = dto.Description;
            existing.Amount = dto.Amount;
            existing.ExpenseDate = dto.ExpenseDate;
            existing.Notes = dto.Notes;

            _expenseDal.Update(existing);

            var category = _expenseCategoryDal.Get(c => c.ExpenseCategoryId == dto.ExpenseCategoryId);

            var updatedDto = new ExpenseDto
            {
                ExpenseId = existing.ExpenseId,
                ExpenseCategoryId = existing.ExpenseCategoryId,
                ExpenseCategoryName = category?.Name,
                Description = existing.Description,
                Amount = existing.Amount,
                ExpenseDate = existing.ExpenseDate,
                Notes = existing.Notes,
                CreatedDate = existing.CreatedDate
            };

            return new SuccessDataResult<ExpenseDto>(updatedDto, "Masraf başarıyla güncellendi.");
        }

        public IResult Delete(int expenseId)
        {
            var expense = _expenseDal.Get(e => e.ExpenseId == expenseId && !e.IsDeleted);
            if (expense == null)
                return new Result(false, "Silinecek masraf bulunamadı.");

            expense.IsDeleted = true;
            _expenseDal.Update(expense);
            return new SuccessResult("Masraf başarıyla silindi.");
        }

        public IDataResult<List<ExpenseDto>> GetAll()
        {
            var list = _expenseDal.GetList(
                e => !e.IsDeleted,
                q => q.ExpenseCategory)
                .Select(e => new ExpenseDto
                {
                    ExpenseId = e.ExpenseId,
                    ExpenseCategoryId = e.ExpenseCategoryId,
                    ExpenseCategoryName = e.ExpenseCategory.Name,
                    Description = e.Description,
                    Amount = e.Amount,
                    ExpenseDate = e.ExpenseDate,
                    Notes = e.Notes,
                    CreatedDate = e.CreatedDate
                })
                .OrderByDescending(e => e.ExpenseDate)
                .ToList();

            return new SuccessDataResult<List<ExpenseDto>>(list, "Tüm masraflar listelendi.");
        }

        public IDataResult<ExpenseDto> GetById(int expenseId)
        {
            var expense = _expenseDal.Get(e => e.ExpenseId == expenseId && !e.IsDeleted, q => q.ExpenseCategory);
            if (expense == null)
                return new ErrorDataResult<ExpenseDto>(null, "Masraf bulunamadı.");

            var dto = new ExpenseDto
            {
                ExpenseId = expense.ExpenseId,
                ExpenseCategoryId = expense.ExpenseCategoryId,
                ExpenseCategoryName = expense.ExpenseCategory.Name,
                Description = expense.Description,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Notes = expense.Notes,
                CreatedDate = expense.CreatedDate
            };

            return new SuccessDataResult<ExpenseDto>(dto, "Masraf detayı getirildi.");
        }

        public IDataResult<List<ExpenseCategoryDto>> GetAllCategories()
        {
            var list = _expenseCategoryDal.GetList()
                .Select(c => new ExpenseCategoryDto
                {
                    ExpenseCategoryId = c.ExpenseCategoryId,
                    Name = c.Name,
                    Description = c.Description
                }).ToList();

            return new SuccessDataResult<List<ExpenseCategoryDto>>(list, "Masraf kategorileri listelendi.");
        }
    }
}
