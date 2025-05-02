using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sapica_backend.Data.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsSeen { get; set; }

        [ForeignKey(nameof(AdoptionRequest))]
        public int? AdoptionRequestId { get; set; }
        public AdoptionRequest? AdoptionRequest { get; set; }
    }
}
