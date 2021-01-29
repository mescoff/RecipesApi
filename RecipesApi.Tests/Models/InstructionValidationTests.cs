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
    public class InstructionValidationTests
    {

        [Test]
        public void Instruction_StepNumIsRequiredAndHasToBeGreaterThan1()
        {
            var instruction = new Instruction { Id = 1, Recipe_Id = 4, Description = "Something" };
            var context = new ValidationContext(instruction, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(instruction, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("Please enter a value bigger than 0", validationError.ErrorMessage);
            Assert.AreEqual("StepNum", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Instruction_DescriptionIsRequired()
        {
            var instruction = new Instruction { Id = 1, Recipe_Id = 4, StepNum = 6 };
            var context = new ValidationContext(instruction, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(instruction, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Description field is required.", validationError.ErrorMessage);
            Assert.AreEqual("Description", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Instruction_DescriptionCannotBeLongerThan500char()
        {
            var rdm = new Random();
            var description = rdm.GenerateRandomString(501);
            var instruction = new Instruction { Id = 1, Recipe_Id = 4, StepNum = 6, Description = description };
            var context = new ValidationContext(instruction, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(instruction, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("Description cannot be longer than 500 characters", validationError.ErrorMessage);
            Assert.AreEqual("Description", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Instruction_RecipeIdIsRequired()
        {
            var instruction = new Instruction { Id = 1, StepNum = 3, Description = "Something" };
            var context = new ValidationContext(instruction, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(instruction, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("Instruction needs a valid Recipe_Id", validationError.ErrorMessage);
            Assert.AreEqual("Recipe_Id", validationError.MemberNames.FirstOrDefault());
        }
    }
}
