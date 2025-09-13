using BiteBoard.API.DTOs.Mail;

namespace BiteBoard.API.Services.Interfaces;

public interface IMailService
{
    Task SendAsync(MailRequest request);
}