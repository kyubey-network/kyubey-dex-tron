using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class TokenManifest
    {
        public string Id { get; set; }

        public string[] Owners { get; set; }

        public int Priority { get; set; }

        public TokenManifestBasic Basic { get; set; }

        public Incubation Incubation { get; set; }

        public bool Dex { get; set; }

        public bool Contract_Exchange { get; set; }
    }

    public class Incubation
    {
        public decimal Goal { get; set; }

        public DateTime DeadLine { get; set; }

        public DateTime? Begin_Time { get; set; }
    }

    public class TokenManifestBasic
    {
        public string Protocol { get; set; }

        public TokenManifestBasicContract Contract { get; set; }

        public string Website { get; set; }

        public string Github { get; set; }

        public string Email { get; set; }

        public string Tg { get; set; }

        public double[] Curve_Arguments { get; set; }

        public string Price_Table { get; set; }

        public string Price_Scope { get; set; }
    }

    public class TokenManifestBasicContract
    {
        public string Transfer { get; set; }

        public string Pricing { get; set; }

        public string Depot { get; set; }
    }
}
