using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Back.Dominio.Interfaces
{
    public interface IRepositorioComando<TEntidade> where TEntidade : class, IEntity
    {
        Task<TEntidade> Insert(TEntidade entidade);
        Task InsertRange(IEnumerable<TEntidade> entidades);
        void Update(TEntidade entidade);
        void UpdateRange(IEnumerable<TEntidade> entidades);
        void Delete<TKey>(TKey key);
        void Delete(TEntidade entidade);
        void Delete(Expression<Func<TEntidade, bool>> entidade);
        void DeleteRange(Expression<Func<TEntidade, bool>> filter);
        Task<int> SaveChangesAsync();
    }
}
