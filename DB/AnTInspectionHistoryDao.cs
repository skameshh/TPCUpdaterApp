using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
    public class AnTInspectionHistoryDao
    {
        public string Id { set; get; }
        public string FixtureId { set; get; }
        public DateTime LastInsDate { set; get; }
        public DateTime NextInsDate { set; get; }
        public string LastInsWO { set; get; }
        public int UsageCount { set; get; }
        public int Status { set; get; }
        public string Remarks { set; get; }
        public string LocationQR { set; get; }
        public string BusinessUnit { set; get; }
    }
}
