using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
    class UsageHistoryDao
    {

        public int Id { get; set; }
        public String Hid { get; set; }

        public String Name { get; set; }

        public String Action { get; set; }

        public DateTime ActionTime { get; set; }

        public String Remarks { get; set; }
    }
}
