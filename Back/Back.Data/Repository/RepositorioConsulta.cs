using Microsoft.EntityFrameworkCore;
using Back.Data.Context;
using Back.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Back.Data.Repository
{
    public class RepositorioConsulta<T> : IDisposable, IRepositorioConsulta<T> where T : class, IEntity
    {
        private readonly BancoDBContext _db;

        public RepositorioConsulta(BancoDBContext db)
        {
            _db = db;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
        public async Task<bool> Any(Expression<Func<T, bool>> filter, params string[] includes) => await Query(true, includes).AnyAsync(filter);

        public async Task<T> FindById(int id, bool readOnly = false, params string[] includes)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<T>> FindBy(Expression<Func<T, bool>> filter, bool readOnly = false, params string[] includes)
        {
            return await Query(readOnly, includes).Where(filter).ToListAsync();
        }

        public async Task<IEnumerable<TResult>> FindBy<TResult>(Expression<Func<T, TResult>> selection, Expression<Func<T, bool>> filter, bool readOnly = false, params string[] includes)
        {
            return await Query(readOnly, includes).Where(filter).Select(selection).ToListAsync();
        }

        public async Task<T> FirstOrDefault(Expression<Func<T, bool>> filter, bool readOnly = false, params string[] includes)
        {
            return await Query(readOnly, includes).FirstOrDefaultAsync(filter);
        }

        public async Task<TResult> FirstOrDefault<TResult>(Expression<Func<T, TResult>> selection, Expression<Func<T, bool>> filter, bool readOnly = false, params string[] includes)
        {
            return await Query(readOnly, includes).Where(filter).Select(selection).FirstOrDefaultAsync();
        }

        public IQueryable<T> Query(bool readOnly = false, params string[] includes)
        {
            var query = readOnly
                 ? _db.Set<T>().AsNoTracking()
                 : _db.Set<T>();

            if (includes != null &&
                includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query;
        }

        public IQueryable<T> Query(Expression<Func<T, bool>> filter, bool readOnly = false, params string[] includes) => Query(readOnly, includes).Where(filter);
    }
}
