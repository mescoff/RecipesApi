using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipesApi.Validate
{
    public class ValidateChildren : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            CustomValidationResult result = new CustomValidationResult();
            result.ErrorMessage = string.Format(@"Error occured at {0}", validationContext.DisplayName);

            IEnumerable list = value as IEnumerable;
            if (list == null)
            {
                // Single Object
                List<ValidationResult> results = new List<ValidationResult>();
                Validator.TryValidateObject(value, validationContext, results, true);
                result.NestedResults = results;
                return result;
            }
            else
            {
                List<ValidationResult> recursiveResultList = new List<ValidationResult>();

                // List Object
                foreach (var item in list)
                {
                    List<ValidationResult> nestedItemResult = new List<ValidationResult>();
                    ValidationContext context = new ValidationContext(item);

                    CustomValidationResult nestedParentResult = new CustomValidationResult();
                    nestedParentResult.ErrorMessage = string.Format(@"Error occured at {0}", validationContext.DisplayName);

                    // keep track of all prop validation error in single model
                    Validator.TryValidateObject(item, context, nestedItemResult, true);
                    if (nestedItemResult.Count > 0)
                    {
                        nestedParentResult.NestedResults = nestedItemResult;
                        recursiveResultList.Add(nestedParentResult);
                    }
                }

                if (recursiveResultList.Count > 0)
                {
                    // keep track of all validations error in all objets/models provided
                    result.NestedResults = recursiveResultList;
                    return result;
                }
                return ValidationResult.Success;
                
            }
        }
    }
}
