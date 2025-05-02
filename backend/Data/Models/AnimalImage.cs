using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sapica_backend.Data.Models
{
    public class AnimalImage
    {
        [Key]
        public int Id { get; set; }
        public byte[] Image { get; set; }

        [Required]
        [ForeignKey(nameof(Animal))]
        public int AnimalId { get; set; }
    }
}
