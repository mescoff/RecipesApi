using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipesApi.Services
{
    public class IngredientsService: EntityService<Ingredient>
    {
        public IngredientsService(DbContext context, ILogger<IngredientsService> logger) : base(context, logger)
        {
        }

        public override IEnumerable<Ingredient> GetAll()
        {
            
            var result = this.Entities.Include(r => r.Unit);
            return result;
        }

        public async override Task<Ingredient> GetOne(int id)
        {
            var result = await this.Entities.Include(r => r.Unit).SingleOrDefaultAsync(r => r.Id == id);
            return result;
        }

        protected override void prepareInputForCreateOrUpdate(Ingredient input, bool isCreation)
        {
            if (isCreation)
            {
                input.Id = 0;
            }
        }

    }
}
