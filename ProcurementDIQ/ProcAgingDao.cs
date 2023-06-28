using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.ProcurementDIQ
{
    public class ProcAgingDao
    {
        public String MaterialNum { set; get; }
        public String ProdOrdNum { set; get; }
        public String Resource { set; get; }
        public String Remarks { set; get; }
        public int Diq { set; get; }

    }
}
