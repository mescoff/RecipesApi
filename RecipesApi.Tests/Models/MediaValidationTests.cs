using NUnit.Framework;
using RecipesApi.DTOs;
using RecipesApi.Tests.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace RecipesApi.Tests.Models
{
    [TestFixture]
    public class MediaDTOValidationTests
    {
        #region MediaDataUrl
        [Test]
        public void MediaDTO_MediaDataUrlIsRequired()
        {
            var mediaDto = new MediaDto { Id = 1, Title = "0", Recipe_Id = 1 };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The MediaDataUrl field is required.", validationError.ErrorMessage);
            Assert.AreEqual("MediaDataUrl", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void MediaDTO_MediaDataUrlNeedsLengthGreaterThan22()
        {
            var mediaDto = new MediaDto { Id = 1, Title = "0", Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64," };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The field MediaDataUrl must be a string or array type with a minimum length of '22'.", validationError.ErrorMessage);
            Assert.AreEqual("MediaDataUrl", validationError.MemberNames.FirstOrDefault());
        }

        [Test]
        public void MediaDTO_MediaDataUrlWithLengthGreaterThan22IsValid()
        {
            var mediaDto = new MediaDto { Id = 1, Title = "0", Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64,/" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);

            Assert.AreEqual(true, actual);
        }
        #endregion

        #region Title
        [Test]
        public void MediaDTO_TitleIsRequired()
        {
            var mediaDto = new MediaDto { Id = 1, Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64,/9ndhjffj" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);
            var validationError = results.FirstOrDefault();

            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Title field is required.", validationError.ErrorMessage);
            Assert.AreEqual("Title", validationError.MemberNames.FirstOrDefault());
        }

        [TestCase("sugar")]
        [TestCase("SUGar")]
        public void MediaDTO_TitleWithOnlyLettersIsValid(string title)
        {
            var mediaDto = new MediaDto { Id = 1, Title = title, Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64,/9ndhjffj" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);

            Assert.AreEqual(true, actual);
        }

        [TestCase("sugar_DADDY")]
        [TestCase("sugar___DADDY_hi")]
        public void MediaDTO_TitleWithUnderscoreIsValid(string title)
        {
            var mediaDto = new MediaDto { Id = 1, Title = title, Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64,/9ndhjffj" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);

            Assert.AreEqual(true, actual);
        }

        [TestCase("1256")]
        [TestCase("1256_hello")]
        public void MediaDTO_TitleWithDigitsIsValid(string title)
        {
            var mediaDto = new MediaDto { Id = 1, Title = title, Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64,/9ndhjffj" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);

            Assert.AreEqual(true, actual);
        }

        [TestCase("12.90")]
        [TestCase(@"abo\dso")]
        [TestCase(@"abo dso")]
        [TestCase(@"abo@dso")]
        [TestCase(@"abo#dso")]
        public void MediaDTO_TitleWithUnauthorizedSymbolsIsInvalid(string title)
        {
            var mediaDto = new MediaDto { Id = 1, Title = title, Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64,/9ndhjffj" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);

            var validationError = results.FirstOrDefault();
            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Media title can only contain MAJ or MIN letters, digits, or _", validationError.ErrorMessage);
            Assert.AreEqual("Title", validationError.MemberNames.FirstOrDefault());
        }
        #endregion

        #region Tag
        [Test]
        public void MediaDTO_TagLengthCannotBeGreaterThan50()
        {
            var rdm = new Random();
            var tag = rdm.GenerateRandomString(51);
            var mediaDto = new MediaDto { Id = 1, Tag = tag, Title = "hi", Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64,/9ndhjffj" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);

            var validationError = results.FirstOrDefault();
            Assert.AreEqual(false, actual);
            Assert.AreEqual("The field Tag must be a string or array type with a maximum length of '50'.", validationError.ErrorMessage);
            Assert.AreEqual("Tag", validationError.MemberNames.FirstOrDefault());
        }

        [TestCase("sugar")]
        [TestCase("SUGar")]
        public void MediaDTO_TagWithOnlyLettersIsValid(string tag)
        {
            var mediaDto = new MediaDto { Id = 1, Tag = tag, Title = "hi", Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64,/9ndhjffj" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);

            Assert.AreEqual(true, actual);
        }

        [TestCase("sugar_DADDY")]
        [TestCase("sugar___DADDY_hi")]
        public void MediaDTO_TagWithUnderscoreIsValid(string tag)
        {
            var mediaDto = new MediaDto { Id = 1, Tag = tag, Title = "hi", Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64,/9ndhjffj" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);

            Assert.AreEqual(true, actual);
        }

        [TestCase("1256")]
        [TestCase("1256_hello")]
        public void MediaDTO_TagWithDigitsIsValid(string tag)
        {
            var mediaDto = new MediaDto { Id = 1, Tag = tag, Title = "hi", Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64,/9ndhjffj" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);

            Assert.AreEqual(true, actual);
        }

        [TestCase("12.90")]
        [TestCase(@"abo\dso")]
        [TestCase(@"abo dso")]
        [TestCase(@"abo@dso")]
        [TestCase(@"abo#dso")]
        public void MediaDTO_TagWithUnauthorizedSymbolsIsInvalid(string tag)
        {
            var mediaDto = new MediaDto { Id = 1, Tag = tag, Title = "hi", Recipe_Id = 1, MediaDataUrl = "data:image/dd;base64,/9ndhjffj" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);

            var validationError = results.FirstOrDefault();
            Assert.AreEqual(false, actual);
            Assert.AreEqual("The Media tag can only contain MAJ or MIN letters, digits, or _", validationError.ErrorMessage);
            Assert.AreEqual("Tag", validationError.MemberNames.FirstOrDefault());
        }
        #endregion

        [Test]
        public void MediaDTO_RecipIdIsRequired()
        {
            var mediaDto = new MediaDto { Id = 1, Title = "hi", MediaDataUrl = "data:image/dd;base64,/9ndhjffj" };
            var context = new ValidationContext(mediaDto, null, null);
            var results = new List<ValidationResult>();
            var actual = Validator.TryValidateObject(mediaDto, context, results, true);

            var validationError = results.FirstOrDefault();
            Assert.AreEqual(false, actual);
            Assert.AreEqual("The field Recipe_Id must be between 1 and 2147483647.", validationError.ErrorMessage);
            Assert.AreEqual("Recipe_Id", validationError.MemberNames.FirstOrDefault());
        }
    }
}
