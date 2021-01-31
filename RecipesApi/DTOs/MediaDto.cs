using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace RecipesApi.DTOs
{
    [DataContract]
    public class MediaDto
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        [Required]
        public byte[] MediaBytes { get; set; }

        [DataMember]
        [Required]
        public string Title { get; set; }

        [DataMember]
        public string Tag { get; set; }

        [DataMember]
        public int Recipe_Id { get; set; }
    }
}
