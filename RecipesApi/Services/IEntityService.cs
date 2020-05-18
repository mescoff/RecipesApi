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
        Task<int> AddOne(T recipe);
        Task<bool> DeleteOne(int id);
        Task<bool> UpdateOne(T input);
    }
}