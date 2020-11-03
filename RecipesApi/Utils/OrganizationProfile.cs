using AutoMapper;
using RecipesApi.DTOs;
using RecipesApi.DTOs.Recipes;
using RecipesApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Drawing;


namespace RecipesApi.Utils
{
    public class OrganizationProfile: Profile
    {

        public OrganizationProfile()
        {
            CreateMap<Ingredient, IngredientDto>();
            CreateMap<IngredientDto, Ingredient>();
            CreateMap<MediaDto, Media>();
            CreateMap<Media, MediaDto>()
                .ForMember(dto => dto.MediaBytes, m => m.MapFrom<CustomMediaResolver>());
            CreateMap<AddRecipeCategoryDto, RecipeCategory>();
            CreateMap<Recipe, GetRecipeDto>()
                .ForMember(dto => dto.Categories, r => r.MapFrom(r => r.RecipeCategories.Select(rc => rc.Category)));
            //CreateMap<Category, GetCategoryDto>();
            //CreateMap<RecipeBaseDto, Recipe>();
        }
    }

    public class CustomMediaResolver : IValueResolver<Media, MediaDto, byte[]>
    {
        public byte[] Resolve(Media source, MediaDto destination, byte[] member, ResolutionContext context)
        {
            try
            {
                var imgBytes = System.IO.File.ReadAllBytes(source.MediaPath);
                // Validating it's an image
                try
                {
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        var image = new Bitmap(ms);
                    }
                }
                catch (ArgumentException)
                {
                    return new byte[1];
                }
                return imgBytes;
            }
            catch (Exception e)
            {
                // TODO: temporary solution. Final solution should be -> Remove from recipe media list if can't find
                return new byte[1];
                //throw (e);
            }
        }
    }
}
