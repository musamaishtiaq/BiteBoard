using Microsoft.Extensions.Primitives;
using BiteBoard.Data.Enums;

namespace BiteBoard.API.DTOs.Requests.Account;

public class LoginResponse
{
    public string Token { get; set; }
    public Roles Role { get; set; }
    public string Tenant { get; set; }
}
