using System.Runtime.Serialization;

namespace RecipesApi.DTOs
{
    [DataContract]
    public class MediaDto
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string MediaPath { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Tag { get; set; }

        [DataMember]
        public int Recipe_Id { get; set; }
    }
}
