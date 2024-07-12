using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ZOI.DAL.DatabaseUtility.Interface
{
    public interface IRepository<T> where T : class
    {
        void Add(T entity);

        void Update(T entity);

        void Remove(T entity);

        IQueryable<T> All();

        IQueryable<T> AllAsNoTracking();

        T FindSingle(Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includes);

        IQueryable<T> Filter(Expression<Func<T, bool>> predicate);

        IQueryable<T> Filter(Expression<Func<T, bool>> filter, out int total, int index = 0, int size = 50);

        IQueryable<T> Find(Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includes);

        IQueryable<T> FindIncluding(params Expression<Func<T, object>>[] includeProperties);

        int Count(Expression<Func<T, bool>> predicate = null);

        bool Contains(Expression<Func<T, bool>> predicate);

        bool Exist(Expression<Func<T, bool>> predicate = null);

        /*
        PagedListResult<T> Search(SearchQuery<T> searchQuery);

        PagedListResult<T> Search(SearchQuery<T> searchQuery, out string query);
        */

        //asyc methods
        Task<List<T>> AllAsync();

        Task<T> FindSingleAsync(Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includes);

        Task<List<T>> FilterAsync(Expression<Func<T, bool>> predicate);

        Task<List<T>> FilterAsync(Expression<Func<T, bool>> filter, int index = 0, int size = 50);

        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includes);

        Task<List<T>> FindIncludingAsync(params Expression<Func<T, object>>[] includeProperties);

        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);

        Task<bool> ContainsAsync(Expression<Func<T, bool>> predicate);

        Task<bool> ExistAsync(Expression<Func<T, bool>> predicate = null);

        //Models.ModelsView.Common.DataTables.PagedListResult<T> Search(SearchQuery<T> searchQuery);
        //Models.ModelsView.Common.DataTables.PagedListResult<T> Search(SearchQuery<T> searchQuery, out string query);
    }
}
