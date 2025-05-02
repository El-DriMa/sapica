namespace sapica_backend.Data.Models
{
    public class User:UserAccount
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int YearBorn { get; set; }
        public string ActivationToken { get; set; } = string.Empty;
        public bool IsActivated { get; set; } = false;
    }
}
