namespace CurrencyConverterApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }      // ← new
        public bool IsActive { get; set; } = true;
    }
}
