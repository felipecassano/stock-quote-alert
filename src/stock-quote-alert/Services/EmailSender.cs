using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using StockQuoteAlert.Config;
using StockQuoteAlert.Interfaces;

namespace StockQuoteAlert.Services;


public class EmailSender : IEmailSender
{

    private readonly EmailSettings _emailSettings;
    public EmailSender(IOptions<EmailSettings> options)
    {
        _emailSettings = options.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message, bool isHtml = true)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        emailMessage.To.Add(new MailboxAddress("", toEmail));

        emailMessage.Subject = subject;
        if (isHtml)
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message };
        else
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message };

        using (var client = new SmtpClient())
            try
            {
                // usar o nome de usuário caso seja necessário. Se for pra usar o Email, deixar o nome de usuário vazio em appsettings.json
                var user = string.IsNullOrWhiteSpace(_emailSettings.Username)
      ? _emailSettings.FromEmail
      : _emailSettings.Username;

                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(user, _emailSettings.Password);
                await client.SendAsync(new SendRequest(emailMessage));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Erro ao enviar e-mail.", ex);
            }
            finally
            {
                await client.DisconnectAsync(true);

            }
    }
}

