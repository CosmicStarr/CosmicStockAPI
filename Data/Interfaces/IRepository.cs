namespace Data.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> Get(int Id);
        // Task<T> Get(string Id);
        Task<PagerList<T>> GetAllParams(PageParams Prams = null,Expression<Func<T,bool>> filter = null, 
        Func<IQueryable<T>, IOrderedQueryable<T>> orderby = null, 
        string includeproperties = null);
        Task<IEnumerable<T>> GetAll(Expression<Func<T,bool>> filter = null,
        Func<IQueryable<T>,IOrderedQueryable<T>> orderby = null,
        string includeProperties = null);
        Task<T> GetFirstOrDefault(Expression<Func<T,bool>> filter = null,
        string includeProperties = null);
        void Add(T entity);
        void Remove(T entity);
        void Remove(string Id);
        void RemoveRange(IEnumerable<T> entity);
        void Update(T entity);
    }
}