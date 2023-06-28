using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
   public class T2MaterialDao
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public String MaterialNbr { get; set; }
        public int TotalReqQty { get; set; }
        public String MaterialDesc { get; set; }
        public String RawMaterial { get; set; }
        public String MaterialSpec { get; set; }
        public String ProdOrderNbr { get; set; }
        public String DrawingNbr { get; set; }
        public String DrawingRev { get; set; }
        public String Comments { get; set; }
        public DateTime MaterialDueDate { get; set; }
        public String MaterialStatus { get; set; }
        public String RoutingScope { get; set; }
        public String ProdZEWO { get; set; }
        public String RawMaterialAssmnt { get; set; }
        public String RoutingStatus { get; set; }
        public String Routers { get; set; }
        public String ProgramsList { get; set; }
        public String ProgramStatus { get; set; }
        public String RawMaterialETA { get; set; }
        public DateTime RMTKPUR_ETA { get; set; }
        public String RMTKPUR_POLN { get; set; }
        public DateTime RMTKPUR_GR_Date { get; set; }
        public String RMTKPUR_PReq { get; set; }
        public int PR_LN_NBR { get; set; }
        public String ZNDPO { get; set; }
        public int ZNDPO_LN { get; set; }
        public String Farmout_PO_LN { get; set; }
        public DateTime Farmout_ETA { get; set; }
        public DateTime Farmout_GR_Date { get; set; }
        public String Farmout_PReq { get; set; }
        public int Farmout_PR_LN { get; set; }
        public String ISDeletedItem { get; set; }
        public String ISCompletedItem { get; set; }
        public DateTime ExpectedCompDate { get; set; }
        public String Priority { get; set; }
        public int Diq { get; set; }
        public String SystemStatus { get; set; }
        public String PCNF { get; set; }
        public String UNConfirmWC { get; set; }
        public String UpdBy { get; set; }
        public DateTime UpdOn { get; set; }

        public String MaterialUnit { set; get; }
        public String MaterialRev { set; get; }
        public String VendorName { set; get; }
        public String BuyerName { set; get; }

        public DateTime WCDeliveryDate { set; get; }
        public String WCOwner { set; get; }
        public String WCRemarks { set; get; }
        public String WCVendor { set; get; }
        public DateTime WCPromisedDate { set; get; }

        public String Temp1 { set; get; }
        public String Temp2 { set; get; }
        public String Temp3 { set; get; }
    }
}
