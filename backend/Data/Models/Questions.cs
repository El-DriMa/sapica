using System.ComponentModel.DataAnnotations.Schema;

namespace sapica_backend.Data.Models
{
    public class Questions
    {
        public int Id { get; set; }
        [ForeignKey(nameof(Admin))]
        public int AdminId { get; set; }
        public Admin Admin { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string AnswerText { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }
    }
}
