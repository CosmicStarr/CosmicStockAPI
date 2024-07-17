using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Classes
{
    public class UnitOfWork : IUnitOfWork
    {
        private Hashtable _repo;
        private readonly ApplicationDbContext _contex;
        public UnitOfWork(ApplicationDbContext Contex)
        {
            _contex = Contex;
        }
        public async Task<int> Complete()
        {
            return await _contex.SaveChangesAsync();
        }

        public void Dispose()
        {
            _contex.Dispose();
        }

        public IRepository<T> Repository<T>() where T : class
        {
            if(_repo == null) _repo = new Hashtable();
            var type = typeof(T).Name;
            if(!_repo.ContainsKey(type))
            {
                var repoType = typeof (Repository<>);
                var repoInstance = Activator.CreateInstance(repoType.MakeGenericType(typeof(T)),_contex);
                _repo.Add(type,repoInstance);
            }
            return (IRepository<T>)_repo[type];
        }
    }
}