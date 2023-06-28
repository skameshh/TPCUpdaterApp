using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPC2UpdaterApp.Trigger;

namespace TPC2UpdaterApp.Jobs
{
   public class TestCellBackupJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            TestCellBackupTrigger.doTestcellbackup();

            return Task.CompletedTask;

        }
    }
}
