using MySql.Data.MySqlClient;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.Repositories
{
    public interface IRecipesRepository
    {
        Task<IEnumerable<RecipeBase>> GetAllRecipes();
    }
}
