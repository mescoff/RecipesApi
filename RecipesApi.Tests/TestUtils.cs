using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipesApi.Tests
{
    public static class TestUtils
    {

        // TODO: Nope. This should be a class with IDisposable. See 4:02 of Chapter 9 - refactoring.
        // SO that it can be called temporarily for a test
        public static async Task<RecipesContext> GetRecipesContext()
        {
            var options = new DbContextOptionsBuilder<RecipesContext>()
                  .UseInMemoryDatabase("TestContext")
                  .EnableSensitiveDataLogging()
                  .Options;
            var recipeContext = new RecipesContext(options);
            recipeContext.Database.EnsureCreated();
            if (await recipeContext.Recipes.CountAsync() <= 0)
            {
                // todo: use AddRange()
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
