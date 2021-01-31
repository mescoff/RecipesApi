using RecipesApi.DTOs;
using RecipesApi.Models;

namespace RecipesApi.Utils
{
    public interface IMediaLogicHelper
    {
        ServiceResponse<Media> SaveMediaLocally(MediaDto media, string recipeShortTitle);
        string FullMediaPath { get; }

    }
}