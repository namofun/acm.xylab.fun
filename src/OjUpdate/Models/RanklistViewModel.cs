#nullable disable
using System;
using System.Collections.Generic;

namespace SatelliteSite.OjUpdateModule.Models
{
    public class RanklistViewModel
    {
        public IEnumerable<OjAccount> OjAccounts { get; set; }

        public bool IsUpdating { get; set; }

        public DateTimeOffset LastUpdate { get; set; }

        public Func<int?, string> RankTemplate { get; set; }

        public string AccountTemplate { get; set; }

        public string OjName { get; set; }

        public string CountColumn { get; set; }
    }
}
