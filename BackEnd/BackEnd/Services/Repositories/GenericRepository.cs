using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using BackEnd.Data;
using BackEnd.Entities;
using BackEnd.Interfaces.IRepositories;

namespace BackEnd.Services.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : EntityBase
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            this._context = context;
            _dbSet = _context.Set<T>();

        }


        public async ValueTask<EntityEntry<T>> InsertAsync(T entity)
        {
            var entityDb = await _dbSet.AddAsync(entity);
            return entityDb;
        }

        public virtual Task InsertRangeAsync(IEnumerable<T> entities)
        {
            return _dbSet.AddRangeAsync(entities);
        }
        public IQueryable<T> BaseQuery(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, T>>? selectExpression = null,
            bool enableTracking = true,
            bool ignoreQueryFilters = false)
        {
            IQueryable<T> query = _dbSet;

            if (!enableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (filterPredicate != null)
            {
                query = filterPredicate(query);
            }

            if (selectExpression != null)
            {
                query = query.Select(selectExpression);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }
            return query;
        }

        public async Task<T?> FirstOrDefaultAsync(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, T>>? selectExpression = null,
            bool enableTracking = true,
            bool ignoreQueryFilters = false)
        {
            IQueryable<T> query = BaseQuery(filterPredicate, include, enableTracking: enableTracking, ignoreQueryFilters: ignoreQueryFilters, selectExpression: selectExpression);
            return orderBy != null ? await orderBy(query).FirstOrDefaultAsync() : await query.FirstOrDefaultAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = _dbSet;

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "Predicate can not be null");
            }

            return await query.AnyAsync(predicate);
        }

        public async Task<List<T>> GetListAsync(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, T>>? selectExpression = null,
            bool enableTracking = true)
        {
            IQueryable<T> query = BaseQuery(filterPredicate, include, enableTracking: enableTracking, selectExpression: selectExpression);

            return await (orderBy != null
                ? orderBy(query).ToListAsync() : query.ToListAsync());
        }

        public async Task<IQueryable<T>> GetIQuerableListAsync(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, T>>? selectExpression = null,
            bool enableTracking = true)
        {
            IQueryable<T> query = BaseQuery(filterPredicate, include, enableTracking: enableTracking, selectExpression: selectExpression);

            return (orderBy != null ? orderBy(query) : query);
        }
        public void Update(T entity)
        {
            _dbSet.Update(entity);

        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
