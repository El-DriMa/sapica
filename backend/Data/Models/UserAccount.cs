using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sapica_backend.Data.Models
{
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }
        public bool isAdmin { get; set; } = false;
        public bool isShelter { get; set; } = false;
        public bool isUser { get; set; } = false;
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? ImageUrl { get; set; }
        public string PhoneNumber { get; set; }

        [Required]
        [ForeignKey(nameof(City))]
        public int CityId { get; set; }
        public City? City { get; set; }
    }
}
