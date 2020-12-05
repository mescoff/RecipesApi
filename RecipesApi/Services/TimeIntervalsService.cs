using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipesApi.Models;
using System.Collections.Generic;

namespace RecipesApi.Services
{
    public class TimeIntervalsService : EntityService<TimeInterval>
    {
        public TimeIntervalsService(RecipesContext context, ILogger<TimeIntervalsService> logger) : base(context, logger)
        {
        }


        public override IEnumerable<TimeInterval> GetAll()
        {
            var result = this.Entities.Include(t => t.Label).Include(t => t.TimeSpans);
            return result;
        }


        /// <summary>
        /// Clean input before sending it to DB for creation/update
        /// </summary>
        /// <param name="input">The Time Interval</param>
        /// <param name="isCreation">Is it a creation or update</param>
        /// <returns></returns>
        protected override void prepareInputForCreateOrUpdate(TimeInterval input, bool isCreation)
        {         
            if (isCreation)
            {
                input.Id = 0;
            }
        }
    }
}
