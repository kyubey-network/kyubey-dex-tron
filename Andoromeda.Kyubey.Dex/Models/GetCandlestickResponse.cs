using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetCandlestickResponse
    {
        public double Opening { get; set; }
            
        public double Closing { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public DateTime Time { get; set; }

        public int Volume { get; set; }
    }
}
