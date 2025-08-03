using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IExpenseService
    {
        IDataResult<List<ExpenseDto>> GetAll();
        IDataResult<ExpenseDto> GetById(int expenseId);
        IDataResult<ExpenseDto> Add(ExpenseAddDto dto);
        IDataResult<ExpenseDto> Update(ExpenseUpdateDto dto);
        IResult Delete(int expenseId);
        IDataResult<List<ExpenseCategoryDto>> GetAllCategories();
    }
}
