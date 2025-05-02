using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sapica_backend.Data.Models
{
    public class AdoptionRequest
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Reason { get; set; }
        public string LivingSpace { get; set; }

        public string Backyard { get; set; }
        public string? BackyardSize { get; set; }
        public int Age { get; set; }
        public int FamilyMembers { get; set; }
        public string AnyKids { get; set; }
        public int? NumberOfKids { get; set; }
        public string AnyAnimalsBefore { get; set; }
        public string? AnimalsBefore { get; set; }
        public string? Experience { get; set; }
        public int TimeCommitment { get; set; }
        public string PreferredCharacteristic { get; set; }
        public bool IsAccepted { get; set; } = false;



        [Required]
        [ForeignKey(nameof(AdoptionPost))]
        public int AdoptionPostId { get; set; }
        public AdoptionPost? AdoptionPost { get; set; }


        [Required]
        [ForeignKey(nameof(City))]
        public int CityId { get; set; }
        public City? City { get; set; }


        [ForeignKey(nameof(User))]
        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}
