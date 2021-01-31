
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RecipesApi.Utils;
using System.IO;

namespace RecipesApi.Tests.Utils
{
    [TestFixture]
    public class MediaLogicHelperTests
    {
        private Mock<ILogger<MediaLogicHelper>> _logger;
        private Mock<IConfiguration> _configuration;
        private readonly string baseDirectory = @"C:\\Users\\Manon\\Programming\\Apps\\Recipes\\Media";
        private readonly string imagesPath = @"RecipeImages";
        private readonly string user = "2301";

        [SetUp]
        public void Setup()
        {
            this._logger = new Mock<ILogger<MediaLogicHelper>>();
            this._configuration = new Mock<IConfiguration>();
            this._configuration.SetupGet(c => c["MediaPath:BaseDirectory"]).Returns(baseDirectory);         
            this._configuration.SetupGet(c => c["MediaPath:ImagesPath"]).Returns(imagesPath);         
        }

        [Test]
        public void Constructor_FullMediaPathLoadsProperly()
        {
            var helper = new MediaLogicHelper(this._configuration.Object, this._logger.Object);
            var fullPath = Path.Combine(this.baseDirectory, this.user, this.imagesPath);
            Assert.AreEqual(fullPath, helper.FullMediaPath);
        }
        

    }
}
