using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeddyBearExport.Model
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
        public int BrOdeljenja { get; set; }
        public string Odsek { get; set; }
        public int GazJedinica { get; set; }
        public List<Stablo> Stabla { get; set; }
        public List<MrtvoStablo> MrtvaStabla { get; set; }
        public Biodiverzitet Biodiverzitet { get; set; }
    }
}
