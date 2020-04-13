using System;
using System.Runtime.Serialization;

namespace RecipesApi.DTOs
{
    [DataContract]
    public class RecipeBaseDto
    {
        [DataMember]
        public int Recipe_Id { get; set; }
        [DataMember]
        public string TitleShort { get; set; }
        [DataMember]
        public string TitleLong { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string OriginalLink { get; set; }
        [DataMember]
        public string LastModifier { get; set; }
        [DataMember]
        public DateTime? AuditDate { get; set; }
        [DataMember]
        public DateTime? CreationDate { get; set; }
    }
}
