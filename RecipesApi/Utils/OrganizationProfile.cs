using AutoMapper;
using RecipesApi.DTOs;
using RecipesApi.DTOs.Recipes;
using RecipesApi.Models;
using System.Linq;

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
            CreateMap<AddRecipeCategoryDto, RecipeCategory>();
            CreateMap<Recipe, GetRecipeDto>()
                .ForMember(dto => dto.Categories, r => r.MapFrom(r => r.RecipeCategories.Select(rc => rc.Category)));
            //CreateMap<Category, GetCategoryDto>();
            //CreateMap<RecipeBaseDto, Recipe>();
        }
    }
}
