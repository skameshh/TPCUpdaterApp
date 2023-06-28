using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
    public class SAPPurchaseOrder
    {
        public string PO_NBR { set; get; }
        public int PO_Item_no { set; get; }
        public string VendorNbr { set; get; }
        public decimal PO_Net_unit_price { set; get; }
        public decimal PO_price_unit { set; get; }
        public string PO_Currency { set; get; }
        public decimal Local_net_unit_price { set; get; }
        public string Local_currency { set; get; }


    }
}
