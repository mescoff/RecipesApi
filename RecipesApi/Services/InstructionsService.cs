using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipesApi.Services
{
    public class InstructionsService: EntityService<Instruction>
    {
        public InstructionsService(DbContext context, ILogger<InstructionsService> logger) : base(context, logger)
        {
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
