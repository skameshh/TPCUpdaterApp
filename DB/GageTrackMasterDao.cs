using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.DB
{
   public class GageTrackMasterDao
    {
        public string GageId { set; get; }
        public string GageDescription { set; get; }
        public string Unit { set; get; }
        public string StorageLocation { set; get; }
        public string CurrentLocation { set; get; }
        public double CalibrationFrequency { set; get; }
        public string CablicarationUO { set; get; }
        public DateTime NextDueDate { set; get; }
        public DateTime LastCalibDate { set; get; }
        public string Status { set; get; }
        public string GMType { set; get; }
        public string StandaardGroup { set; get; }

        public string UserRef1 { set; get; }
        public string UserRef2 { set; get; }
        public string UserRef3 { set; get; }
        public string UserRef4 { set; get; }
        public string UserRef5 { set; get; }
    }
}
