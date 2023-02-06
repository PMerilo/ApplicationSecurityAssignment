namespace Spoonful.Services
{
    public interface IEmailService
    {
        bool SendEmail(string ToEmail, string Subject, string HTMLContent, string TextContent, List<IFormFile>? Attachments);
    }
}
