using System;

namespace UtmaningenReg.Helpers
{
    public class Avgift
    {
        public static int Kalk(Registreringar registrering)
        {
            var avgift = registrering.Banor.Avgift
                + (registrering.Kanoter != null ? registrering.Kanoter.Avgift : 0)
                + registrering.Forseningsavgift
                - registrering.Rabatt;
            return avgift != null ? avgift.Value : 0;
        }

        public static int Forseningsavgift(UtmaningenEntities db)
        {
            var avgift = 0;

            foreach (Forseningsavgift forseningsavgift in db.Forseningsavgift)
            {
                if (DateTime.Now > forseningsavgift.FranDatum)
                {
                    avgift = forseningsavgift.Summa;   
                }
            }
            return avgift;
        }
    }
}