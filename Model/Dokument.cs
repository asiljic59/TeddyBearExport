using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeddyBearExport.Model
{
    public class Dokument
    {
        public int BrOdeljenja { get; set; } = 0;
        public string Odsek { get; set; } = "";
        public int GazJedinica { get; set; } = 0;
        public List<Krug> Krugovi { get; set; } = new List<Krug>();

        public Dokument()
        {
        }
    }
}
