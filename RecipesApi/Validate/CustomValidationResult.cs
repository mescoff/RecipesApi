using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi.Validate
{
    public class CustomValidationResult: ValidationResult
    {
        public CustomValidationResult() : base("")
        {

        }
        public IList<ValidationResult> NestedResults { get; set; }
    }
}
