public class EmailSettings
{

    public string? SmtpServer
    {
        get; set;
    }
    public int SmtpPort { get; set; }

    public string? FromName { set; get; }
    public string? FromEmail { set; get; }

    public string? Password { get; set; }
}