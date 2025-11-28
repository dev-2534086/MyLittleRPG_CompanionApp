using System.ComponentModel.DataAnnotations;

namespace API_Pokemon.Models
{
    public class User
    {
        [Key]
        public int UserId {  get; set; }
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public bool IsConnected { get; set; }

        public User() 
        {
        
        }

        public User(string userName, string email, string password, bool isConnected)
        {
            UserName = userName;
            Email = email;
            Password = password;
            IsConnected = isConnected;

        }
    }
}
