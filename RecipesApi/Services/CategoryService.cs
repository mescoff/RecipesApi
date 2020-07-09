using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using RecipesApi.Utils;

namespace RecipesApi.Services
{
    public class CategoryService : EntityService<Category>
    {
        public CategoryService(DbContext context, ILogger<CategoryService> logger) : base(context, logger)
        {
        }

        // TODO: if we want to see Recipes for each category you need to override the Get and GetOne and Include RecipeCategories.ThenInclude(Recipe)

        /// <summary>
        /// Clean input before sending it to DB for creation/update
        /// </summary>
        /// <param name="input">The category</param>
        /// <param name="isCreation">Is it a creation or update</param>
        /// <returns></returns>
        protected override void prepareInputForCreateOrUpdate(Category input, bool isCreation)
        {
            // Make sure first letter is upper case
            //var name = Functions.FirstChatToUpper(input.Name.ToCharArray();
            var name = Functions.FirstCharToUpper(input.Name);
            //name[0] = char.ToUpper(name[0]);
            var description = Functions.FirstCharToUpper(input.Description);
            input.Name = name;
            input.Description = description;

            if (isCreation)
            {
                input.Id = 0;
            }
        }

    }
}
