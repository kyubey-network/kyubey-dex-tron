using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetFavoriteResponse
    {
        public string Symbol { get; set; } 

        public double UnitPrice { get; set; }

        public double Change { get; set; }

        public bool Favorite { get; set; }
    }
}
