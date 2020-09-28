using RecipesApi.Utils;
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
        Task<ServiceResponse<T>> AddOne(T input);
        Task<bool> DeleteOne(int id);
        Task<bool> UpdateOne(T input);
    }
}