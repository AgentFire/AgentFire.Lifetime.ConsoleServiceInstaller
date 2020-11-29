using AgentFire.Lifetime.ConsoleServiceInstaller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace TestProject
{
    [RunInstaller(true)]
    public sealed class Ins : SmartNetworkServiceInstaller
    {
        public Ins() : base("My Test Service", "http://+:80/yay", "https://+:443/yay") { }

        protected override IEnumerable<string> EnumerateDependencies()
        {
            return base.EnumerateDependencies();//.Concat(ServiceHelper.GetInstalledMssqlServerInstances());
        }

        protected override IEnumerable<string> ServiceParameters => new string[] { "-dev", "-test" };
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (WinService service = new WinService())
            {
                if (Environment.UserInteractive)
                {
                    // Startup as console app.
                    Controller.Run(service.ServiceName, new RunParameters(), args != null && args.Length == 1 ? args[0] : null);
                }
                else
                {
                    Thread.Sleep(5000);
                    // Startup as service.
                    ServiceBase.Run(service);
                }
            }
        }
    }
}
