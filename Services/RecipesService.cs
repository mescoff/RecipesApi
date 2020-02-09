using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using RecipesApi.Services;
using System;

namespace RecipesApi
{
    /// <summary>
    /// Recipe service
    /// Handles relaying requests between controller and DB + perform logic when necessary
    /// </summary>
    public class RecipesService : EntityService<RecipeBase>  //IEntityService<RecipeBase>
    {
        private readonly ILogger _logger;
        private readonly RecipesContext _context;
        public RecipesService(RecipesContext context, ILogger<RecipesService> logger) : base(context, logger)
        {
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
