using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sapica_backend.Data.Models
{
    public class Favourite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(AdoptionPost))]
        public int AdoptionPostId { get; set; }
        public AdoptionPost? AdoptionPost { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
