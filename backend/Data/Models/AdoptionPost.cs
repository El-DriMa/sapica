using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sapica_backend.Data.Models
{
    public class AdoptionPost
    {
        [Key]
        public int Id { get; set; }
        public DateOnly DateOfCreation { get; set; }
        public DateOnly? DateOfModification { get; set; }
        public int ViewCounter { get; set; } = 0;
        public bool Urgent { get; set; } = false;
        public string ShortDescription { get; set; }
        public bool IsAdopted { get; set; } = false;
        public DateTime DateOfAdoption { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [ForeignKey(nameof(Animal))]
        public int AnimalId { get; set; }
        public Animal? Animal { get; set; }

        [Required]
        [ForeignKey(nameof(City))]
        public int? CityId { get; set; }
        public City? City { get; set; }
    }
}
