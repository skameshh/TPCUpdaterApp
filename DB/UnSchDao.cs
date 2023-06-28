using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
   public class UnSchDao
    {
        public int Id { set; get; }
        public String MfgOrder { set; get; }
        public String Plant { set; get; }
        public String UnConfirmWC { set; get; }

        public int Oper { set; get; }
        public int CurrentOper { set; get; }
        public int DaysInQueue { set; get; }

        public String MPMPCNFDate { set; get; }


        public String OperationId { set; get; }
        public String ResourceWC { set; get; }
        public String ResourceDesc { set; get; }
        public int Qty { set; get; }
        public double RunTimeHrs { set; get; }
    }
}
