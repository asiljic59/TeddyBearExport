using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeddyBearExport.Model.Doznaka
{
    public class DokumentDoznaka
    {
        public int BrOdeljenja { get; set; } = 0;
        public string Odsek { get; set; } = string.Empty;
        public int GazJedinica { get; set; } = 0;
        public TipDoznake? TipDoznake { get; set; } = null;
        public int BrojDoznake { get; set; } = 0;
        public string Doznacar { get; set; } = string.Empty;
        public float PovrsinaDoznake { get; set; } = 0f;
        public int VrstaPrinosa { get; set; } = 0;
        public int VrstaSece { get; set; } = 0;
        public bool IsFinished { get; set; } = false;
        public long StartTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public List<Tablica> Tablice { get; set; } = new List<Tablica>();
        public List<DokumentRadniDan> RadniDani { get; set; } = new List<DokumentRadniDan>();
        public List<Stablo>? Stabla{ get; set; } = new List<Stablo>();

        public DokumentDoznaka() { }
    }
}
