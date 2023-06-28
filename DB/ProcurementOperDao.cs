using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
   public class ProcurementOperDao
    {
        public int Id { set; get; }
        public string ProdZEWO { set; get; }
        public int OperNo { set; get; }
        public String Operation { set; get; }
        public string SRTKey { set; get; }
        public bool Deleted { set; get; }
    }
}
