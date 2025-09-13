namespace BiteBoard.API.DTOs.Requests.Account;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}
