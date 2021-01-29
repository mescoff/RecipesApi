
using NUnit.Framework;
using RecipesApi.Models;

namespace RecipesApi.Tests.Models
{
    [TestFixture]
    public class EqualityTests
    {

        [Test]
        public void Ingredient_Equal_OtherIngredients_EvenWithRelatedEntities()
        {
            var ingredient1 = new Ingredient { Id = 5, Name = "Cake", Quantity = 3, Recipe_Id = 1, Unit_Id = 2 };
            var ingredient2 = new Ingredient { Id = 5, Name = "Cake", Quantity = 3, Recipe_Id = 1, Unit_Id = 2 };
            Assert.AreEqual(ingredient1, ingredient2);

            // Ingredient with related entity should still work
            ingredient2 = new Ingredient() { Id = 5, Name = "Cake", Quantity = 3, Recipe_Id = 1, Unit_Id = 2, Unit = new Unit() { Id = 2, Name = "Gram", Symbol = "g" } };
            Assert.AreEqual(ingredient1, ingredient2);
        }

        [Test]
        public void Ingredient_NotEqual_OtherIngredients()
        {
            var ingredient1 = new Ingredient { Id = 5, Name = "Cake", Quantity = 3, Recipe_Id = 1, Unit_Id = 2 };
            var ingredient2 = new Ingredient { Id = 5, Name = "Strawberry", Quantity = 3, Recipe_Id = 1, Unit_Id = 2 };
            Assert.AreNotEqual(ingredient1, ingredient2);

            ingredient2 = new Ingredient() { Id = 5, Name = "Strawberry", Quantity = 3, Recipe_Id = 1, Unit_Id = 2, Unit = new Unit() { Id = 2, Name = "Gram", Symbol = "g" } };
            Assert.AreNotEqual(ingredient1, ingredient2);
        }

        [Test]
        public void Instruction_Equal_OtherIngredients_EvenWithRelatedEntities()
        {
            var instruction1 = new Instruction { Id = 5, Description = "Step1", StepNum = 3, Recipe_Id = 1, RecipeMedia_Id = 2 };
            var instruction2 = new Instruction { Id = 5, Description = "Step1", StepNum = 3, Recipe_Id = 1, RecipeMedia_Id = 2 };
            Assert.AreEqual(instruction1, instruction2);

            // Ingredient with related entity should still work
            instruction2 = new Instruction { Id = 5, Description = "Step1", StepNum = 3, Recipe_Id = 1, RecipeMedia_Id = 2, Media = new Media { Id = 2, MediaPath = @"D:/", Title = "picture", Recipe_Id = 2 } };
            Assert.AreEqual(instruction1, instruction2);
        }

        [Test]
        public void Instruction_NotEqual_OtherIngredients()
        {
            var instruction1 = new Instruction { Id = 5, Description = "Step1", StepNum = 3, Recipe_Id = 1, RecipeMedia_Id = 2 };
            var instruction2 = new Instruction { Id = 5, Description = "Step2", StepNum = 3, Recipe_Id = 1, RecipeMedia_Id = 2 };
            Assert.AreNotEqual(instruction1, instruction2);

            // Ingredient with related entity should still work
            instruction2 = new Instruction { Id = 5, Description = "Step3", StepNum = 3, Recipe_Id = 1, RecipeMedia_Id = 2, Media = new Media { Id = 2, MediaPath = @"D:/", Title = "picture", Recipe_Id = 2 } };
            Assert.AreNotEqual(instruction1, instruction2);
        }

        // TODO: do test for each entity
    }
}
