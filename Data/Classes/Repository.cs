namespace Data.Classes
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            dbSet = context.Set<T>();

        }
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public async Task<T> Get(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderby = null, string includeProperties = null)
        {
                IQueryable<T> query = dbSet;
                if(filter != null)
                {
                    query = query.Where(filter);
                }
                if(includeProperties != null)
                {
                    foreach (var item in includeProperties.Split( new char [] {','},StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(item);
                    }
                }
                if(orderby != null)
                {
                    return await orderby(query).ToListAsync();
                }
                return await query.ToListAsync();
        }

        public async Task<PagerList<T>> GetAllParams(PageParams Params = null,Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderby = null, string includeproperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (includeproperties != null)
            {
                foreach (var item in includeproperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            if (orderby != null)
            {
                return await PagerList<T>.CreateAsync(orderby(query), Params.PageNumber, Params.PageSize);
            }
            return await PagerList<T>.CreateAsync(query, Params.PageNumber, Params.PageSize);
        }

        public async Task<T> GetFirstOrDefault(Expression<Func<T, bool>> filter = null, string includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if(filter != null)
            {
                query = query.Where(filter);
            }
            if(includeProperties != null)
            {
                foreach (var item in includeProperties.Split( new char [] {','},StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            return await query.FirstOrDefaultAsync();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void Remove(string Id)
        {
            var obj = dbSet.Find(Id);
            dbSet.Remove(obj);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }

        public void Update(T entity)
        {
            dbSet.Update(entity).State = EntityState.Modified;
        }
    }
}