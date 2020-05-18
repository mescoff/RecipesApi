using Microsoft.Extensions.Logging;
using RecipesApi.Models;

namespace RecipesApi.Services
{
    public class MediaService : EntityService<Media>
    {
        public MediaService(DbContext context, ILogger<MediaService> logger) : base(context, logger)
        {
        }

        protected override void prepareInputForCreateOrUpdate(Media input, bool isCreation)
        {
            // TODO: Not sure this is needed now that we added the [DatabaseGenerated(DatabaseGeneratedOption.Identity)] attribute at Model level
            if (isCreation)
            {
                input.Id = 0;
            }
            //return input;
        }
    }
}
