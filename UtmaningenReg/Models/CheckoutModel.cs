using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace UtmaningenReg.Models
{
    public class CheckoutModel
    {
        public Registreringar Registrering { get; set; }

        public string ForwardUrl { get; set; }

        public string Token { get; set; }

        public int RegId { get; set; }

        [Display(Name = "Förnamn")]
        [Required(ErrorMessage = "Måste anges")]
        public string SenderFirstName { get; set; }

        [Display(Name = "Efternamn")]
        [Required(ErrorMessage = "Måste anges")]
        public string SenderLastName { get; set; }

        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Detta är inte en välformad epostadress")]
        [Required(ErrorMessage = "Måste anges")]
        [Display(Name = "Epost")]
        [DataType(DataType.EmailAddress)]
        public string SenderEmail { get; set; }

        public static string PaysonUserId
        {
            get { return ConfigurationManager.AppSettings["PAYSON-SECURITY-USERID"] ?? "2";}
        }

        public static string PaysonUserKey
        {
            get
            {
                return ConfigurationManager.AppSettings["PAYSON-SECURITY-PASSWORD"] ??
                       "2acab30d-fe50-426f-90d7-8c60a7eb31d4";
            }
        }

        public static string PaysonRecieverEmail
        {
            get { return ConfigurationManager.AppSettings["Receiver.Email"] ?? "testagent-1@payson.se"; }
        }

        public static string PaysonRecieverFirstName
        {
            get { return "Karlstad"; }
        }

        public static string PaysonRecieverLastName
        {
            get { return "Multisport"; }
        }
    }
}