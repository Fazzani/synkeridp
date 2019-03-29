using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using StsServerIdentity.Models;
using SynkerIdpAdminUI.STS.Identity.Models;
using System.Threading.Tasks;

namespace StsServerIdentity.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IOptions<EmailSettings> _optionsEmailSettings;

        public EmailSender(IOptions<EmailSettings> optionsEmailSettings)
        {
            _optionsEmailSettings = optionsEmailSettings;
        }

        public async Task SendEmail(string email, string subject, string message, string toUsername = null)
        {
            var client = new SendGridClient(_optionsEmailSettings.Value.SendGridApiKey);
            var msg = new SendGridMessage();
            msg.SetFrom(new EmailAddress(_optionsEmailSettings.Value.SenderEmailAddress, "synker authenticator"));
            msg.AddTo(new EmailAddress(email, toUsername));
            msg.SetSubject(subject);
            msg.AddContent(MimeType.Text, message);
            //msg.AddContent(MimeType.Html, message);

            msg.SetReplyTo(new EmailAddress(_optionsEmailSettings.Value.SenderEmailAddress, "synker authenticator"));

            //TODO: Polly
            var response = await client.SendEmailAsync(msg);
        }

        //public async Task SendEmailTemplate(string email, string subject, string message, string toUsername = null)
        //{
        //    var client = new SendGridClient(_optionsEmailSettings.Value.SendGridApiKey);
        //    var msg = new SendGridMessage();
        //    msg.SetFrom(new EmailAddress(_optionsEmailSettings.Value.SenderEmailAddress, "synker authenticator"));
        //    msg.AddTo(new EmailAddress(email, toUsername));
        //    msg.SetSubject(subject);
        //    msg.SetTemplateId("d-46b66e30d388448d955ec0b73630eb21");
        //    msg.SetTemplateData(new
        //    {
        //        header = "Activate Account",
        //        text = "Your almost there. To finish activating your account please click the link below.",
        //        c2a_link = "https://idp.synker.ovh",
        //        c2a_button = "Activate Account"
        //    });
        //    //msg.AddContent(MimeType.Text, message);
        //    //msg.AddContent(MimeType.Html, message);

        //    msg.SetReplyTo(new EmailAddress(_optionsEmailSettings.Value.SenderEmailAddress, "synker authenticator"));

        //    //TODO: Polly
        //    var response = await client.SendEmailAsync(msg);
        //}

        /// <summary>
        /// Send email based on sendgrid template
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="templateId"></param>
        /// <param name="templateData"></param>
        /// <param name="toUsername"></param>
        /// <returns></returns>
        public async Task SendEmailTemplate(string email, string subject, string templateId, object templateData, string toUsername = null)
        {
            var client = new SendGridClient(_optionsEmailSettings.Value.SendGridApiKey);
            var msg = new SendGridMessage();
            msg.SetFrom(new EmailAddress(_optionsEmailSettings.Value.SenderEmailAddress, "synker authenticator"));
            msg.AddTo(new EmailAddress(email, toUsername));
            msg.SetSubject(subject);
            msg.SetTemplateId(templateId);
            msg.SetTemplateData(templateData);
            //msg.AddContent(MimeType.Text, message);
            //msg.AddContent(MimeType.Html, message);

            msg.SetReplyTo(new EmailAddress(_optionsEmailSettings.Value.SenderEmailAddress, "synker authenticator"));

            //TODO: Polly
            var response = await client.SendEmailAsync(msg);
        }
    }
}
