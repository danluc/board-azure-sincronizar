using Back.Data.Context;
using Back.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Back.Data.Repository
{
    public class RepositorioComando<T> : IDisposable, IRepositorioComando<T> where T : class, IEntity
    {
        private readonly BancoDBContext _db;

        public RepositorioComando(BancoDBContext db)
        {
            _db = db;
        }

        public void Delete<TKey>(TKey key)
        {
            var entity = _db.Set<T>().Find(key);

            if (entity != null)
                Delete(entity);
        }

        public void Delete(T entidade)
        {
            _db.Set<T>().Remove(entidade);
        }

        public void Delete(Expression<Func<T, bool>> entidade)
        {
            var entity = _db.Set<T>().FirstOrDefault(entidade);

            if (entity != null)
                Delete(entity);
        }

        public void DeleteRange(Expression<Func<T, bool>> filter)
        {
            var entities = _db.Set<T>().Where(filter);
            _db.Set<T>().RemoveRange(entities);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public async Task<T> Insert(T entidade)
        {
            var entry = await _db.Set<T>().AddAsync(entidade);
            return entry.Entity;
        }

        public async Task InsertRange(IEnumerable<T> entidades)
        {
            await _db.Set<T>().AddRangeAsync(entidades);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync();
        }

        public void Update(T entidade)
        {
            _db.Set<T>().Update(entidade);
        }

        public void UpdateRange(IEnumerable<T> entidades)
        {
            _db.Set<T>().UpdateRange(entidades);
        }
    }
}
