using Microsoft.Extensions.Logging;
using RecipesApi.Models;

namespace RecipesApi.Services
{
    /// <summary>
    /// Unit Service
    /// </summary>
    public class UnitsService: EntityService<Unit>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="logger">The logger</param>
        public UnitsService(DbContext context, ILogger<UnitsService> logger) : base(context, logger)
        {
        }

        /// <summary>
        /// Clean input before sending it to DB for creation/update
        /// </summary>
        /// <param name="input">The unit</param>
        /// <param name="isCreation">Is it a creation or update</param>
        /// <returns></returns>
        protected override Unit prepareInputForCreateOrUpdate(Unit input, bool isCreation)
        {
            if (isCreation)
            {
                input.Id = 0;
            }
            return input;
        }
    }
}
