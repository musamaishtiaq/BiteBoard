using System.ComponentModel.DataAnnotations;

namespace BiteBoard.API.DTOs.Requests.Identity
{
    public class ApplicationUserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
