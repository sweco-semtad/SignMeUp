using System;
using System.ComponentModel.DataAnnotations;

namespace UtmaningenReg
{
    [MetadataType(typeof(RegistreringarMetadata))]
    public partial class Registreringar
    {   
    }

    public class RegistreringarMetadata
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Adress måste anges")]
        public string Adress { get; set; }
        
        [Required(ErrorMessage = "Telefonnummer måste anges")]
        public string Telefon { get; set; }

        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Detta är inte en välformad epostadress")]
        [Required(ErrorMessage = "Epost måste anges")]
        [DataType(DataType.EmailAddress)]
        public string Epost { get; set; }

        public bool Ranking { get; set; }

        public int? Startnummer { get; set; }

        public TrackableCollection<Deltagare> Deltagare { get; set; }

        [Required(ErrorMessage = "Lagnamn måste anges")]
        public string Lagnamn { get; set; }

        [Required(ErrorMessage = "Kanot måste väljas")]
        public int Kanot { get; set; }

        public string Klubb { get; set; }

        [Required(ErrorMessage = "Klass måste väljas")]
        public int Klass { get; set; }

        public bool? HarBetalt { get; set; }

        public int? Forseningsavgift { get; set; }
        
        public DateTime Registreringstid { get; set; }

        public string Kommentar { get; set; }

        [Required(ErrorMessage = "Bana måste väljas")]
        public int Bana { get; set; }
    }

    [MetadataType(typeof(DeltagareMetadata))]
    public partial class Deltagare
    {
    }

    public class DeltagareMetadata
    {
        [Display(Name = "Förnamn")]
        [Required(ErrorMessage = "Måste anges")]
        public string Förnamn { get; set; }

        [Display(Name = "Efternamn")]
        [Required(ErrorMessage = "Måste anges")]
        public string Efternamn { get; set; }

        [Display(Name = "Personnummer")]
        public string Personnummer { get; set; }
    }

    [MetadataType(typeof(InvoiceMetadata))]
    public partial class Invoice
    {
    }

    public class InvoiceMetadata
    {
        [Display(Name = "Företagsnamn")]
        [Required(ErrorMessage = "Måste anges")]
        public string Namn { get; set; }

        [Display(Name = "Märkning")]
        public string Att { get; set; }

        public string Box { get; set; }

        [Required(ErrorMessage = "Måste anges")]
        public string Postnummer { get; set; }

        [Required(ErrorMessage = "Måste anges")]
        public string Organisationsnummer { get; set; }
        
        [Required(ErrorMessage = "Måste anges")]
        public string Postort { get; set; }
        
        [Required(ErrorMessage = "Måste anges")]
        public string Postadress { get; set; }
    }
}