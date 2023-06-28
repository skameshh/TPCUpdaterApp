using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.ProcurementDIQ
{
    class ProcurementDIQDao
    {
        public String ProcDate { set; get; }
        public int SoCount { set; get; }
        public int SoLineCount { set; get; }
        public int TKCount { set; get; }
        public int TKLineCount { set; get; }
        public int FoCount { set; get; }
        public int FoLineCount { set; get; }

        public int PinfoInProgressCount { set; get; }
        public int PinfoLineCount { set; get; }
    }
}
