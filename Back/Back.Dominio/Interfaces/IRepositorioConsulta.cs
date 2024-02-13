using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Back.Dominio.Interfaces
{
    public interface IRepositorioConsulta<TEntidade> where TEntidade : class, IEntity
    {
        Task<TEntidade> FindById(int id, bool readOnly = false, params string[] includes);
        Task<IEnumerable<TEntidade>> FindBy(Expression<Func<TEntidade, bool>> filter, bool readOnly = false, params string[] includes);
        Task<IEnumerable<TResult>> FindBy<TResult>(Expression<Func<TEntidade, TResult>> selection, Expression<Func<TEntidade, bool>> filter, bool readOnly = false, params string[] includes);
        Task<TEntidade> FirstOrDefault(Expression<Func<TEntidade, bool>> filter, bool readOnly = false, params string[] includes);
        Task<TResult> FirstOrDefault<TResult>(Expression<Func<TEntidade, TResult>> selection, Expression<Func<TEntidade, bool>> filter, bool readOnly = false, params string[] includes);
        Task<bool> Any(Expression<Func<TEntidade, bool>> filter, params string[] includes);
        IQueryable<TEntidade> Query(bool readOnly = false, params string[] includes);
        IQueryable<TEntidade> Query(Expression<Func<TEntidade, bool>> filter, bool readOnly = false, params string[] includes);
    }
}
