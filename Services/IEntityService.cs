using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;
using System.Collections.Generic;

namespace RecipesApi
{
    public interface IEntityService<T> 
    {
        IEnumerable<T> GetAll();
        T GetOne(int id);
        bool AddOne(T recipe);
        bool DeleteOne(int id);
        bool UpdateOne(T input);
    }
}