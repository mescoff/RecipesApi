
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using RecipesApi.Models;
using RecipesApi.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RecipesApi.Tests.Utils
{
    [TestFixture]
    public class MediaLogicHelperTests
    {
        private Mock<ILogger<MediaLogicHelper>> _loggerMock;
        private Mock<IConfiguration> _configuration;
        private readonly string receivedDataDirectory = @"Received";
        private readonly string imagesDirectory = @"RecipeImages";
        private readonly long MAX_IMAGESIZE = 800000;
        //private readonly string userId = "2301";
        private string workingDirectory;
        private string receivedImagePath;
        private string mediasPath;

        [SetUp]
        public void Setup()
        {
            this._loggerMock = new Mock<ILogger<MediaLogicHelper>>();

            this.workingDirectory = Environment.CurrentDirectory;
            this.mediasPath = Path.Combine(workingDirectory, "TestData");

            this._configuration = new Mock<IConfiguration>();
            this._configuration.SetupGet(c => c["MediasSettings:BasePath"]).Returns(mediasPath);
            this._configuration.SetupGet(c => c["MediasSettings:ImagesDirectory"]).Returns(imagesDirectory);
            this._configuration.SetupGet(c => c["MediasSettings:MaxImageSize"]).Returns(Convert.ToString(MAX_IMAGESIZE));
            // test image          
            this.receivedImagePath = Path.Combine(this.mediasPath, receivedDataDirectory, "foodMarket.jpg");
        }

        [Test]
        public void Constructor_FullMediaPathLoadsProperly()
        {
            var helper = new MediaLogicHelper(this._configuration.Object, this._loggerMock.Object);
            // for now ID matches because its hardcoded in Helper constructor and we use same
            var fullPath = Path.Combine(this.mediasPath, Convert.ToString(helper.CurrentUserId), this.imagesDirectory);
            Assert.AreEqual(fullPath, helper.UserMediasPath);
        }

        [Test]
        public void LocateAndLoadMedias_LoadsExistingMediaProperly()
        {
            var media = new Media { MediaPath = this.receivedImagePath };
            var helper = new MediaLogicHelper(this._configuration.Object, this._loggerMock.Object);
            var mediaDtos = helper.LocateAndLoadMedias(new List<Media>() { media });
            Assert.IsNotNull(mediaDtos);
            var mediaDto = mediaDtos.FirstOrDefault();
            Assert.IsNotEmpty(mediaDto.MediaDataUrl);
        }

        [Test]
        public void LocateAndLoadMedias_ReturnsEmptyIfNoMediaProvided()
        {
            using (var helper = new MediaLogicHelper(this._configuration.Object, this._loggerMock.Object))
            {
                var mediaDtos = helper.LocateAndLoadMedias(new List<Media>()); // empty medias list
                Assert.IsEmpty(mediaDtos);
            }
        }


        [Test]
        public void LocateAndLoadMedias_ReturnsEmptyIfProvidedSingleMediaThatDoesntExistAndLogsError()
        {
            var errorQ = new Queue<string>();
            // track errors logged
            //this._logger.Setup(l => l.LogError(It.IsAny<string>())).Callback((string error) => errorQ.Enqueue(error));

            // providing wrong/fake path
            var testImagePath = Path.Combine(this.workingDirectory, "WrongFolder", "food.jpeg");
            var media = new Media { MediaPath = testImagePath };
            using (var helper = new MediaLogicHelper(this._configuration.Object, this._loggerMock.Object))
            {
                var mediaDtos = helper.LocateAndLoadMedias(new List<Media>() { media });
                this._loggerMock.Verify(l =>
                    l.Log(LogLevel.Warning, 0, It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())
                    , Times.Once);
            }
        }


        [TestCase("foodMarket.jpg", 54)]
        [TestCase("tartines.jpg", 6)]
        [TestCase("foodColor.jpeg", 96)]
        [TestCase("foodMarketPng.png", 4)]
        public void SaveImageLocally_CanSaveOneImage(string imageName, int mediaId)
        {
            var recipeId = 52;
            var receivedImgPath = Path.Combine(this.mediasPath, receivedDataDirectory, imageName);
            // STEP1 - Create Media to load with image path
            var media = new Media { MediaPath = receivedImgPath, Id = mediaId, Title = "Some media", Recipe_Id = recipeId };
            using (var helper = new MediaLogicHelper(this._configuration.Object, this._loggerMock.Object))
            {
                // STEP2 - Load Image from disk
                // loading image as MediaDTO (to get img bytes)
                var mediaDtos = helper.LocateAndLoadMedias(new List<Media>() { media });
                // STEP3 - Save it again
                ServiceResponse savingResult;
                var medias = helper.SaveImagesLocally(mediaDtos, out savingResult);

                // VERIFY
                Assert.IsTrue(savingResult.Success);
                string userRecipePath;
                var savedImgPath = MediaLogicHelper.GenerateSingleMediaPath(helper.UserMediasPath, recipeId, mediaId, out userRecipePath);
                // check file exists
                Assert.IsTrue(File.Exists(savedImgPath));
                Assert.AreEqual(medias.First(m => m.Id == media.Id).MediaPath, savedImgPath);
                long savedImageSize = new FileInfo(savedImgPath).Length;
                // verify size is under 800kb
                Assert.IsTrue(savedImageSize <= this.MAX_IMAGESIZE);
            }
        }

        [Test]
        public void SaveImageLocally_CanSaveMultipleImages()
        {
            var recipeId = 53;
            var receivedImages = new List<(string File, int MediaId)> { ("foodMarket.jpg", 1), ("foodColor.jpeg", 2), ("foodMarketPng.png", 3) };

            // STEP1 - Create list of Media to load with image path
            var medias = new List<Media>();
            foreach (var i in receivedImages)
            {
                var receivedImgPath = Path.Combine(this.mediasPath, receivedDataDirectory, i.File);
                medias.Add(new Media { MediaPath = receivedImgPath, Id = i.MediaId, Title = "Some media", Recipe_Id = recipeId });
            }
            using (var helper = new MediaLogicHelper(this._configuration.Object, this._loggerMock.Object))
            {
                // STEP2 - Load all Images from disk
                // loading all images as MediaDTO (to get img bytes)
                var mediaDtos = helper.LocateAndLoadMedias(medias);

                // STEP3 - Save it again
                ServiceResponse savingResult;
                var resultMedias = helper.SaveImagesLocally(mediaDtos, out savingResult);
                Assert.IsTrue(savingResult.Success);

                // VERIFY
                string userRecipePath; // not needed so we override
                foreach (var i in receivedImages)
                {
                    var savedImgPath = MediaLogicHelper.GenerateSingleMediaPath(helper.UserMediasPath, recipeId, i.MediaId, out userRecipePath);
                    // check file exists
                    Assert.IsTrue(File.Exists(savedImgPath));
                    Assert.AreEqual(resultMedias.First(m => m.Id == i.MediaId).MediaPath, savedImgPath);

                    long savedImageSize = new FileInfo(savedImgPath).Length;
                    // verify size is under 800kb
                    Assert.Less(savedImageSize, this.MAX_IMAGESIZE);
                }
            }

        }
    }
}
