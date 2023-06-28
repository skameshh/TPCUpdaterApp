using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.TestCellBackupReport
{
    public class TestCellBackupDao
    {
        public int Id { set; get; }
        public string TestCellName { set; get; }
        public DateTime Rdate { set; get; }
        public int CameraCount { set; get; }
        public int CanaryCount { set; get; }
        public int ReportCount { set; get; }
        public int DBCount { set; get; }


    }
}
