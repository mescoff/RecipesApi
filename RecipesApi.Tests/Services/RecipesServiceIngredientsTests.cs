using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.Tests.Services
{
    [TestFixture]
    public class RecipeServiceIngredientsTests
    {
        private Mock<ILogger<RecipesService>> _logger;

        [SetUp]
        public void Setup()
        {
            this._logger = new Mock<ILogger<RecipesService>>();
        }

        #region Update Recipe ingredients 
        [Test]
        public async Task RecipeUpdate_NewIngredients_AreAdded()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                SetupBasicContext(options);

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object);
                    var recipeToUpdate = await service.GetOne(4);

                    // Add Ingredients to recipe
                    recipeToUpdate.Ingredients.Add(new Ingredient { Id = 3, Name = "Butter", Quantity = 2, Unit_Id = 3, Recipe_Id = recipeToUpdate.Id });
                    recipeToUpdate.Ingredients.Add(new Ingredient { Id = 4, Name = "Flour", Quantity = 3, Unit_Id = 3, Recipe_Id = recipeToUpdate.Id });
                    await service.UpdateOne(recipeToUpdate);

                    var dbrecipe = await service.GetOne(4);
                    Assert.AreEqual(4, dbrecipe.Ingredients.Count());
                    var ingredientOne = dbrecipe.Ingredients.FirstOrDefault(i => i.Id == 3);
                    Assert.AreEqual("Butter", ingredientOne.Name);
                    Assert.AreEqual(2, ingredientOne.Quantity);
                    Assert.AreEqual(3, ingredientOne.Unit_Id);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_ClearingAllIngredients_AreDeleted()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                SetupBasicContext(options);

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object);
                    var recipeToUpdate = await service.GetOne(4);

                    // Remove all ingredients
                    recipeToUpdate.Ingredients = new List<Ingredient>();
                    await service.UpdateOne(recipeToUpdate);

                    var dbrecipe = await service.GetOne(4);
                    //Assert.IsFalse(dbrecipe.Ingredients.Any());
                    Assert.AreEqual(0, dbrecipe.Ingredients.Count());
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_ClearingOneIngredients_IsDeleted()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                SetupBasicContext(options);

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object);
                    var recipeToUpdate = await service.GetOne(4);

                    // Remove ingredient with Id = 1
                    var ingredient = recipeToUpdate.Ingredients.FirstOrDefault(i => i.Id == 1);
                    recipeToUpdate.Ingredients.Remove(ingredient);
                    await service.UpdateOne(recipeToUpdate);

                    var dbrecipe = await service.GetOne(4);
                    Assert.AreEqual(1, dbrecipe.Ingredients.Count());
                    ingredient = dbrecipe.Ingredients.FirstOrDefault();
                    Assert.AreEqual(2, ingredient.Id);
                    Assert.AreEqual("Flour", ingredient.Name);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_UpdatingIngredient_IsUpdated()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                SetupBasicContext(options);

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object);
                    var recipeToUpdate = await service.GetOne(4);

                    // Update ingredient with Id:1's Name and Unit
                    var ingredient = recipeToUpdate.Ingredients.FirstOrDefault(i => i.Id == 1);
                    // overriding ingredient to remove linked/tracked objects, and modifying some properties
                    recipeToUpdate.Ingredients.Remove(ingredient);
                    recipeToUpdate.Ingredients.Add(new Ingredient { Id = ingredient.Id, Name = "Strawberry", Quantity = 1, Unit_Id = 3, Recipe_Id = ingredient.Recipe_Id });

                    await service.UpdateOne(recipeToUpdate);

                    var dbrecipe = await service.GetOne(4);
                    Assert.AreEqual(2, dbrecipe.Ingredients.Count());  // verify count hasn't changed
                    ingredient = dbrecipe.Ingredients.FirstOrDefault(i => i.Id == 1);
                    Assert.AreEqual("Strawberry", ingredient.Name);
                    Assert.AreEqual(1, ingredient.Quantity);
                    Assert.AreEqual(3, ingredient.Unit_Id);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_UpdatingRecipeAndIngredient_IsUpdated()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                SetupBasicContext(options);

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object);
                    var recipeToUpdate = await service.GetOne(4);

                    // modifying recipe properties
                    recipeToUpdate.OriginalLink = "https://www.something.com";
                    recipeToUpdate.TitleShort = "Banana pancakes";

                    // modifying ingredient with id 1
                    var ingredient = recipeToUpdate.Ingredients.FirstOrDefault(i => i.Id == 1);
                    // overriding ingredient to remove linked/tracked objects, and modifying some properties
                    recipeToUpdate.Ingredients.Remove(ingredient);
                    recipeToUpdate.Ingredients.Add(new Ingredient { Id = ingredient.Id, Name = "Strawberry", Quantity = 1, Unit_Id = 3, Recipe_Id = ingredient.Recipe_Id });

                    await service.UpdateOne(recipeToUpdate);

                    // get recipe again
                    var newRecipe = await service.GetOne(4);
                    Assert.AreEqual("Banana pancakes", newRecipe.TitleShort);
                    Assert.AreEqual("https://www.something.com", newRecipe.OriginalLink);

                    ingredient = recipeToUpdate.Ingredients.FirstOrDefault(i => i.Id == 1);
                    Assert.AreEqual("Strawberry", ingredient.Name);
                    Assert.AreEqual(1, ingredient.Quantity);
                    Assert.AreEqual(3, ingredient.Unit_Id);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_DeleteAddAndUpdateIngredients_WorkAsExpected()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                SetupBasicContext(options);

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object);
                    // todo: is this bad practice? Should I just recreate an object Recipe here, with same Id? (Since we want to reproduce offline example). To avoid tracked/extra entities..
                    // get recipe with Id 4
                    var recipeToUpdate = await service.GetOne(4);

                    // Removing ingredient with id 1
                    var ingredient1 = recipeToUpdate.Ingredients.FirstOrDefault(i => i.Id == 1);
                    recipeToUpdate.Ingredients.Remove(ingredient1);

                    // Modify ingredient with Id 2
                    var ingredient2 = recipeToUpdate.Ingredients.FirstOrDefault(i => i.Id == 2);
                    recipeToUpdate.Ingredients.Remove(ingredient2);
                    recipeToUpdate.Ingredients.Add(new Ingredient { Id = ingredient2.Id, Name = "Strawberry", Quantity = 1, Unit_Id = 3, Recipe_Id = ingredient2.Recipe_Id });

                    // Add new ingredient
                    var ingredient3 = new Ingredient() { Id = 3, Name = "Flour", Quantity = 3, Unit_Id = 1, Recipe_Id = 4 };
                    recipeToUpdate.Ingredients.Add(ingredient3);

                    // Get recipe again from db and verify all changes worked
                    await service.UpdateOne(recipeToUpdate);
                    var dbrecipe = await service.GetOne(4);
                    Assert.AreEqual(2, dbrecipe.Ingredients.Count());
                    // recipe should have 2 ingredient with id:2 and id:3 (new)
                    var ingredientIds = dbrecipe.Ingredients.Select(i => i.Id).ToList();
                    Assert.AreEqual(new int[2] { 2, 3 }, ingredientIds);
                    // check modif in ingredient with Id:2
                    ingredient2 = dbrecipe.Ingredients.FirstOrDefault(i => i.Id == 2);
                    Assert.AreEqual("Strawberry", ingredient2.Name);
                    Assert.AreEqual(1, ingredient2.Quantity);
                    Assert.AreEqual(3, ingredient2.Unit_Id);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        // TODO: Tests
        // - Check for duplicate ingredients?
        // - Remove everything and add new ones
        #endregion

        private void SetupBasicContext(DbContextOptions<RecipesContext> options)
        {
            // Create the schema in the database
            using (var context = new RecipesContext(options))
            {
                RecipeServiceTestsHelper.EnsureCreated(context);
            }

            // Run the test against one instance of the context
            using (var context = new RecipesContext(options))
            {
                // Adding 2 units
                context.Units.Add(new Unit()
                {
                    Id = 1,
                    Name = "Cup",
                    Symbol = "Cl"
                });
                context.Units.Add(new Unit()
                {
                    Id = 3,
                    Name = "Gram",
                    Symbol = "G"
                });

                // Adding a Recipe with 2 ingredients
                context.Recipes.Add(new Recipe()
                {
                    Id = 4,
                    TitleLong = "LongTitle",
                    TitleShort = "ShortTitle",
                    Description = String.Empty,
                    LastModifier = $"testuser@example.com",
                    OriginalLink = "",
                    AuditDate = new DateTime(2019, 12, 03),
                    CreationDate = new DateTime(),
                    Ingredients = new List<Ingredient> {
                        new Ingredient { Id = 1, Name = "Chocolate", Quantity = 4, Unit_Id = 1 },
                        new Ingredient { Id = 2, Name = "Flour", Quantity = 4, Unit_Id = 1 }
                    }
                });
                context.SaveChanges();
            }
        }



        //private static void RecipeServiceTestsHelper.EnsureCreated(RecipesContext context)
        //{
        //    if (context.Database.RecipeServiceTestsHelper.EnsureCreated())
        //    {
        //        using var viewCommand = context.Database.GetDbConnection().CreateCommand();
        //        viewCommand.CommandText = @"
        //                CREATE VIEW AllResources AS
        //                SELECT *
        //                FROM Recipes;";
        //        viewCommand.ExecuteNonQuery();
        //    }
        //}
    }
}
