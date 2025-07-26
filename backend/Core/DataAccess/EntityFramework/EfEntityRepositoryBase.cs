using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Core.DataAccess.EntityFramework
{
    public class EfEntityRepositoryBase<TEntity, TContext> : IEntityRepository<TEntity>
        where TEntity : class, IEntity, new()
        where TContext : DbContext
    {
        private readonly TContext _context;

        public EfEntityRepositoryBase(TContext context)
        {
            _context = context;
        }

        public void Add(TEntity entity)
        {
            var entry = _context.Entry(entity);
            entry.State = EntityState.Added;
            _context.SaveChanges();
        }

        public async Task AddAsync(TEntity entity)
        {
            var entry = _context.Entry(entity);
            entry.State = EntityState.Added;
            await _context.SaveChangesAsync();
        }

        public void Update(TEntity entity)
        {
            var entry = _context.Entry(entity);
            entry.State = EntityState.Modified;
            _context.SaveChanges();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            var entry = _context.Entry(entity);
            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public void Delete(TEntity entity)
        {
            var entry = _context.Entry(entity);
            entry.State = EntityState.Deleted;
            _context.SaveChanges();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            var entry = _context.Entry(entity);
            entry.State = EntityState.Deleted;
            await _context.SaveChangesAsync();
        }

        public TEntity Get(
            Expression<Func<TEntity, bool>> filter = null,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (includeProperties != null)
            {
                foreach (var include in includeProperties)
                    query = query.Include(include);
            }
            return filter == null
                ? query.FirstOrDefault()
                : query.FirstOrDefault(filter);
        }

        public TEntity Get(
            Expression<Func<TEntity, bool>> filter,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> eager,
            params Expression<Func<TEntity, object>>[] includeProperties
        )
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includeProperties != null)
            {
                foreach (var include in includeProperties)
                    query = query.Include(include);
            }

            if (eager != null)
                query = eager(query);

            return filter == null
                 ? query.FirstOrDefault()
                 : query.FirstOrDefault(filter);
        }

        public async Task<TEntity> GetAsync(
            Expression<Func<TEntity, bool>> filter = null)
        {
            return await _context.Set<TEntity>()
                .SingleOrDefaultAsync(filter);
        }

        public List<TEntity> GetList(
            Expression<Func<TEntity, bool>> filter = null,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (includeProperties != null)
            {
                foreach (var include in includeProperties)
                    query = query.Include(include);
            }
            return filter == null
                ? query.ToList()
                : query.Where(filter).ToList();
        }

        public List<TEntity> GetList(
    Expression<Func<TEntity, bool>> filter,
    Func<IQueryable<TEntity>, IQueryable<TEntity>> eager,
    params Expression<Func<TEntity, object>>[] includeProperties
)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includeProperties != null)
            {
                foreach (var include in includeProperties)
                    query = query.Include(include);
            }

            if (eager != null)
                query = eager(query);

            if (filter != null)
                query = query.Where(filter);

            return query.ToList();
        }

        public async Task<List<TEntity>> GetListAsync(
            Expression<Func<TEntity, bool>> filter = null)
        {
            return filter == null
                ? await _context.Set<TEntity>().ToListAsync()
                : await _context.Set<TEntity>().Where(filter).ToListAsync();
        }

        public TEntity GetNoTracking(
            Expression<Func<TEntity, bool>> filter = null)
        {
            return filter == null
                ? _context.Set<TEntity>().AsNoTracking().FirstOrDefault()
                : _context.Set<TEntity>().AsNoTracking().FirstOrDefault(filter);
        }

        public async Task<TEntity> GetNoTrackingAsync(
            Expression<Func<TEntity, bool>> filter = null)
        {
            return await (filter == null
                ? _context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync()
                : _context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(filter));
        }

        public List<TEntity> GetListNoTracking(
            Expression<Func<TEntity, bool>> filter = null)
        {
            return filter == null
                ? _context.Set<TEntity>().AsNoTracking().ToList()
                : _context.Set<TEntity>().AsNoTracking().Where(filter).ToList();
        }

        public async Task<List<TEntity>> GetListNoTrackingAsync(
            Expression<Func<TEntity, bool>> filter = null)
        {
            return filter == null
                ? await _context.Set<TEntity>().AsNoTracking().ToListAsync()
                : await _context.Set<TEntity>().AsNoTracking().Where(filter).ToListAsync();
        }

        public void DeleteRange(Expression<Func<TEntity, bool>> filter)
        {
            var items = _context.Set<TEntity>().Where(filter).ToList();
            if (!items.Any()) return;
            _context.Set<TEntity>().RemoveRange(items);
            _context.SaveChanges();
        }

        public async Task DeleteRangeAsync(Expression<Func<TEntity, bool>> filter)
        {
            var items = await _context.Set<TEntity>().Where(filter).ToListAsync();
            if (!items.Any()) return;
            _context.Set<TEntity>().RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
