using System;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Framework.Localization;
using log4net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;

namespace Niteco.Common.Mail
{
    public class EmailService : IEmailService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(EmailService));
        private static SmtpSection smtpsettings = (SmtpSection)System.Web.Configuration.WebConfigurationManager.GetSection("system.net/mailSettings/smtp");
        private static readonly string EmailFrom = smtpsettings.From != null ? smtpsettings.From : "";
        private static readonly SmtpDeliveryMethod EmailDeliveryMethod = smtpsettings.DeliveryMethod;
        private static readonly string EmailHost = smtpsettings.Network.Host != null ? smtpsettings.Network.Host : "";
        private static readonly int EmailPort = smtpsettings.Network.Port;
        private static readonly bool EmailDefaultCredentials = smtpsettings.Network.DefaultCredentials;
        private static readonly string EmailUserName = smtpsettings.Network.UserName != null ? smtpsettings.Network.UserName : "";
        private static readonly string EmailPassword = smtpsettings.Network.Password != null ? smtpsettings.Network.Password : "";

        public void SendEmail(string recipients, string fromField, string subject, string body, bool isAsync)
        {

            try
            {
                //create an instance of the SMTP transport mechanism
                SmtpClient client = new SmtpClient();
                client.Host = EmailHost;
                client.Port = EmailPort;
                client.UseDefaultCredentials = EmailDefaultCredentials;
                client.DeliveryMethod = EmailDeliveryMethod;
                client.Credentials = new NetworkCredential(EmailUserName, EmailPassword);
                client.EnableSsl = true;
                //create a new message object
                logger.Debug("Email configuration sent from" + EmailUserName);

                var message = new MailMessage();
                //set the message recipients
                if (string.IsNullOrEmpty(recipients))
                    return;
                var mailTos = recipients.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string recipient in mailTos)
                {
                    message.To.Add(recipient);
                }
                var fromName = string.IsNullOrEmpty(fromField) ? LocalizationService.Current.GetString("/niteco/emailservice/fromname") : fromField;
                //set the sender
                message.From = new MailAddress(EmailFrom, fromName);

                //set the message body
                message.Body = body;
                message.IsBodyHtml = true;

                //set the message subject
                message.Subject = subject;

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                logger.Debug("Email started sending to" + recipients + "with content" + message.Body);

                //send the mail
                if (!isAsync)
                    client.Send(message);
                else
                    client.SendMailAsync(message);

            }
            catch (System.Exception ex)
            {
                logger.Error("Email send failed.", ex);
                throw ex;
                //ex.QueueLog(" Email Grid sent fail to  " + recipients);
            }
        }
        //public string Encrypt(byte[] data)
        //{
        //    System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
        //    data = x.ComputeHash(data);
        //    String md5Hash = System.Text.Encoding.ASCII.GetString(data);
        //    return md5Hash;
        //}
        public string Encrypt(byte[] data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text
            md5.ComputeHash(data);

            //get hash result after compute it
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits
                //for each byte
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        public async Task SendSendgridEmailAsync(string recipientEmail, string fromEmail, string fromName, string subject,
            string body, string apiKey)
        {
            HttpClient httpClient = null;
            HttpRequestMessage httpRequestMessage = null;
            HttpResponseMessage httpResponseMessage = null;
            try
            {
                httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://api.sendgrid.com"),
                    Timeout = new TimeSpan(0, 0, 30)
                };
                httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/mail.send.json");
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", apiKey);
                var keyValuePairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("from",fromEmail),
                    new KeyValuePair<string, string>("fromname",fromName),
                    new KeyValuePair<string, string>("html", body),
                    new KeyValuePair<string, string>("subject", subject)
                };
                var recipients = recipientEmail.Split(';');
                keyValuePairs.AddRange(recipients.Select(recipient => new KeyValuePair<string, string>("to[]", recipient)));
                httpRequestMessage.Content = new FormUrlEncodedContent(keyValuePairs);
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return;
                }
                throw new Exception(await httpResponseMessage.Content.ReadAsStringAsync());
            }
            catch (Exception exception)
            {
                logger.Error(exception);
            }
            finally
            {
                if (httpClient != null)
                {
                    httpClient.Dispose();
                }
                if (httpRequestMessage != null)
                {
                    httpRequestMessage.Dispose();
                }
                if (httpResponseMessage != null)
                {
                    httpResponseMessage.Dispose();
                }
            }
        }
    }
}
