using BiteBoard.Data.Enums;

namespace BiteBoard.API.DTOs.Requests.Account;

public class RegisterRequest : LoginRequest
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string ConfirmEmailUrl { get; set; }
}