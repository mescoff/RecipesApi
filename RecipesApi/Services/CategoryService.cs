using Microsoft.Extensions.Logging;
using RecipesApi.Models;

namespace RecipesApi.Services
{
    public class CategoryService : EntityService<Category>
    {
        public CategoryService(DbContext context, ILogger<CategoryService> logger) : base(context, logger)
        {
        }

        /// <summary>
        /// Clean input before sending it to DB for creation/update
        /// </summary>
        /// <param name="input">The category</param>
        /// <param name="isCreation">Is it a creation or update</param>
        /// <returns></returns>
        protected override void prepareInputForCreateOrUpdate(Category input, bool isCreation)
        {
            if (isCreation)
            {
                input.Id = 0;
            }
        }

    }
}
