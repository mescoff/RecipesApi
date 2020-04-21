using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipesApi.Tests
{
    public static class TestUtils
    {
        public static async Task<DbContext> GetRecipesContext()
        {
            var options = new DbContextOptionsBuilder<DbContext>()
                  .UseInMemoryDatabase("TestContext")
                  .Options;
            var recipeContext = new DbContext(options);
            recipeContext.Database.EnsureCreated();
            if (await recipeContext.Recipes.CountAsync() <= 0)
            {
                for (int i = 1; i <= 5; i++)
                {
                    recipeContext.Recipes.Add(new Recipe()
                    {
                        Id = i,
                        TitleLong = (i % 2 == 0) ? $"LongTitle{i}" : String.Empty,
                        TitleShort = $"ShortTitle{i}",
                        Description = String.Empty,
                        LastModifier = $"testuser{i}@example.com",
                        OriginalLink = "",
                        AuditDate = new DateTime(2019, 12, 03),
                        CreationDate = new DateTime(),
                        Ingredients = new List<Ingredient> { new Ingredient { Id = 0, Name = $"Ingredient{i}", Quantity = 4, Unit_Id = 2 } }
                    });
                    await recipeContext.SaveChangesAsync();
                }
            }
            return recipeContext;
        }
    }
}
