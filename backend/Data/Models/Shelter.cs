namespace sapica_backend.Data.Models
{
    public class Shelter:UserAccount
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public int YearFounded { get; set; }
        public string Address { get; set; }
        public string ActivationToken { get; set; } = string.Empty; 
        public bool IsActivated { get; set; } = false; 
    }
}
