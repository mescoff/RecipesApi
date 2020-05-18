using AutoMapper;
using RecipesApi.DTOs;
using RecipesApi.Models;

namespace RecipesApi.Utils
{
    public class OrganizationProfile: Profile
    {

        public OrganizationProfile()
        {
            CreateMap<Ingredient, IngredientDto>();
            CreateMap<IngredientDto, Ingredient>();
            CreateMap<MediaDto, Media>();
            CreateMap<Media, MediaDto>();
            //CreateMap<Recipe, RecipeBaseDto>();
            //CreateMap<RecipeBaseDto, Recipe>();
        }
    }
}
