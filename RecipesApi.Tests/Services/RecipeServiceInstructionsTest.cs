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
using RecipesApi.Utils;
using RecipesApi.DTOs;

namespace RecipesApi.Tests.Services
{
    [TestFixture]
    public class RecipeServiceInstructionsTests
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

        #region Update Recipe instructions
        [Test]
        public async Task RecipeUpdate_NewInstructions_AreAdded()
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
                    var recipeToUpdate = await service.GetOne(4);

                    // Add Ingredients to recipe
                    recipeToUpdate.Instructions.Add(new Instruction { Id = 0, StepNum = 3, Description = "Go do something", Recipe_Id = recipeToUpdate.Id });
                    recipeToUpdate.Instructions.Add(new Instruction { Id = 0, StepNum = 4, Description = "Done", Recipe_Id = recipeToUpdate.Id });
                    await service.UpdateOne(recipeToUpdate);

                    var dbrecipe = await service.GetOne(4);
                    Assert.AreEqual(4, dbrecipe.Instructions.Count());
                    var instructiontThree = dbrecipe.Instructions.FirstOrDefault(i => i.Id == 3);
                    Assert.AreEqual("Go do something", instructiontThree.Description);
                    Assert.AreEqual(3, instructiontThree.StepNum);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_NewInstructionWithGivenId_IsAddedAndAssignedNewIdByDatabase()
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
                    var recipeToUpdate = await service.GetOne(4);

                    // Add Ingredient with ID that doesn't exist yet
                    recipeToUpdate.Instructions.Add(new Instruction { Id = 75, StepNum = 3, Description = "Go do something", Recipe_Id = recipeToUpdate.Id });
                    await service.UpdateOne(recipeToUpdate);

                    var dbrecipe = await service.GetOne(4);
                    Assert.AreEqual(3, dbrecipe.Instructions.Count());
                    // According to our context, DB should override provided ID and assign ID 3 to this instruction
                    var instructiontThree = dbrecipe.Instructions.FirstOrDefault(i => i.Id == 3);
                    Assert.IsNotNull(instructiontThree);
                    Assert.AreEqual("Go do something", instructiontThree.Description);
                    Assert.AreEqual(3, instructiontThree.StepNum);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_ClearingAllInstructions_AreDeleted()
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
                    var recipeToUpdate = await service.GetOne(4);

                    // Remove all instructions
                    recipeToUpdate.Instructions = new List<Instruction>();
                    await service.UpdateOne(recipeToUpdate);

                    var dbrecipe = await service.GetOne(4);
                    //Assert.IsFalse(dbrecipe.Ingredients.Any());
                    Assert.AreEqual(0, dbrecipe.Instructions.Count());
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_ClearingOneInstructions_IsDeleted()
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
                    var recipeToUpdate = await service.GetOne(4);

                    // Remove instruction with Id = 1
                    var instruction = recipeToUpdate.Instructions.FirstOrDefault(i => i.Id == 1);
                    recipeToUpdate.Instructions.Remove(instruction);
                    await service.UpdateOne(recipeToUpdate);

                    var dbrecipe = await service.GetOne(4);
                    Assert.AreEqual(1, dbrecipe.Instructions.Count());
                    instruction = dbrecipe.Instructions.FirstOrDefault();
                    Assert.AreEqual(2, instruction.Id);
                    Assert.AreEqual("Start cutting stuff", instruction.Description);
                    Assert.AreEqual(2, instruction.StepNum);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_UpdatingInstruction_IsUpdated()
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
                    var recipeToUpdate = await service.GetOne(4);

                    // Update instruction with Id:1's Name and Unit
                    var instruction = recipeToUpdate.Instructions.FirstOrDefault(i => i.Id == 1);
                    // overriding instruction to remove linked/tracked objects, and modifying some properties
                    recipeToUpdate.Instructions.Remove(instruction);
                    recipeToUpdate.Instructions.Add(new Instruction { Id = instruction.Id, Description = "Do something new", StepNum = 3, Recipe_Id = instruction.Recipe_Id });

                    await service.UpdateOne(recipeToUpdate);

                    var dbrecipe = await service.GetOne(4);
                    Assert.AreEqual(2, dbrecipe.Instructions.Count());  // verify count hasn't changed
                    instruction = dbrecipe.Instructions.FirstOrDefault(i => i.Id == 1);
                    Assert.AreEqual("Do something new", instruction.Description);
                    Assert.AreEqual(3, instruction.StepNum);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_UpdatingRecipeAndInstruction_IsUpdated()
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
                    var recipeToUpdate = await service.GetOne(4);

