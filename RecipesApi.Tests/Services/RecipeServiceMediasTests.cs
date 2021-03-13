using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RecipesApi.DTOs;
using RecipesApi.Models;
using RecipesApi.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecipesApi.Tests.Services
{
    [SingleThreadedAttribute]
    [TestFixture]
    public class RecipeServiceMediasTests
    {
        private Mock<ILogger<RecipesService>> _loggerMock;
        private Mock<IConfiguration> _configuration;
        private IMediaLogicHelper _mediaHelper;
        private IMapper _mapper;
        private List<Media> _contextMedia;  // keep track of Media list used in context created by test (override in each test)
        private string _savedMediasPath;
        private string _mediasPath; // path where to load image byte for testing purposes (act like received img)
        private readonly string _imagesDirectory = @"RecipeImages";
        private readonly long MAX_IMAGESIZE = 800000;
        private readonly int USER_ID = 2301;

        // TODO: Should/can we mock Helper? 

        [SetUp]
        public void Setup()
        {
            this._loggerMock = new Mock<ILogger<RecipesService>>();
            var workingDirectory = Environment.CurrentDirectory;
            this._mediasPath = Path.Combine(workingDirectory, "TestData");

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OrganizationProfile());
            });
            this._mapper = mapperConfig.CreateMapper();

            this._configuration = new Mock<IConfiguration>();
            this._configuration.SetupGet(c => c["MediasSettings:BasePath"]).Returns(this._mediasPath);
            this._configuration.SetupGet(c => c["MediasSettings:ImagesDirectory"]).Returns(this._imagesDirectory);
            this._configuration.SetupGet(c => c["MediasSettings:MaxImageSize"]).Returns(Convert.ToString(MAX_IMAGESIZE));

            this._mediaHelper = new MediaLogicHelper(this._configuration.Object, new Mock<ILogger<MediaLogicHelper>>().Object);
            //this._savedMediasPath = Path.Combine(workingDirectory, "TestData", "Saved"); // TODO: rethink this. It will then need UserId/RecipeId
            //this._mediaHelper.SetupGet(h => h.UserMediasPath).Returns("C:\\Users\\Manon\\Programming\\Apps\\Recipes\\Media\\2301\\RecipeImages\\SpinashTart");
            //this._mediaHelper.SetupGet(h => h.UserMediasPath).Returns(this._savedMediasPath);
        }

        #region Update Recipe medias
        [Test]
        public async Task RecipeUpdate_AddingNewImagesToRecipeWithNoImages_WorksAndPathCreatedProperly()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                var recipeId = 1; // TODO: why is this text conflicting with DeleteMultipleImages ? Not same path nor Recipe ID and diff contexts
                // Setup context with one recipe, no medias
                SetupBasicContext(options, true, recipeId);

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    // get recipe with ID setup for this test suite
                    var recipeToUpdate = await service.GetOne(recipeId);

                    // loading 3 mediaDtos from json
                    var mediaDtos = LoadRecipeMediaDtoFromJson(recipeId);
                    // insert into recipe that we want to update, to act like incoming media bytes
                    recipeToUpdate.Medias = mediaDtos;

                    await service.UpdateOne(recipeToUpdate);
                }
                using (var context = new RecipesContext(options))
                {
                    // VERIFY
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);

                    // Get recipe again from DB with new medias
                    var recipeToUpdate = await service.GetOne(recipeId);
                    Assert.AreEqual(3, recipeToUpdate.Medias.Count());
                    string userRecipePath; // not needed so we override
                    foreach (var i in recipeToUpdate.Medias)
                    {
                        var savedImgPath = MediaLogicHelper.GenerateSingleMediaPath(this._mediaHelper.UserMediasPath, recipeToUpdate.Id, i.Id, out userRecipePath);
                        // check file exists
                        Assert.IsTrue(File.Exists(savedImgPath));

                        long savedImageSize = new FileInfo(savedImgPath).Length;
                        // verify size is under 800kb
                        Assert.Less(savedImageSize, this.MAX_IMAGESIZE);
                    }

                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_UpdatingExistingImageProperties_UpdatesProperly()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                var recipeId = 2;
                // Setup context with one recipe, and medias. No specific media path provided since test doesn't modify media physically, can remain in standard path
                SetupBasicContext(options, false, recipeId);

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    // get recipe with ID 4
                    var recipeToUpdate = await service.GetOne(recipeId);

                    // Update media with ID: 3
                    var media = recipeToUpdate.Medias.Where(m => m.Id == 1).First();
                    // overriding ingredient to remove linked/tracked objects, and modifying some properties
                    // TODO: verify why object is being tracked? When we have asNoTracking set
                    recipeToUpdate.Medias.Remove(media);
                    recipeToUpdate.Medias.Add(new MediaDto { Id = media.Id, Recipe_Id = media.Recipe_Id, Title = "OrangeJuice", Tag = "Juice", MediaBytes = media.MediaBytes });

                    await service.UpdateOne(recipeToUpdate);
                }
                using (var context = new RecipesContext(options))
                {
                    // VERIFY
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    var recipeUpdated = await service.GetOne(recipeId);
                    var image = recipeUpdated.Medias.Where(m => m.Id == 1).First();
                    Assert.AreEqual("OrangeJuice", image.Title);
                    Assert.AreEqual("Juice", image.Tag);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_UpdatingExistingImageBytes_UpdatesProperly()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                var recipeId = 3;
                // Setup context with one recipe, and medias. Specifying separate media path since modifying image physically (to avoid conflict with other tests)
                SetupBasicContext(options, false, recipeId, "RecipeUpdate_UpdatingExistingImageBytes");

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    // get recipe with ID 4
                    var recipeToUpdate = await service.GetOne(recipeId);

                    // Update media with ID: 1
                    var media = recipeToUpdate.Medias.Where(m => m.Id == 3).First();
                    // overriding ingredient to remove linked/tracked objects, and modifying some properties
                    // TODO: verify why object is being tracked? When we have asNoTracking set
                    recipeToUpdate.Medias.Remove(media);

                    // Load images json and select image 1 (foodMarket.jpg) that we know isn't in Media with ID 1 (foodColor.jpeg)
                    var TestImages = LoadRecipeMediaDtoFromJson(recipeId);
                    // Update properties AND mediabytes
                    recipeToUpdate.Medias.Add(new MediaDto { Id = media.Id, Recipe_Id = media.Recipe_Id, Title = "FoodMarket", Tag = media.Tag, MediaBytes = TestImages.First().MediaBytes });
                    await service.UpdateOne(recipeToUpdate);
                }
                using (var context = new RecipesContext(options))
                {
                    // VERIFY
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    var recipeUpdated = await service.GetOne(recipeId);
                    var image = recipeUpdated.Medias.Where(m => m.Id == 3).First();
                    Assert.AreEqual("FoodMarket", image.Title);

                    // Verify logger wrote INFO specifying that Media with Id 1 had its image overriden
                    this._loggerMock.Verify(l =>
                    l.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("New image provided")), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())
                    , Times.Once);
                    //l.Log(LogLevel.Information, 0, It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);           
                }
            }
            finally
            {
                connection.Close();
            }
        }


        [Test]
        public async Task RecipeUpdate_DeletingOneImage_DeletesProperly()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                var recipeId = 4; // choose recipe ID not used by other test to prevent conflicts on result checks
                // Setup context with one recipe, and medias. Specifying separate media path since modifying image physically (to avoid conflict with other tests)
                // TODO: Each test should have their own data to work with
                SetupBasicContext(options, false, recipeId, "RecipeUpdate_DeletingOneImage");

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    // get recipe with ID defined at class level
                    // TODO: we're doing all of this wrong... You need a moqed environment for all test to work properly... The mediaLogicHelperTest already handles testing physical loading

                    var recipeToUpdate = await service.GetOne(recipeId);
                    // Remove media with ID: 3
                    var image = recipeToUpdate.Medias.Where(m => m.Id == 3).First();
                    recipeToUpdate.Medias.Remove(image);


                    await service.UpdateOne(recipeToUpdate);
                }
                using (var context = new RecipesContext(options))
                {
                    // VERIFY
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    var recipeUpdated = await service.GetOne(recipeId);
                    var image = recipeUpdated.Medias.Where(m => m.Id == 3).FirstOrDefault();
                    Assert.IsNull(image);
                    Assert.AreEqual(2, recipeUpdated.Medias.Count, "Medias count should be 2");
                    // Verify logger wrote INFO specifying that Image 3 was deleted
                    this._loggerMock.Verify(l =>
                    l.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DELETED") && v.ToString().Contains("ID:3")), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())
                    , Times.Once);
                    // verify file no longer exist   
                    // File path was defined in SetupBasicContext, it's not following regular path standards (from recipe service)
                    var imgToDeletePath = this._contextMedia.Where(m => m.Id == 3).First().MediaPath;
                    Assert.IsFalse(File.Exists(imgToDeletePath));
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        // TODO: bad practice. If doing integration test that use physical disk, setup different folder with pics for each test
        public async Task RecipeUpdate_DeletingMultipleImages_DeletesProperly()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                var recipeId = 5;
                // Setup context with one recipe, and medias. Specifying separate media path since modifying image physically (to avoid conflict with other tests)
                SetupBasicContext(options, false, recipeId, "RecipeUpdate_DeletingMultipleImages");
                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    // get recipe with ID defined at class level
                    var recipeToUpdate = await service.GetOne(recipeId);

                    // Remove all medias       
                    recipeToUpdate.Medias.Clear();

                    await service.UpdateOne(recipeToUpdate);
                }
                using (var context = new RecipesContext(options))
                {
                    // VERIFY
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    var recipeUpdated = await service.GetOne(recipeId);
                    Assert.IsEmpty(recipeUpdated.Medias);
                    // Verify logger wrote INFO specifying that Images were deleted, for all 3 images
                    this._loggerMock.Verify(l =>
                        l.Log(LogLevel.Information, 0, It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DELETED")), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())
                        , Times.Exactly(3));
                    // Verify files no longer exist
                    foreach (var media in this._contextMedia)
                    {
                        Assert.IsFalse(File.Exists(media.MediaPath));
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_AddingOneImageToRecipeWithImages_IsAddedProperlyAndPathCreated()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                var recipeId = 6;
                // Setup context with one recipe, and medias
                SetupBasicContext(options, false, recipeId);

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    // get recipe with ID defined at class level
                    var recipeToUpdate = await service.GetOne(recipeId);

                    // loading mediaDtos from json (to have media with image bytes)
                    var mediaDtos = LoadRecipeMediaDtoFromJson(recipeId);
                    var mediaToAdd = mediaDtos.First();
                    recipeToUpdate.Medias.Add(mediaToAdd);

                    await service.UpdateOne(recipeToUpdate);
                }
                using (var context = new RecipesContext(options))
                {
                    // VERIFY
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    var recipeUpdated = await service.GetOne(recipeId);
                    Assert.AreEqual(4, recipeUpdated.Medias.Count);

                    string userRecipePath;
                    // We can't access image path but we can recreate it from method used by service when saving it
                    var savedImgPath = MediaLogicHelper.GenerateSingleMediaPath(this._mediaHelper.UserMediasPath, recipeUpdated.Id, 4, out userRecipePath);
                    // check file exists
                    Assert.IsTrue(File.Exists(savedImgPath));
                    long savedImageSize = new FileInfo(savedImgPath).Length;
                    // verify size is under 800kb
                    Assert.Less(savedImageSize, this.MAX_IMAGESIZE);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Test]
        public async Task RecipeUpdate_AddingMultipleImagesToRecipeWithImages_AreAddedProperlyAndPathCreated()
        {
            // each test creates new Connection / Options / DbSchema
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<RecipesContext>()
                    .UseSqlite(connection)
                    .Options;

                var recipeId = 7;
                // Setup context with one recipe, and medias
                SetupBasicContext(options, false, recipeId);

                using (var context = new RecipesContext(options))
                {
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    // get recipe with ID defined at class level
                    var recipeToUpdate = await service.GetOne(recipeId);

                    // loading mediaDtos from json (to have media with image bytes)
                    var mediaDtos = LoadRecipeMediaDtoFromJson(recipeId).ToArray<MediaDto>();
                    // Adding multiple media
                    recipeToUpdate.Medias.Add(mediaDtos[0]);
                    recipeToUpdate.Medias.Add(mediaDtos[1]);

                    await service.UpdateOne(recipeToUpdate);
                }
                using (var context = new RecipesContext(options))
                {
                    // VERIFY
                    var service = new RecipesService(context, this._loggerMock.Object, this._mediaHelper, this._mapper);
                    var recipeUpdated = await service.GetOne(recipeId);
                    Assert.AreEqual(5, recipeUpdated.Medias.Count, "Medias count should be 5");

                    string userRecipePath;
                    // CHECK for IMAGE with id 4
                    // We can't access image path but we can recreate it from method used by service when saving it
                    var savedImgPath = MediaLogicHelper.GenerateSingleMediaPath(this._mediaHelper.UserMediasPath, recipeUpdated.Id, 4, out userRecipePath);
                    // check file exists
                    Assert.IsTrue(File.Exists(savedImgPath), $"Image with path {savedImgPath} should not exist");
                    long savedImageSize = new FileInfo(savedImgPath).Length;
                    // verify size is under 800kb
                    Assert.Less(savedImageSize, this.MAX_IMAGESIZE);

                    // CHECK for IMAGE with id 5
                    savedImgPath = MediaLogicHelper.GenerateSingleMediaPath(this._mediaHelper.UserMediasPath, recipeUpdated.Id, 5, out userRecipePath);
                    // check file exists
                    Assert.IsTrue(File.Exists(savedImgPath), $"Image with path {savedImgPath} should not exist");
                    savedImageSize = new FileInfo(savedImgPath).Length;
                    // verify size is under 800kb
                    Assert.Less(savedImageSize, this.MAX_IMAGESIZE);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        #endregion

        private IList<MediaDto> LoadRecipeMediaDtoFromJson(int recipeId)
        {
            // loading 3 mediaDtos from json
            List<MediaDto> mediaDtos;
            var testMediasBytesPath = Path.Combine(this._mediasPath, "recipeMediaDtos.json");
            using (StreamReader r = new StreamReader(testMediasBytesPath))
            {
                string json = r.ReadToEnd();
                mediaDtos = JsonSerializer.Deserialize<List<MediaDto>>(json);
            }
            mediaDtos.ForEach(m => m.Recipe_Id = recipeId); // making sure recipeId is set to recipe we're updating      

            return mediaDtos;
        }

        // TODO: TESTS
        // TEST that updating Media value that was null works
        // TEST that updating Media value with null works
        // Make sure we can't change around how we deal with physical media in MediaLogicHelperTests
        // Rename the update... image to something normal

        /// <summary>
        /// Setup a test set
        /// </summary>
        /// <param name="options">The Context Options</param>
        /// <param name="EmptyMedia">Return recipe if empty images list or with full images list</param>
        /// <param name="recipeId">Set different recipe Id on each context for test ran in a row</param>
        private void SetupBasicContext(DbContextOptions<RecipesContext> options, bool EmptyMedia, int recipeId, string testMediaPath = "RecipeUpdate_AllOtherTests")
        {
            // Create the schema in the database
            using (var context = new RecipesContext(options))
            {
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


                var medias = new List<Media>();
                if (!EmptyMedia)
                {
                    // Test images are in a non standard path (not <RecipeID>/<MediaID>/image.png) but easier for testing setup         
                    var receivedImagesPath = Path.Combine(this._mediasPath, "RecipeServiceMediaTests", testMediaPath);
                    // loading 3 mediaDtos from json
                    medias = new List<Media>()
                    {
                        new Media{Id = 0, Recipe_Id = recipeId, Title = "Tartines", MediaPath= Path.Combine(receivedImagesPath,"tartines.jpg")},
                        new Media{Id = 0, Recipe_Id = recipeId, Title = "FoodColor", MediaPath= Path.Combine(receivedImagesPath,"foodColor.jpeg")},
                        new Media{Id = 0, Recipe_Id = recipeId, Title = "OrangeJuice", MediaPath= Path.Combine(receivedImagesPath,"RecipeUpdate_UpdatingExistingMediaImage.jpg")} // using image that we are sure won't be used by other tests
                    };
                    this._contextMedia = medias;
                }

                // Adding a Recipe with 2 instructions
                context.Recipes.Add(new Recipe()
                {
                    Id = recipeId,
                    TitleLong = "LongTitle",
                    TitleShort = "ShortTitle",
                    Description = String.Empty,
                    LastModifier = $"testuser@example.com",
                    OriginalLink = "",
                    AuditDate = null,
                    CreationDate = null,
                    Medias = medias
                });
                context.SaveChanges();
            }
        }
    }
}
