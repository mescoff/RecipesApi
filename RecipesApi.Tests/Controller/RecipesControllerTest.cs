//using AutoMapper;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NUnit.Framework;
//using RecipesApi.Controllers;
//using RecipesApi.Models;
//using System.Collections.Generic;

//namespace RecipesApi.Tests
//{
//    public class RecipesControllerTest
//    {
//        private Mock<IEntityService<Recipe>> _recipeServicesMock;
//        private Mock<IMapper> _mapperMock;

//        // IN PROGRESS

//        [SetUp]
//        public async void Setup()
//        {
//            var logger = new Mock<ILogger<IEntityService<Recipe>>>();
//            this._recipeServicesMock = new Mock<IEntityService<Recipe>>();
//            this._mapperMock = new Mock<IMapper>();
//            var dbContext = await TestUtils.GetRecipesContext();
//            this._recipeServicesMock.Setup(r => r.GetAll()).Returns(dbContext.Recipes);
//        }


//        public void GetAll_Should_Return_All_Recipes()
//        {
//            var controller = new RecipesController(_recipeServicesMock.Object, _mapperMock.Object);
//            var okResult = controller.GetAll();

//            Assert.IsInstanceOf<Recipe>(okResult);
//            //Assert.IsAssignableFrom<IEnumerable<Recipe>>
//            //Assert.Equals(5,okResult.Count);
//        }
//    }
//}