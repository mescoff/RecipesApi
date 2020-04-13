using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using RecipesApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipesApi
{
    /// <summary>
    /// Recipe service
    /// Handles relaying requests between controller and DB + perform logic when necessary
    /// </summary>
    public class RecipesService : EntityService<RecipeBase>  //IEntityService<RecipeBase>
    {
      
        public RecipesService(RecipesContext context, ILogger<RecipesService> logger) : base(context, logger)
        {
        }

        public  override IEnumerable<RecipeBase> GetAll()
        {
            var result = this._context.Set<RecipeBase>().Include(r => r.Ingredients).ThenInclude(i => i.Unit);
            return result;
        }

        public async override Task<RecipeBase> GetOne(int id)
        {               
            var result = await this._context.Set<RecipeBase>().Include(r => r.Ingredients).ThenInclude(i => i.Unit).SingleOrDefaultAsync(r => r.Recipe_Id == id);
            return result;
        }

        /// <summary>
        /// Cleanse the object of any unwanted parameters before pushing to DB
        /// </summary>
        /// <param name="recipe">The recipe</param>
        /// <returns></returns>
        protected override RecipeBase prepareInputForUpdate(RecipeBase input, bool isCreation)
        {
            if (isCreation)
            {
                input.Recipe_Id = 0;
                input.CreationDate = null;
                input.AuditDate = null;
            }
            else
            {
                input.AuditDate = DateTime.Now;
            }   
            return input;
        }
    }
}
