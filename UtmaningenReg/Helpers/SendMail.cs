using System;
using System.Configuration;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace UtmaningenReg.Helpers
{
    public class SendMail
    {
        private static SmtpClient SmtpClient
        {
            get
            {
                return new SmtpClient
                         {
                             Host = ConfigurationManager.AppSettings["SmtpServer"],
                             Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["MailUserName"],
                                 ConfigurationManager.AppSettings["MailPassword"]),
                             EnableSsl = true,
                             Port = int.Parse(ConfigurationManager.AppSettings["SmtpServerPort"])
                         };
            }
        }

        public static void SendRegistration(string message, string hostAddress, string link, string toEmailAdress)
        {   
            message = message.Replace("href=\"/", string.Format("href=\"{0}", hostAddress));
            message = message.Replace("src=\"/", string.Format("src=\"{0}", hostAddress));
            message = message.Replace("<html>", string.Format("<html>Ser meddelandet konstigt ut? Öppna följande adress i en webbläsare: {0}<br/><br/>", link));

            var mailDefinition = new MailDefinition
                              {
                                  From = ConfigurationManager.AppSettings["MailSender"],
                                  Subject = ConfigurationManager.AppSettings["MailSubject"],
                                  IsBodyHtml = true
                              };
            // Send to the registrator
            SmtpClient.Send(mailDefinition.CreateMailMessage(toEmailAdress, null, message, new Control()));
            // Send to KMS
            SmtpClient.Send(mailDefinition.CreateMailMessage(ConfigurationManager.AppSettings["MailList"], null, message, new Control()));
        }

        public static void SendErrorMessage(string errorMessage)
        {
#if DEBUG
            return;
#endif
            try
            {
                SmtpClient.Send(ConfigurationManager.AppSettings["MailSender"],
                    ConfigurationManager.AppSettings["MailUserName"],
                    "Fel på anmälningssidan",
                    errorMessage);
            }
            catch (Exception)
            {
            }
        }
    }
}