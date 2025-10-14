using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using BackEnd.Interfaces;

namespace BackEnd.Services
{
    public class AsyncGenericRepository<T> : IAsyncGenericRepository<T> where T : class
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public AsyncGenericRepository(DbContext dbContext)
        {
            _dbContext=dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        #region SingleOrDefault
        protected virtual IQueryable<T> BaseQuery(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
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
       
       
        public Task<TResult> MaxValue<TResult>(Expression<Func<T, TResult>>? selectExpression = null)
        {
            return _dbSet.MaxAsync(selectExpression);
        }

        #endregion SingleOrDefault


        public IQueryable<TSelection> Select<TSelection>(Expression<Func<T, TSelection>> expression)
        {
            return _dbSet.Select(expression);
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Where(expression);
        }
        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }
        #region GetListAsync

        public Task<IPaginate<T>> GetListAsync(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            Expression<Func<T, T>>? selectExpression = null,
            int index = 0,
            int size = 20,
            bool enableTracking = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = BaseQuery(filterPredicate, include, enableTracking: enableTracking, selectExpression: selectExpression);

            return orderBy != null
                ? orderBy(query).ToPaginateAsync(index, size, 0, cancellationToken)
                : query.ToPaginateAsync(index, size, 0, cancellationToken);
        }

       

        public async Task<List<T>> GetFullListAsync(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, T>>? selectExpression = null,
            bool enableTracking = true)
        {
            IQueryable<T> query = BaseQuery(filterPredicate, include, enableTracking: enableTracking, selectExpression: selectExpression);

            return await (orderBy != null
                ? orderBy(query).ToListAsync() : query.ToListAsync());
        }

        #endregion GetListAsync

        #region Delete Functions

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Delete(params T[] entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void Delete(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        #endregion Delete Functions

        #region Insert Functions

        

        #endregion Insert Functions

        #region Update Functions

        public void Update(T entity)
        {
            _dbSet.Update(entity);
            
        }

        public void Update(params T[] entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void Update(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        #endregion Update Functions


    }
}