using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
    class TestT
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void doTest()
        {
            String sdt = "07/01/2020";
            DateTime dtx =   DateTime.ParseExact(sdt, "MM/dd/yyyy", CultureInfo.InvariantCulture);

            //String mmx = dtx.ToString("yyyy-MM-dd");
            //log.Info("Dtx " + mmx);
            //DateTime mmdd = DateTime.Parse(mmx);

            TimeSpan ts = DateTime.Now.Subtract(dtx);
            log.Info("ts " + ts +", days="+ ts.Days +", now = "+ DateTime.Now +", - "+ dtx.ToString());
            ;
        }


    }
}
