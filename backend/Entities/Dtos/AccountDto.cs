using Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Entities.Dtos
{
    public class LoginDto:IDto
    {
        [JsonPropertyName("Username")]
        public string Username { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }
    }

    public class RegisterDto:IDto
    {
        [Required]
        [StringLength(75)]
        [EmailAddress]
        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(1000)]
        [JsonPropertyName("Password")]
        public string Password { get; set; }

        [StringLength(75)]
        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; }

        [StringLength(75)]
        [JsonPropertyName("LastName")]
        public string LastName { get; set; }

        [StringLength(75)]
        [JsonPropertyName("Username")]
        public string Username { get; set; }

        [StringLength(50)]
        [JsonPropertyName("PhoneNumber")]
        public string PhoneNumber { get; set; }

    }
}