using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi
{
    public class RecipesService: IRecipesService
    {
        private readonly ILogger _logger;
        private readonly RecipesContext _context;
        public RecipesService(ILogger<IRecipesService> logger, RecipesContext context)
        {
            this._logger = logger;
            this._context = context;
        }

        public IEnumerable<RecipeBase> GetAllRecipes()
        {
            return this._context.Recipes;
        }
    }
}
