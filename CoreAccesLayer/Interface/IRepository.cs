using CoreAccesLayer.Wraper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CoreAccesLayer.Interface
{
    public interface IRepository
    {
        bool SaveObject<T>(Entity<T> entity) where T : class, new();
        bool CallProcedure<T>(string nameProcedure, params object[] parameters) where T : class, new();
        List<T> GetDataByProcedure<T>(string nameProcedure, params object[] parameters) where T : class, new();
        List<T> SimpleSelect<T>(Expression<Func<T, bool>> predicate) where T : class, new();
        List<T> Getall<T>() where T : class, new();
        bool Commit();
        bool Rollback();

    }
}
