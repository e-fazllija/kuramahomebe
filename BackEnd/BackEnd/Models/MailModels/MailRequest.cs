namespace BackEnd.Models.MailModels;

public class MailRequest
{
    public string FromEmail { get; set; }
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public List<string>? CcEmail { get; set; } = new List<string>();
    public List<IFormFile>? Attachments { get; set; }
}