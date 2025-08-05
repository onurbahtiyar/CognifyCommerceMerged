using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfExpenseCategoryDal : EfEntityRepositoryBase<ExpenseCategory, ContextDb>, IExpenseCategoryDal
    {
        public EfExpenseCategoryDal(ContextDb _context) : base(_context)
        {
        }
    }
}
