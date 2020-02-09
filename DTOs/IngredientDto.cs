using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RecipesApi.DTOs
{
    [DataContract]
    public class IngredientDto
    {
        [DataMember]
        public int RecipeIng_Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public double Quantity { get; set; }
        [DataMember]
        public int Recipe_Id { get; set; }
        [DataMember]
        public int Unit_Id { get; set; }

    }
}
