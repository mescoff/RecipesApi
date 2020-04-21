using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipesApi
{
    public interface IEntityService<T> : IDisposable
    {
        // TODO: should get all be async?
        IEnumerable<T> GetAll();
        Task<T> GetOne(int id);
        bool AddOne(T recipe);
        bool DeleteOne(int id);
        bool UpdateOne(T input);
    }
}