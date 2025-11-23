using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeddyBearExport.Model.Woodometer
{
    public class Krug
    {
        public int? IdBroj { get; set; }
        public int BrKruga { get; set; }
        public bool? Permanentna { get; set; }
        public bool? Pristupacnost { get; set; }
        public float Nagib { get; set; }
        public int GazTip { get; set; }
        public int UzgojnaGrupa { get; set; }

        public string Napomena { get; set; }

        public string StartTime {  get; set; }
        public string EndTime { get; set; }

        public int XKoordinata {get; set; }

        public int YKoordinata { get; set; }
        public List<Stablo> Stabla { get; set; }
        public List<MrtvoStablo> MrtvaStabla { get; set; }
        public Biodiverzitet Biodiverzitet { get; set; }

        public Krug()
        {
            Stabla = new List<Stablo>();       // <-- initialize
            MrtvaStabla = new List<MrtvoStablo>(); // <-- initialize
            Biodiverzitet = new Biodiverzitet(); // <-- already initialized
        }
    }
}
