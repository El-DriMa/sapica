using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sapica_backend.Data.Models
{
    public class Donation
    {
        [Key]
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Amount {  get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [ForeignKey(nameof(Shelter))]
        public int ShelterId { get; set; }
        public Shelter? Shelter { get; set; }
    }
}
