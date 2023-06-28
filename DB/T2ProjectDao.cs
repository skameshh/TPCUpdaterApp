using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
   public class T2ProjectDao
    {
        public int Id { get; set;  }
        public String Plant { get; set; }
        public String ProjectName { get; set; }
        public String TrackingNumber { get; set; }
        public String PSLFamily { get; set; }
        public String SubPsl { get; set; }
        public String SingZewo { get; set; }
        public String SAPNetwork { get; set; }
        public String ActCode { get; set; }
        public String TechProjMgr { get; set; }
        public String TechRespEngr { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime NeedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime FinishedDate { get; set; }
        public String Remarks { get; set; }
        public String ProjectStatus { get; set; }
        public String AcceptedBy { get; set; }
        public String FormType { get; set; }
        public String TPContact { get; set; }

        public String Priority { get; set; }
        public DateTime PromisedDate { get; set; }
        public int TotalLineItems { get; set; }
        public int TotalCompleted { get; set; }
        public Double CompletedPercentage { get; set; }
        public String Ownership { get; set; }

        public String ProjUpdBy { get; set; }
        public DateTime ProjUpdOn { get; set; }

        public String ZNDPOMOT { get; set; }

        public String ZNDPOInterPlant { get; set; }

        public DateTime RecoveryDate { get; set; }
        public string SubmittedBy { set; get; }

        //special case for TechProejct Id
        public int TechProjectId { set; get; }
    }
}
