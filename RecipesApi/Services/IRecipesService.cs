using RecipesApi.DTOs.Recipes;
using RecipesApi.Models;
using RecipesApi.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipesApi
{
    public interface IRecipesService<RecipeDto> : IDisposable 
    {
        IEnumerable<RecipeDto> GetAll();
        Task<RecipeDto> GetOne(int id);
        Task<bool> UpdateOne(RecipeDto updatedRecipe);
        Task<ServiceResponse<RecipeDto>> AddOne(RecipeDto input);
        Task<bool> DeleteOne(int id);
    }
}