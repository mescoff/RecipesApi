using RecipesApi.Models;
using System.Runtime.Serialization;

namespace RecipesApi.DTOs
{
    [DataContract]
    public class GetIngredientDto
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public double Quantity { get; set; }
        [DataMember]
        public int Recipe_Id { get; set; }
        [DataMember]
        public int Unit_Id { get; set; } // TODO: Maybe keep Unit_ID ? So that as input, Unit is empty but Unit Id is provided, and as Output, we give both
        [DataMember]
        public Unit Unit { get; set; }

    }
}
