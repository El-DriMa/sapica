using System.ComponentModel.DataAnnotations.Schema;

namespace sapica_backend.Data.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        [ForeignKey(nameof(Shelter))]
        public int ShelterId { get; set; }
        public Shelter Shelter { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
    }
}
