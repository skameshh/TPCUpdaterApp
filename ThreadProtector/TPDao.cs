using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.ThreadProtector
{
   public class TPDao
    {
        public String ItemNum { set; get; }
        public String ItemDesc { set; get; }
        public int MinStock { set; get; }
        public int ReOrderLevel { set; get; }
        public int ReorderQty { set; get; }
        public String ItemType { set; get; }
        public String itemSize { set; get; }
        public int CurrentQty { set; get; }
        public String ConnectionType { set; get; }
        public String Material { set; get; }
        public String Vendor { set; get; }

    }
}
