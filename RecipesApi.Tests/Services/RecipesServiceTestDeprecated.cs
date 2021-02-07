using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RecipesApi.DTOs.Recipes;
using RecipesApi.Models;
using RecipesApi.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipesApi.Tests.Services
{
    [TestFixture]
    [Ignore("Deprecated")]
    public class RecipesServiceTestDeprecated
    {
        private Mock<IEntityService<Recipe>> _recipeServicesMock;
        private Mock<ILogger<RecipesService>> _logger;
        private Mock<IMediaLogicHelper> _mediaHelper;

        [SetUp]
        public void Setup()
        {
            //this._logger = new Mock<ILogger<IEntityService<Recipe>>>();
            this._logger = new Mock<ILogger<RecipesService>>();
            this._mediaHelper = new Mock<IMediaLogicHelper>();
            this._mediaHelper.SetupGet(h => h.UserMediasPath).Returns("C:\\Users\\Manon\\Programming\\Apps\\Recipes\\Media\\2301\\RecipeImages\\SpinashTart");
        }

        [Test]
        public async Task Test_GetAllReturnsAll()
        {
            var dbContext = await TestUtils.GetRecipesContext();
            using (var recipeService = setupService(dbContext))
            {
                var result = recipeService.GetAll();
                Assert.That(result.ToList().Count() == 5);
            }
        }

        [Test]
        public async Task Test_GetOneRecipeWorks()
        {
            var dbContext = await TestUtils.GetRecipesContext();
            using (var recipeService = setupService(dbContext))
            {
                var recipe = await recipeService.GetOne(2);
                Assert.AreEqual(2, recipe.Id);
                Assert.AreEqual(new DateTime(2019, 12, 03), recipe.AuditDate);
                Assert.AreEqual("ShortTitle2", recipe.TitleShort);
                Assert.AreEqual(String.Empty, recipe.Description);
                Assert.AreEqual(String.Empty, recipe.OriginalLink);
                Assert.AreEqual(1, recipe.Ingredients.Count());
            }
        }

        [Test]
        public void Test_PostOneRecipeWorks()
        {
            var options = new DbContextOptionsBuilder<RecipesContext>()
                  .UseInMemoryDatabase("AddOneRecipe")
                  .Options;
            using (var recipeContext = new RecipesContext(options))
            {
                var recipeService = setupService(recipeContext);
                var creationDate = new DateTime(2019, 12, 03);
                var auditDate = DateTime.Now;
                recipeService.AddOne(new RecipeDto
                {
                    Id = 1,
                    TitleLong = "LongTitle",
                    TitleShort = "ShortTitle",
                    Description = String.Empty,
                    LastModifier = "testuser@example.com",
                    OriginalLink = "www",
                    AuditDate = auditDate,
                    CreationDate = creationDate,
                    Ingredients = new List<Ingredient> { new Ingredient { Id = 0, Name = "Chocolate", Quantity = 4, Unit_Id = 2 } }
                });
            }
            // Use a separate instance of the context to verify correct data was saved to database
            using (var recipeContext = new RecipesContext(options))
            {
                Assert.AreEqual(1, recipeContext.Recipes.Count());
                Assert.AreEqual("LongTitle", recipeContext.Recipes.Single().TitleLong);
                Assert.AreEqual("ShortTitle", recipeContext.Recipes.Single().TitleShort);
                Assert.AreEqual("testuser@example.com", recipeContext.Recipes.Single().LastModifier);
                // Since DB handles audit and creation date for new items, we should have null here
                Assert.AreEqual(null, recipeContext.Recipes.Single().AuditDate);
                Assert.AreEqual(null, recipeContext.Recipes.Single().CreationDate);
              
                var ings = recipeContext.Recipes.Include(r => r.Ingredients).Single().Ingredients;
                Assert.AreEqual(1, ings.Count());
                Assert.AreEqual("Chocolate", ings.First().Name);
                Assert.AreEqual(4, ings.First().Quantity);
            }
        }

        [Test]
        public async Task Test_PostNotExistingRecipeFails()
        {
            var dbContext = await TestUtils.GetRecipesContext();
            using (var recipeService = setupService(dbContext))
            {
                var recipe = new RecipeDto { Id = 780, TitleShort = "Some recipe" };
                var result = await recipeService.UpdateOne(recipe);
                Assert.IsFalse(result); // TODO: not sufficiant. Get real error msg to work in Service
            }
        }

        [Test]
        public async Task Test_PostAndPutRecipeWithDuplicateIngredientsIdThrowsException()
        {
            var dbContext = await TestUtils.GetRecipesContext();
            using (var recipeService = setupService(dbContext))
            {
                var creationDate = new DateTime(2019, 12, 03);
                var auditDate = DateTime.Now;

                Assert.Throws<DuplicateNameException>(() => recipeService.AddOne(new RecipeDto
                {
                    Id = 1,
                    TitleLong = "LongTitle",
                    TitleShort = "ShortTitle",
                    Description = String.Empty,
                    LastModifier = "testuser@example.com",
                    OriginalLink = "www",
                    AuditDate = auditDate,
                    CreationDate = creationDate,
                    Ingredients = new List<Ingredient> {
                        new Ingredient { Id = 4, Name = "Chocolate", Quantity = 4, Unit_Id = 2 },
                        new Ingredient { Id = 4, Name = "Soja", Quantity = 4, Unit_Id = 2 } }
                }));

                //var recipe = dbContext.Set<Recipe>().Include(r => r.Ingredients).First();
                //recipe.Ingredients = new List<Ingredient>{
                //        new Ingredient { Id = 4, Name = "Chocolate", Quantity = 4, Unit_Id = 2 },
                //        new Ingredient { Id = 4, Name = "Soja", Quantity = 4, Unit_Id = 2 }
                //};

                //Assert.Throws<DuplicateNameException>(() => recipeService.UpdateOne(recipe));
            }
        }

        [Test]
        public async Task Test_DeleteRecipeClearsIngredient()
        {
            var dbContext = await TestUtils.GetRecipesContext();
            using (var recipeService = setupService(dbContext))
            {
                var recipe = dbContext.Set<Recipe>().Include(r => r.Ingredients).First();
                var recipeIngredient = recipe.Ingredients.First();

                recipeService.DeleteOne(recipe.Id);
                var recipes = dbContext.Set<Recipe>();
                Assert.IsNull(recipes.Find(recipe.Id));
                var ingredients = dbContext.Set<Ingredient>();
                Assert.IsNull(ingredients.Find(recipeIngredient.Id));
            }
        }

        [Test]
        public async Task Test_DeleteNotExistingRecipeFails()
        {
            var dbContext = await TestUtils.GetRecipesContext();
            using (var recipeService = setupService(dbContext))
            {
                var result = await recipeService.DeleteOne(909);
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void Test_PostWithRecipeUpdateAndIngredientsUpdateAndDeleteWorks()
        {
            var options = new DbContextOptionsBuilder<RecipesContext>()
                  .UseInMemoryDatabase("PostRecipe") //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                  .EnableSensitiveDataLogging()
                  .Options;

            using (var recipeContext = new RecipesContext(options))
            {

                // TODO: MOCK ALL BELOW. 
                var recipe = new Recipe
                {
                    Id = 0,
                    CreationDate = DateTime.Now,
                    AuditDate = DateTime.Now,
                    TitleShort = "Crepes",
                    Ingredients = new List<Ingredient> { new Ingredient { Id = 3, Name = "Flour", Quantity = 2, Unit_Id = 1 }, new Ingredient { Id = 7, Name = "Flour", Quantity = 2, Unit_Id = 1 }
                    }
                };
                recipeContext.Set<Recipe>().Add(recipe);
                // recipeContext.Entry<Recipe>(recipe).State = EntityState.Detached;
                recipeContext.SaveChanges();

            }
            using (var recipeContext = new RecipesContext(options))
            {
                var recipeService = setupService(recipeContext);
                var recipes = recipeContext.Set<Recipe>().ToList();
                // New recipe with same ID and updates to ingredients
                var recipe = new RecipeDto
                {
                    Id = 1,
                    CreationDate = DateTime.Now,
                    AuditDate = DateTime.Now,
                    TitleShort = "French Crepes with brown butter",
                    Ingredients = new List<Ingredient> { new Ingredient { Id = 3, Name = "Flour", Quantity = 6, Unit_Id = 1 }, new Ingredient { Id = 4, Name = "Sugar", Quantity = 3, Unit_Id = 1 }
                    }
                };

                recipeService.UpdateOne(recipe);
                var updatedRecipe = recipeContext.Set<Recipe>().Include(r => r.Ingredients).FirstOrDefault(r => r.Id == recipe.Id);
                Assert.AreNotEqual(updatedRecipe.AuditDate, recipe.AuditDate);
                Assert.AreEqual(2, recipe.Ingredients.Count());
                Assert.IsNull(recipe.Ingredients.FirstOrDefault(i => i.Id == 7));
                Assert.AreEqual("Sugar", recipe.Ingredients.FirstOrDefault(i => i.Id == 4));
            }
        }
        

            private RecipesService setupService(RecipesContext dbContext)
        {
            return new RecipesService(dbContext, this._logger.Object, this._mediaHelper.Object);
        }

    }
}
