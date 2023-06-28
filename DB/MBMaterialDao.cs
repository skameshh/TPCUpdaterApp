using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
    class MBMaterialDao
    {
        public int MatlId { set; get; }
        public String MaterialNum { set; get; }
        public String MaterialStatus { set; get; }
        public String RoutingScope { set; get; }
        public String ProdZewo { set; get; }
        public String RawMaterialAssignmet { set; get; }
        public DateTime RMTKPurchGRDate { set; get; }
        public DateTime ActCompltdDateFromSAP { set; get; }
        public String RMTKPurchPartPOLine{ set; get; }
        public string WCUnconfirmOper { set; get; }
        public string RoutingSts { set; get; }
        public DateTime RoutingStsCompltdDate { set; get; }
        public int TotReqQty { set; get; }
    }
}
