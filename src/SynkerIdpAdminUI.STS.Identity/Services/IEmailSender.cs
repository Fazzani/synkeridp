namespace SynkerIdpAdminUI.STS.Identity.Services
{
    using System.Threading.Tasks;
    public interface IEmailSender
    {
        Task SendEmail(string email, string subject, string message, string toUsername = null);
        Task SendEmailTemplate(string email, string subject, string templateId, object templateData, string toUsername = null);
    }
}
