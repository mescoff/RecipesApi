using NUnit.Framework;
using RecipesApi.Models;
using RecipesApi.Tests.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace RecipesApi.Tests.Models
{
    [TestFixture]
    public class IngredientValidationTests
    {
        [Test]
        public void Ingredient_NameIsRequired()
        {
            var ingredient = new Ingredient { Id = 1, Recipe_Id = 4, Quantity = 2, Unit_Id = 2 };
            var context = new ValidationContext(ingredient, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(ingredient, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Name field is required.", validationError.ErrorMessage);
            Assert.AreEqual("Name", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Ingredient_NameIsRequiredCannotBeLongerThan100char()
        {
            var rdm = new Random();
            var name = rdm.GenerateRandomString(101);
            var ingredient = new Ingredient { Id = 1, Recipe_Id = 4, Quantity = 2, Unit_Id = 2, Name = name };
            var context = new ValidationContext(ingredient, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(ingredient, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("Ingredient name cannot be longer than 100 characters", validationError.ErrorMessage);
            Assert.AreEqual("Name", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Ingredient_QuantityIsRequiredAndShouldBeGreaterThan0()
        {
            var ingredient = new Ingredient { Id = 1, Recipe_Id = 4, Unit_Id = 2, Name = "Carrots" };
            var context = new ValidationContext(ingredient, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(ingredient, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("Please enter a quantity of 0.1 or greater", validationError.ErrorMessage);
            Assert.AreEqual("Quantity", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Ingredient_RecipeIdIsRequired()
        {
            var ingredient = new Ingredient { Id = 1, Quantity = 2, Unit_Id = 2, Name = "Carrots" };
            var context = new ValidationContext(ingredient, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(ingredient, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("Ingredient needs a valid Recipe_Id", validationError.ErrorMessage);
            Assert.AreEqual("Recipe_Id", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Ingredient_UnitIdIsRequired()
        {
            var ingredient = new Ingredient { Id = 1, Quantity = 2, Recipe_Id = 1, Name = "Carrots" };
            var context = new ValidationContext(ingredient, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(ingredient, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("Ingredient needs a valid Unit_Id", validationError.ErrorMessage);
            Assert.AreEqual("Unit_Id", validationError.MemberNames.FirstOrDefault());
        }


        //Please enter a quantity greater than 0
    }
}
