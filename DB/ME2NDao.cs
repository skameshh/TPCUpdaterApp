using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
    class ME2NDao
    {
        public Int32 Id { set; get; }
        public String PlantId { set; get; }
        public String PONum { set; get; }
        public int POItemNum { set; get; }
        public String MaterialNum { set; get; }
        public double POQty { set; get; }
        public String LastDelvDate { set; get; }

        public int NoOfDelv { set; get; }
        public String Note { set; get; }
        public String PRNum { set; get; }
        public int PRItemNu { set; get; }
        public DateTime LastUpdated { set; get; }

        //======= GR Info ===========
        public String GRTransDate { set; get; }
        public String GRVendorNum { set; get; }
        public int GRMomentType { set; get; }
        public DateTime PRCreatedDate { set; get; }
        public String PRCreatedBy { set; get; }

        //===== Vendor info===========
        public string VendorName { set; get; }
        public string VendorCode { set; get; }

    }
}
