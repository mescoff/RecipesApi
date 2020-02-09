using Microsoft.Extensions.Logging;
using RecipesApi.Models;

namespace RecipesApi.Services
{
    public class IngredientsService: EntityService<Ingredient>
    {
        public IngredientsService(RecipesContext context, ILogger<IngredientsService> logger) : base(context, logger)
        {
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
