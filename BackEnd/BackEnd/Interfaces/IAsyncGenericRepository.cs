using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace BackEnd.Interfaces
{
    public interface IAsyncGenericRepository<T>
        where T : class
    {
        #region Insert Functions

      

        void Update(T entity);
        void Update(params T[] entities);
        void Update(IEnumerable<T> entities);

        #endregion Insert Functions

      
        IQueryable<T> Where(Expression<Func<T, bool>> expression);

        IQueryable<T> GetAll();

        Task<IPaginate<T>> GetListAsync(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            Expression<Func<T, T>>? selectExpression = null,
            int index = 0,
            int size = 20,
            bool enableTracking = true,
            CancellationToken cancellationToken = default);


      

        /// <summary>
        /// Retrieves the full list of entities without paging
        /// </summary>
        /// <param name="filterPredicate"></param>
        /// <param name="orderBy"></param>
        /// <param name="include"></param>
        /// <param name="enableTracking"></param>
        /// <returns></returns>
        Task<List<T>> GetFullListAsync(Func<IQueryable<T>, IQueryable<T>>? filterPredicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, T>>? selectExpression = null,
            bool enableTracking = true);

        /// <summary>
        /// Retrieves the TOP 1 
        /// </summary>
        /// <param name="filterPredicate"></param>
        /// <param name="orderBy"></param>
        /// <param name="include"></param>
        /// <param name="enableTracking"></param>
        /// <returns></returns>

      
        Task<TResult> MaxValue<TResult>(Expression<Func<T, TResult>>? selectExpression = null);

        IQueryable<TSelection> Select<TSelection>(Expression<Func<T, TSelection>> expression);

        void Delete(T entity);

        void Delete(params T[] entities);

        void Delete(IEnumerable<T> entities);
    }
}