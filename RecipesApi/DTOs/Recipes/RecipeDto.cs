using RecipesApi.Models;
using System.Collections.Generic;

namespace RecipesApi.DTOs.Recipes
{
    // For now creating a general RecipeDto that will be used in Service.
    // Needed for Media... Media needs to be transformed at Service level from DTO to Db Model
    // For now this is my solution

    /// <summary>
    /// Recipe class with overriden properties to allow better flexibility
    /// </summary>
    public class RecipeDto: Recipe, ICustomModel<RecipeDto>
    {       
        public RecipeDto()
        {
            this.Medias = new List<MediaDto>();
        }

        public new IList<MediaDto> Medias { get; set; }

        public bool Equals(RecipeDto obj)
        {
            return (
               this.Id == obj.Id &&
               this.TitleShort == obj.TitleShort &&
               this.TitleLong == obj.TitleLong &&
               this.Description == obj.Description &&
               this.OriginalLink == obj.OriginalLink &&
               this.LastModifier == obj.LastModifier &&
               this.AuditDate == obj.AuditDate &&
               this.CreationDate == obj.CreationDate
               );
        }
    }
}
