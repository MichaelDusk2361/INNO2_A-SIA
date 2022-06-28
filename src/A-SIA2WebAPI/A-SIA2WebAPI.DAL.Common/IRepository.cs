using System;
using System.Collections.Generic;

namespace A_SIA2WebAPI.DAL.Common
{
    /// <summary>
    /// Common repository interface for database CRUD operations
    /// </summary>
    /// <typeparam name="T">The model which this interface handles in the database</typeparam>
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T? Get(Guid id);
        bool Insert(ref T entity);
        bool Update(ref T entity);
        bool Delete(Guid id);
    }
}
