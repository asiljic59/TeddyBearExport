using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeddyBearExport.Model.Doznaka
{
    public class Stablo
    {
        public int Rbr { get; set; } = 1;
        public float Precnik { get; set; } = 0f;
        public int Vrsta { get; set; } = 61;

        public float Tehnika { get; set; } = 0f;

        public float Zapremina { get; set; } = 0f;
        public DateTime RadniDan { get; set; } = DateTime.Now;

        public Stablo() { }

        // Copy constructor
        public Stablo(Stablo stablo)
        {
            if (stablo == null) return;

            this.Rbr = stablo.Rbr;
            this.Precnik = stablo.Precnik;
            this.Vrsta = stablo.Vrsta;
            this.RadniDan = stablo.RadniDan;
        }
    }
}
