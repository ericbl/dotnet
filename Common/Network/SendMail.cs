using Common.Helper;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Common.Network
{
    /// <summary>
    /// Wrapper of the <seealso cref="MailMessage"/> to send an Email via SMTP. Can use either a local or remote SMTP server.
    /// </summary>
    public class Email
    {
        private readonly NetworkCredential smtpCredential;
        private readonly string mailSender;
        private readonly string smtpHost;
        private readonly bool useSSL;
        private readonly string subject;
        private readonly string body;
        private readonly bool isHtml;
        private readonly string mailTo;
        private readonly string filename;

        /// <summary>
        /// Initializes a new instance of the <see cref="Email" /> class to be send from the local host (given it is configured as SMTP server, without authentication nor SSL)
        /// </summary>
        /// <param name="mailSender">The mail sender.</param>
        /// <param name="recipients">The addresses.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body, sent as text.</param>
        /// <param name="filename">The filename.</param>
        public Email(string mailSender, string recipients, string subject, string body, string filename)
            : this(null, false, null, recipients, subject, body, false, filename)
        {
            this.mailSender = mailSender;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Email" /> class to prepare Email with given SMTP parameters.
        /// </summary>
        /// <param name="smtpHost">The SMTP host.</param>
        /// <param name="useSSL">if set to <c>true</c> use SSL.</param>
        /// <param name="smtpLogin">The SMTP login.</param>
        /// <param name="recipients">The addresses. Multiple addresses must be separated by a comma character ', '.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="html">if set to <c>true</c> set the body as HTML.</param>
        /// <param name="filename">The filename.</param>
        public Email(string smtpHost, bool useSSL, NetworkCredential smtpLogin, string recipients, string subject, string body, bool html, string filename)
        {
            this.smtpCredential = smtpLogin;
            if (smtpLogin != null)
                this.mailSender = smtpLogin.UserName;
            this.smtpHost = !string.IsNullOrEmpty(smtpHost) ? smtpHost : "127.0.0.1";
            this.useSSL = useSSL;
            this.subject = subject;
            this.body = body;
            this.isHtml = html;
            this.mailTo = recipients;
            this.filename = filename;
        }

        /// <summary>
        /// Run a new task to send Email.
        /// </summary>
        /// <returns>The task</returns>
        public Task SendTask()
        {
            Task t = new Task(SendInTryCatch);
            t.Start();
            return t;
        }

        /// <summary>
        /// Sends the email in a try/catch.
        /// </summary>
        public void SendInTryCatch()
        {
            Exceptions.Utils.TryCatchMethod(SendEmail, null);
        }

        private void SendEmail()
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(mailSender);
                mail.To.Add(mailTo);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = isHtml;
                if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
                    mail.Attachments.Add(new Attachment(filename));

                using (SmtpClient client = new SmtpClient())
                {
                    if (smtpCredential == null || smtpCredential.SecurePassword.Length == 0)
                    {
                        client.UseDefaultCredentials = true;
                    }
                    else
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = smtpCredential;
                    }
                    client.Host = smtpHost;
                    client.EnableSsl = useSSL;
                    client.Port = useSSL ? 587 : 25;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Send(mail);
                }
            }
        }
    }
}
