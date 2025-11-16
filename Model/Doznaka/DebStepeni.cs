using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeddyBearExport.Model.Doznaka
{
    public class DebStepeni
    {
        public float DebStepen { get; set; } = 0f;
        public Guid TablicaId { get; set; } = Guid.NewGuid();
        public DateTime RadniDan { get; set; } = DateTime.Now;
        public int Kolicina { get; set; } = 0;
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public DebStepeni() { }
    }
}
