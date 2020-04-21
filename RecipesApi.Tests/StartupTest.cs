using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RecipesApi.Controllers;

namespace RecipesApi.Tests
{
    public class StartupTest
    {
        // IN PROGRESS

        private Mock<IConfigurationSection> _configurationSectionStub;
        private Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationStub;

        [SetUp]
        public void Setup()
        {
            //  Arrange
            this._configurationSectionStub = new Mock<IConfigurationSection>();
            this._configurationSectionStub.Setup(x => x["DefaultConnection"]).Returns("TestConnectionString");
            this._configurationStub = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            this._configurationStub.Setup(x => x.GetSection("ConnectionStrings")).Returns(this._configurationSectionStub.Object);    
        }


        // IntegrationTests 
        // Make sure that all required dependencies are registed/loaded for classes that require them
        public void Test_RegistersDependenciesCorrectly()
        {
            //  Arrange
            IServiceCollection services = new ServiceCollection();
            var target = new Startup(this._configurationStub.Object);
            services.AddSingleton<ILogger, NullLogger>();
            
            //Act
            target.ConfigureServices(services);
            //  Mimic internal asp.net core logic.
            services.AddTransient<RecipesController>();
            services.AddTransient<UnitsController>();
            services.AddTransient<IngredientsController>();


            //  Assert
            var serviceProvider = services.BuildServiceProvider();

            var recipeController = serviceProvider.GetService<RecipesController>();
            Assert.IsNotNull(recipeController);
            var unitController = serviceProvider.GetService<UnitsController>();
            Assert.IsNotNull(unitController);
            var ingredController = serviceProvider.GetService<IngredientsController>();
            Assert.IsNotNull(ingredController);
        }

    }
}
