using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipesApi.Services
{
    public class InstructionsService: EntityService<Instruction>
    {
        public InstructionsService(RecipesContext context, ILogger<InstructionsService> logger) : base(context, logger)
        {
        }

        public override IEnumerable<Instruction> GetAll()
        {
            var result = this.Entities.Include(i => i.Media);
            return result;
        }

        public async override Task<Instruction> GetOne(int id)
        {
            var result = await this.Entities.Include(i => i.Media).SingleOrDefaultAsync(r => r.Id == id);
            return result;
        }

        protected override void prepareInputForCreateOrUpdate(Instruction input, bool isCreation)
        {
            if (isCreation)
            {
                input.Id = 0;
            }
        }

    }
}
