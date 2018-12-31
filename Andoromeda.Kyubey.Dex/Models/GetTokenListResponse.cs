using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetTokenListResponse
    {
        public string symbol { get; set; }
        public double current_price { get; set; }
        public double change_recent_day { get; set; }
        public double max_price_recent_day { get; set; }
        public double min_price_recent_day { get; set; }
        public double volume_recent_day { get; set; }
        public bool is_recommend { get; set; }
        public string icon_src { get; set; }
        public int priority { get; set; }
        public double? newdex_price_ask { get; set; }
        public double? newdex_price_bid { get; set; }
        public double? whaleex_price { get; set; }
    }
}
