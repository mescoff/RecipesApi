using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipesApi.Services
{
    public class IngredientsService: EntityService<Ingredient>
    {
        public IngredientsService(RecipesContext context, ILogger<IngredientsService> logger) : base(context, logger)
        {
        }

        public override IEnumerable<Ingredient> GetAll()
        {
            
            var result = this._context.Set<Ingredient>().Include(r => r.Unit);
            return result;
        }

        public async override Task<Ingredient> GetOne(int id)
        {
            var result = await this._context.Set<Ingredient>().Include(r => r.Unit).SingleOrDefaultAsync(r => r.RecipeIng_Id == id);
            return result;
        }

        protected override Ingredient prepareInputForUpdate(Ingredient input, bool isCreation)
        {
            if (isCreation)
            {
                input.RecipeIng_Id = 0;
            }
            return input;
        }

    }
}
