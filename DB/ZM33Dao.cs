using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
    class ZM33Dao
    {
        public int Id { set; get; }
        public String Plant { set; get; }
        public String WorkCentre { set; get; }
        public int DaysInQueue { set; get; }
        public String PreCompDate { set; get; }
        public int PreOper { set; get; }
        public String PreWC { set; get; }

        public int CurrentOper { set; get; }
        public String CurrentOperDesc { set; get; }

    }
}
