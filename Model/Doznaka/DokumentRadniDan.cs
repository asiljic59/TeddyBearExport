using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeddyBearExport.Model.Doznaka
{
    public class DokumentRadniDan
    {
        public DateOnly? Dan { get; set; } = null;
        public int UkupnoDoznaceni { get; set; } = 0;
        public float UkupnoZapremina { get; set; } = 0f;

        public int StBuducnosti {  get; set; } = 0;

        public float DozPovrsina { get; set; } = 0;
    }
}
