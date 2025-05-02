using System.ComponentModel.DataAnnotations;

namespace sapica_backend.Data.Models
{
    public class Animal
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Gender { get; set; }
        public string Size { get; set; }
        public string Age { get; set; }
        public string Color { get; set; }
        public double Weight { get; set; }
        public string AnimalType { get; set; }

        public bool Vaccinated { get; set; } = false;
        public bool Sterilized { get; set; } = false;
        public bool ParasiteFree { get; set; } = false;
        public bool HasPassport { get; set; } = false;
        public ICollection<AnimalImage> Images { get; set; } = new List<AnimalImage>();

    }
}
