using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace BackEnd.Interfaces.IRepositories
{
    public interface IGenericRepository<T> where T : class
    {
        
        Task<T?> FirstOrDefaultAsync(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, T>>? selectExpression = null,
            bool enableTracking = true,
            bool ignoreQueryFilters = false);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        ValueTask<EntityEntry<T>> InsertAsync(T entity);
        Task InsertRangeAsync(IEnumerable<T> entities);

        Task<List<T>> GetListAsync(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, T>>? selectExpression = null,
            bool enableTracking = true);

        Task<IQueryable<T>> GetIQuerableListAsync(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            Expression<Func<T, T>>? selectExpression = null,
            bool enableTracking = true);

        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity);
    }
}