                    // modifying recipe properties
                    recipeToUpdate.OriginalLink = "https://www.something.com";
                    recipeToUpdate.TitleShort = "Banana pancakes";

                    // modifying instruction with id 1
                    var instruction = recipeToUpdate.Instructions.FirstOrDefault(i => i.Id == 1);
                    // overriding instruction to remove linked/tracked objects, and modifying some properties
                    recipeToUpdate.Instructions.Remove(instruction);
                    recipeToUpdate.Instructions.Add(new Instruction { Id = instruction.Id, Description = "New Turn", StepNum = 7, Recipe_Id = instruction.Recipe_Id });

                    await service.UpdateOne(recipeToUpdate);

                    // get recipe again
                    var newRecipe = await service.GetOne(4);
                    Assert.AreEqual("Banana pancakes", newRecipe.TitleShort);
                    Assert.AreEqual("https://www.something.com", newRecipe.OriginalLink);

                    instruction = recipeToUpdate.Instructions.FirstOrDefault(i => i.Id == 1);
                    Assert.AreEqual("New Turn", instruction.Description);
                    Assert.AreEqual(7, instruction.StepNum);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_DeleteAddAndUpdateInstructions_WorkAsExpected()
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
                    // todo: is this bad practice? Should I just recreate an object Recipe here, with same Id? (Since we want to reproduce offline example). To avoid tracked/extra entities..
                    // get recipe with Id 4
                    var recipeToUpdate = await service.GetOne(4);

                    // Removing instruction with id 1
                    var instruction1 = recipeToUpdate.Instructions.FirstOrDefault(i => i.Id == 1);
                    recipeToUpdate.Instructions.Remove(instruction1);

                    // Modify instruction with Id 2
                    var instruction2 = recipeToUpdate.Instructions.FirstOrDefault(i => i.Id == 2);
                    recipeToUpdate.Instructions.Remove(instruction2);
                    recipeToUpdate.Instructions.Add(new Instruction { Id = instruction2.Id, Description = "New Turn", StepNum = 7, Recipe_Id = instruction2.Recipe_Id });

                    // Add new instruction
                    var instruction3 = new Instruction { Description = "Start mixing the eggs", StepNum = 3, Recipe_Id = 4 };
                    recipeToUpdate.Instructions.Add(instruction3);

                    // Get recipe again from db and verify all changes worked
                    await service.UpdateOne(recipeToUpdate);
                    var dbrecipe = await service.GetOne(4);
                    Assert.AreEqual(2, dbrecipe.Instructions.Count());
                    // recipe should have 2 instruction with id:2 and id:3 (new)
                    var instructionIds = dbrecipe.Instructions.Select(i => i.Id).ToList();
                    Assert.AreEqual(new int[2] { 2, 3 }, instructionIds);
                    // check modif in instruction with Id:2
                    instruction2 = dbrecipe.Instructions.FirstOrDefault(i => i.Id == 2);
                    Assert.AreEqual("New Turn", instruction2.Description);
                    Assert.AreEqual(7, instruction2.StepNum);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        #endregion

        // TODO: test
        // - there is NO POINT in testing validation on properties here. Validation is ran at Controller level not Service.

        private void SetupBasicContext(DbContextOptions<RecipesContext> options)
        {
            // Create the schema in the database
            using (var context = new RecipesContext(options))
            {
                //    RecipeServiceTestsHelper.EnsureCreated(context);
                //}

                //// Run the test against one instance of the context
                //using (var context = new RecipesContext(options))
                //{
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
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

                // Adding a Recipe with 2 instructions
                context.Recipes.Add(new Recipe()
                {
                    Id = 4,
                    TitleLong = "LongTitle",
                    TitleShort = "ShortTitle",
                    Description = String.Empty,
                    LastModifier = $"testuser@example.com",
                    OriginalLink = "",
                    AuditDate = null,
                    CreationDate = null,
                    Instructions = new List<Instruction> {
                        new Instruction { StepNum = 1, Description = "Fire up the Oven for 10 min", Recipe_Id = 4 },
                        new Instruction { StepNum = 2, Description = "Start cutting stuff", Recipe_Id = 4 }
                    }
                });
                context.SaveChanges();
            }
        }
    }
}
