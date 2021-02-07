using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipesApi.DTOs.Recipes;
using RecipesApi.Models;
using RecipesApi.Utils;
using System;
using System.Threading.Tasks;

namespace RecipesApi.Services
{
    public class RecipeCategoryService: EntityService<RecipeCategory>
    {
        public RecipeCategoryService(RecipesContext context, ILogger<RecipeCategoryService> logger) : base(context, logger)
        {
        }

        // TODO: below might not be needed, it's only useful because it adds more details if missing one model. Otherwise could just use the parent AddOne
        public async override Task<ServiceResponse<RecipeCategory>> AddOne(RecipeCategory input)
        {
            var response = new ServiceResponse<RecipeCategory>();
            try
            {
                var recipe = await this.Context.Set<Recipe>()
                    .Include(r => r.Medias)
                    .Include(r => r.RecipeCategories).ThenInclude(i => i.Category)
                    .Include(r => r.Instructions).Include(r => r.Ingredients)
                    .ThenInclude(i => i.Unit)
                    .SingleOrDefaultAsync(r => r.Id == input.Recipe_Id);
                if (recipe == null)
                {
                    response.Success = false;
                    response.Message = "Recipe not found";
                    return response;
                }
                var category = await this.Context.Set<Category>().SingleOrDefaultAsync(r => r.Id == input.Category_Id);
                if (category == null)
                {
                    response.Success = false;
                    response.Message = "Category not found";
                    return response;
                }

                var recipeCategory = new RecipeCategory
                {
                    Recipe = recipe,
                    Category = category,
                };

                await this.Entities.AddAsync(recipeCategory);
                await this.Context.SaveChangesAsync();
                response.Content = recipeCategory;
                response.Success = true;

            }
            catch (Exception e)
            {
                // No need to Remove entity from context since it was a new object not tracked ??
                response.Success = false;
                response.Message = e.Message;
            }
            return response;
        }


        protected override void prepareInputForCreateOrUpdate(RecipeCategory input, bool isCreation)
        {
            // TODO: Not sure this is needed now that we added the [DatabaseGenerated(DatabaseGeneratedOption.Identity)] attribute at Model level
            if (isCreation)
            {
                input.Id = 0;
            }
        }
    }
}
