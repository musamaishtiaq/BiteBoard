using FluentEmail.Core;
using BiteBoard.API.DTOs.Mail;
using BiteBoard.API.Services.Interfaces;
using BiteBoard.API.Settings;

namespace BiteBoard.API.Services;

public class MailService : IMailService
{
    private readonly IFluentEmail _fluentEmail;
    private readonly MailSettings _mailSettings;
    private readonly ILogger<MailService> _logger;

    public MailService(IFluentEmail fluentEmail, MailSettings mailSettings, ILogger<MailService> logger)
    {
        _fluentEmail = fluentEmail;
        _mailSettings = mailSettings;
        _logger = logger;
    }

    public async Task SendAsync(MailRequest request)
    {
        try
        {
            request.From = _mailSettings.From;
            var email = _fluentEmail
                .To(request.To)
                .Subject(request.Subject)
                .Body(request.Body, isHtml: true);
            //Attach the file if there's an attachment
            if (!string.IsNullOrEmpty(request.AttachmentPath))
                email.AttachFromFilename(request.AttachmentPath);
            await email.SendAsync();
            _logger.LogInformation("Mail sent successfully to {To} with subject {Subject}", request.To, request.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }
}