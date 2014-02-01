using System.Collections.Generic;
using System.Linq;

namespace UtmaningenReg.Models
{
    public class RegIndexContainer
    {
        public Dictionary<string, BanaData> Banor { get; set; }

        public int TotaltRegistreringar {
            get { return Banor == null ? 0 : Banor.Values.Sum(banaData => banaData.TotaltAntalBetalda); }
        }
    }

    public class BanaData
    {
        public List<Registreringar> Registreringar { get; set; }

        public int TotaltAntal
        {
            get { return Registreringar.Count; }
        }

        public int TotaltAntalBetalda
        {
            get { return Registreringar.Count(regg => regg.HarBetalt || regg.Invoice != null); }
        }

        public Dictionary<string, int> AntalPerKlass
        {
            get {
                var klassDict = new Dictionary<string, int>();
                foreach (var reg in Registreringar)
                {
                    if (!klassDict.ContainsKey(reg.Klasser.Namn))
                    {
                        klassDict.Add(reg.Klasser.Namn, 1);
                    }
                    else
                    {
                        klassDict[reg.Klasser.Namn]++;
                    }
                }
                return klassDict;
            }
        }

        public Dictionary<string, int> AntalPerKlassBetalda
        {
            get
            {
                var klassDict = new Dictionary<string, int>();
                foreach (var reg in Registreringar.Where(reg => reg.HarBetalt || reg.Invoice != null))
                {
                    if (!klassDict.ContainsKey(reg.Klasser.Namn))
                    {
                        klassDict.Add(reg.Klasser.Namn, 1);
                    }
                    else
                    {
                        klassDict[reg.Klasser.Namn]++;
                    }
                }
                return klassDict;
            }
        }

        public Dictionary<string, int> CountKanot
        {
            get
            {
                var kanoterDict = new Dictionary<string, int>();
                foreach (Registreringar reg in Registreringar)
                {
                    if (!kanoterDict.ContainsKey(reg.Kanoter.Namn))
                    {
                        kanoterDict.Add(reg.Kanoter.Namn, 1);
                    }
                    else
                    {
                        kanoterDict[reg.Kanoter.Namn]++;
                    }
                }
                return kanoterDict;
            }
        }
        
    }
}