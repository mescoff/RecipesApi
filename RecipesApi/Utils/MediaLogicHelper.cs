using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecipesApi.DTOs;
using RecipesApi.Models;
using System;
using System.IO;
using System.Linq;

namespace RecipesApi.Utils
{
    /// <summary>
    /// Media Logic Helper : Help upload and download pictures from server
    /// MediaPath will be in format :  <BaseDirectory>/<User>/RecipeImages/<RecipeName>/imageName.png
    /// </summary>
    public class MediaLogicHelper: IMediaLogicHelper
    {
        /// <summary>
        /// Base directory for users media
        /// </summary>
        private readonly string _mediaBaseDirectory;

        /// <summary>
        /// Full path for this request (MediaDirectory+UserSpecificFolder)
        /// </summary>
        public string FullMediaPath { get; }

        private readonly ILogger<MediaLogicHelper> _logger;

      
        public MediaLogicHelper(IConfiguration configuration, ILogger<MediaLogicHelper> logger)
        {
            this._logger = logger;
            try
            {
                this._mediaBaseDirectory = configuration["MediaPath:BaseDirectory"];
                var userId = "2301"; // FOR DEBUG PURPOSES
                var imagesPath = configuration["MediaPath:ImagesPath"];
                this.FullMediaPath = Path.Combine(_mediaBaseDirectory, userId, imagesPath);
                // Create directory if not exist
                Directory.CreateDirectory(this.FullMediaPath);
                this._logger.LogInformation($"Recipe image will be saved in {this.FullMediaPath}");
            }
            catch(Exception e)
            {
                this._logger.LogError($"Something happened: {e}");
                throw e;
            }
        }

        public ServiceResponse<Media> SaveMediaLocally(MediaDto media, string recipeShortTitle)
        {
            var recipeName = String.Concat(recipeShortTitle.Where(c => !Char.IsWhiteSpace(c)));
            
            //var imageName = 
            return new ServiceResponse<Media>();
        }


        // standardize size of image. This will be at most on a laptop. Image should not weight more than 1Mo
        // Name image properly
        // Get general path from config, then set path per user.   UserId/Media/RecipeImages
    }
}
