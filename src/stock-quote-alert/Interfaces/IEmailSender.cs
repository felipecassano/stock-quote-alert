namespace StockQuoteAlert.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string message, bool isHtml);
}
