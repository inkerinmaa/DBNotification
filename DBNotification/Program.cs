using System;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Transactions;
using Topshelf;

/* 1. Copy source\repos\DBNotification\DBNotification\bin\Debug\net6.0 to some folder
 * 2. Run in cmd <DBNotification.exe install start> with admin rights */

namespace DBNotification
{
    class Program
    {
        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<DB_Monitoring>(s =>
                {
                    s.ConstructUsing(DB_Monitoring => new DB_Monitoring());
                    s.WhenStarted(DB_Monitoring => DB_Monitoring.Start());
                    s.WhenStopped(DB_Monitoring => DB_Monitoring.Stop());
                });

                x.RunAsLocalSystem();
                x.SetServiceName("DB_Monitoring");
                x.SetDisplayName("DB_Monitoring");
                x.SetDescription("DB_Monitoring");
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}