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
    public class RecipeValidationTests
    {
        #region ShortTitle
        [Test]
        public void Recipe_ShortTitleWithMoreThan50Characters_ReturnsValidationException()
        {
            var rdm = new Random();
            var shortTitle = rdm.GenerateRandomString(51);

            var recipe = CreateValidRecipe();
            recipe.TitleShort = shortTitle;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Recipe's short Title length needs to be between 5 and 50 characters", validationError.ErrorMessage);
            Assert.AreEqual("TitleShort", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Recipe_ShortTitleWithLessThan5Characters_ReturnsValidationException()
        {
            var rdm = new Random();
            var shortTitle = rdm.GenerateRandomString(4);

            var recipe = CreateValidRecipe();
            recipe.TitleShort = shortTitle;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Recipe's short Title length needs to be between 5 and 50 characters", validationError.ErrorMessage);
            Assert.AreEqual("TitleShort", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Recipe_ShortTitleIsRequired()
        {
            var recipe = CreateValidRecipe();
            recipe.TitleShort = null;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The TitleShort field is required.", validationError.ErrorMessage);
            Assert.AreEqual("TitleShort", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Recipe_ShortTitleInCharRange_IsValid()
        {
            var rdm = new Random();
            var shortTitle = rdm.GenerateRandomString(10);

            var recipe = CreateValidRecipe();
            recipe.TitleShort = shortTitle;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            Assert.AreEqual(true, actual);
        }
        #endregion

        #region Long Title
        [Test]
        public void Recipe_TitleLongWithMoreThan150Characters_ReturnsValidationException()
        {
            var rdm = new Random();
            var longTitle = rdm.GenerateRandomString(151);

            var recipe = CreateValidRecipe();
            recipe.TitleLong = longTitle;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Recipe's long Title length cannot exceed 150 characters", validationError.ErrorMessage);
            Assert.AreEqual("TitleLong", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Recipe_LongTitleInCharRange_IsValid()
        {
            var rdm = new Random();
            var longTitle = rdm.GenerateRandomString(10);

            var recipe = CreateValidRecipe();
            recipe.TitleLong = longTitle;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            Assert.AreEqual(true, actual);
        }
        #endregion

        #region Description
        [Test]
        public void Recipe_DescriptionIsRequired()
        {
            var recipe = CreateValidRecipe();
            recipe.Description = null;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Description field is required.", validationError.ErrorMessage);
            Assert.AreEqual("Description", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Recipe_DescriptionWithMoreThan2000Characters_ReturnsValidationException()
        {
            var rdm = new Random();
            var description = rdm.GenerateRandomString(2001);

            var recipe = CreateValidRecipe();
            recipe.Description = description;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Recipe's description length cannot exceed 2000 characters", validationError.ErrorMessage);
            Assert.AreEqual("Description", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Recipe_DescriptionInCharRange_IsValid()
        {
            var rdm = new Random();
            var description = rdm.GenerateRandomString(10);

            var recipe = CreateValidRecipe();
            recipe.Description = description;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            Assert.AreEqual(true, actual);
        }
        #endregion

        #region OriginalLink

        [Test]
        public void Recipe_LinkWithoutSubdomain_ReturnsValidationException()
        {
            var url = "something.com";
            var recipe = CreateValidRecipe();
            recipe.OriginalLink = url;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("Link provided is not a valid URL", validationError.ErrorMessage);
            Assert.AreEqual("OriginalLink", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Recipe_LinkWithScheme_IsValid()
        {
            // Scheme: http:// or https://
            var url = @"https://docs.microsoft.com/"; ;
            var recipe = CreateValidRecipe();
            recipe.OriginalLink = url;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);

            Assert.AreEqual(true, actual);
        }

        [Test]
        public void Recipe_LinkWithPath_IsValid()
        {
            var url =   @"https://docs.microsoft.com/en-us/dotnet/api/"; ;
            var recipe = CreateValidRecipe();
            recipe.OriginalLink = url;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);

            Assert.AreEqual(true, actual);
        }

        [Test]
        public void Recipe_LinkWithQueryString_IsValid()
        {
            var url = @"https://docs.microsoft.com/en-us/dotnet/api/system.uri.tostring?view=net-5.0"; ;
            var recipe = CreateValidRecipe();
            recipe.OriginalLink = url;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);

            Assert.AreEqual(true, actual);
        }

        [Test]
        public void Recipe_LinkWithMoreThan500Characters_ReturnsValidationException()
        {
            var url = @"https://docs.microsoft.com/en-us/dotnet/api/system.uri.tostring?"; ;
            var rdm = new Random();
            var request = rdm.GenerateRandomString(501-url.Length);
            // Concat to make a url that still has valid characters but is longer than 150 
            url += request;
            var recipe = CreateValidRecipe();
            recipe.OriginalLink = url;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);

            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Recipe's original link length cannot exceed 500 characters", validationError.ErrorMessage);
            Assert.AreEqual("OriginalLink", validationError.MemberNames.FirstOrDefault());
            Assert.AreEqual(false, actual);
        }
        #endregion

        #region Last Modifier
        [Test]
        public void Recipe_LastModifierIsRequired()
        {
            var recipe = CreateValidRecipe();
            recipe.LastModifier = null;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The LastModifier field is required.", validationError.ErrorMessage);
            Assert.AreEqual("LastModifier", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Recipe_LastModifierWithMoreThan500Characters_ReturnsValidationException()
        {
            var rdm = new Random();
            var lastModifier = rdm.GenerateRandomString(501);

            var recipe = CreateValidRecipe();
            recipe.LastModifier = lastModifier;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Recipe's last modifier length cannot exceed 2000 characters", validationError.ErrorMessage);
            Assert.AreEqual("LastModifier", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void Recipe_LastModifierInCharRange_IsValid()
        {
            var lastModifier = "X.yui@gmail.com";

            var recipe = CreateValidRecipe();
            recipe.LastModifier = lastModifier;

            var context = new ValidationContext(recipe, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(recipe, context, results, true);
            Assert.AreEqual(true, actual);
        }
        #endregion

        private Recipe CreateValidRecipe()
        {
            return new Recipe
            {
                Id = 0,
                Description = "Some Recipe",
                TitleShort = "Recipe",
                LastModifier = "manon"
            };
        }
    }
}
