using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC2UpdaterApp.Jobs
{
    class MYJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            // throw new NotImplementedException

            Console.WriteLine("\n\n"+MYGlobal.getCurretnTime() +" - Hi there TEST JOB \n\n");

            return Task.CompletedTask;

        }
    }
}
