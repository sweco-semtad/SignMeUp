using System.Text.RegularExpressions;

namespace UtmaningenReg.Helpers
{
    public class Validators
    {
        public static bool IsEmailAdress(string sEmail)
        {
            if (sEmail != "")
            {
                var sRegex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
                return sRegex.IsMatch(sEmail) ? true : false;
            }
            return false;
        }
    }
}