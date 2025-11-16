using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeddyBearExport.Model.Doznaka
{
    public class Tablica
    {
        public int Vrsta { get; set; } = 61;
        public int Tarifa { get; set; } = 0;
        public int TarifniNiz { get; set; } = 0;
        public float TrenutnaZapremina { get; set; } = 0f;
        public float TehnikaZapremina { get; set; } = 0f;
        public int SortimentnaTablica { get; set; } = 0;
        public List<DebStepeni>? DebStepeni { get; set; } = new List<DebStepeni>();

        public Tablica() { }
    }
}
