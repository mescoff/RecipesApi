using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;
using System.Threading.Tasks;
using System;
using RecipesApi.Utils;
using RecipesApi.DTOs.Recipes;
using System.Collections.Generic;
using RecipesApi.DTOs;
using System.Text.RegularExpressions;

namespace RecipesApi.Tests.Services
{
    [TestFixture]
    public class RecipeServiceRecipeTests
    {
        private Mock<ILogger<RecipesService>> _logger;
        private Mock<IMediaLogicHelper> _mediaHelper;

        [SetUp]
        public void Setup()
        {
            this._logger = new Mock<ILogger<RecipesService>>();
            this._mediaHelper = new Mock<IMediaLogicHelper>();
            this._mediaHelper.Setup(h => h.LocateAndLoadMedias(It.IsAny<IEnumerable<Media>>())).Returns(new List<MediaDto>());
        }

        #region Add
        [Test]
        // Test adding recipe with instructions/ingredients/all props and 1 media
        public async Task RecipeAdd_WithAllProperties_Works()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                SetupBasicContext(options); // TODO: move into setup?
                int createdRecipeId = -1;
                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object, this._mediaHelper.Object);
                    var newRecipe = new RecipeDto
                    {
                        Description = "Something",
                        LastModifier = "xx",
                        TitleShort = "NewRecipe",
                        TitleLong = "Gorgeous wedding cake",
                        OriginalLink = "https://www.foodnetwork.com/recipes/geoffrey-zakarian/classic-gin-gimlet-2341489",
                        Id = 5 // should reset to 0 and be assigned by DB
                    };

                    var response = await service.AddOne(newRecipe);
                    Assert.IsTrue(response.Success);
                    var rgx = new Regex(@"^.*Id:(?<id>[0-9])$");
                    var match = rgx.Match(response.Message);
                    createdRecipeId = Convert.ToInt32(match.Groups["id"].Value);
                }
                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object, this._mediaHelper.Object);
                    var recipe = await service.GetOne(createdRecipeId);

                    Assert.AreEqual("Something", recipe.Description);
                    Assert.AreEqual("xx", recipe.LastModifier);
                    Assert.AreEqual("NewRecipe", recipe.TitleShort);
                    Assert.AreEqual("Gorgeous wedding cake", recipe.TitleLong);
                    Assert.AreEqual("https://www.foodnetwork.com/recipes/geoffrey-zakarian/classic-gin-gimlet-2341489", recipe.OriginalLink);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeAdd_WithNullShortTitle_Fails()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                SetupBasicContext(options); // TODO: move into setup?
                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object, this._mediaHelper.Object);
                    var newRecipe = new RecipeDto { Description = "Something", LastModifier = "xx", TitleShort = null };
                    // 
                    //Assert.ThrowsAsync<SqliteException>(async () => await service.AddOne(newRecipe));

                    var response = await service.AddOne(newRecipe);
                    Assert.IsFalse(response.Success);
                    Assert.IsTrue(response.Message.Contains("Microsoft.EntityFrameworkCore.DbUpdateException"));
                    Assert.IsTrue(response.Message.Contains("SQLite Error 19"));
                    Assert.IsTrue(response.Message.Contains("NOT NULL constraint failed: recipes.TitleShort"));
                }

            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        [Ignore("Not working at the moment")]
        // TODO: isn't this handled by validation tests ?
        public async Task RecipeAdd_WithShortTitleLongerThan50Char_Fails()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                SetupBasicContext(options); // TODO: move into setup?
                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object, this._mediaHelper.Object);
                    var rdm = new Random();
                    var shortTitle = rdm.GenerateRandomString(52);
                    var length = shortTitle.Length;
                    var newRecipe = new RecipeDto { Description = "Something", LastModifier = "xx", TitleShort = shortTitle };
                    // 
                    //Assert.ThrowsAsync<SqliteException>(async () => await service.AddOne(newRecipe));

                    var response = await service.AddOne(newRecipe);
                    Assert.IsFalse(response.Success);
                    Assert.IsTrue(response.Message.Contains("Microsoft.EntityFrameworkCore.DbUpdateException"));
                    Assert.IsTrue(response.Message.Contains("SQLite Error 19"));
                    Assert.IsTrue(response.Message.Contains("NOT NULL constraint failed: recipes.TitleShort"));
                }

            }
            finally
            {
                connection.Close();
            }
        }

        #endregion

        #region Update
        [Test]
        public async Task RecipeUpdate_UpdatingRecipe_IsUpdated()
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
                    var service = new RecipesService(context, this._logger.Object, this._mediaHelper.Object);
                    var recipeUpdate = await service.GetOne(4);

                    // modifying recipe properties
                    recipeUpdate.OriginalLink = "https://www.something.com";
                    recipeUpdate.TitleShort = "Banana pancakes";


                    await service.UpdateOne(recipeUpdate);
                }
                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object, this._mediaHelper.Object);
                    // get recipe again
                    var newRecipe = await service.GetOne(4);
                    Assert.AreEqual("Banana pancakes", newRecipe.TitleShort);
                    Assert.AreEqual("https://www.something.com", newRecipe.OriginalLink);
                    // TODO: Test also that auditDate and creationDate are ignored (and handled by database)
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_UpdatingAuditDate_IsIgnored()
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
                    var service = new RecipesService(context, this._logger.Object, this._mediaHelper.Object);
                    var recipeUpdate = await service.GetOne(4);

                    var currentTime = DateTime.Now;
                    recipeUpdate.AuditDate = currentTime;

                    await service.UpdateOne(recipeUpdate);
                }
                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object, this._mediaHelper.Object);
                    // get recipe again in new "request" / separate context
                    var newRecipe = await service.GetOne(4);
                    Assert.AreEqual(null, newRecipe.AuditDate); // should be unchanged, aka: should be null since it can only be generated by DB
                }

            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_UpdatingCreationDate_IsIgnored()
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
                    var service = new RecipesService(context, this._logger.Object, this._mediaHelper.Object);
                    var recipeUpdate = await service.GetOne(4);

                    var currentTime = DateTime.Now;
                    recipeUpdate.CreationDate = currentTime;

                    await service.UpdateOne(recipeUpdate);
                }
                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._logger.Object, this._mediaHelper.Object);
                    // get recipe again in new "request" / separate context
                    var newRecipe = await service.GetOne(4);
                    Assert.AreEqual(null, newRecipe.CreationDate); // should be unchanged, aka: should be null since it can only be generated by DB
                }

            }
            finally
            {
                connection.Close();
            }
        }
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
                });
                context.SaveChanges();
            }
        }
    }
}
