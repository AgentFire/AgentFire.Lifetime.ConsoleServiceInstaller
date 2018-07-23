using AgentFire.Lifetime.ConsoleServiceInstaller;
using System;
using System.ComponentModel;
using System.ServiceProcess;

namespace TestProject
{
    [RunInstaller(true)]
    public sealed class Ins : SmartNetworkServiceInstaller
    {
        public Ins() : base("My Test Service", "http://+:80/yay", "https://+:443/yay") { }
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
                    // Startup as service.
                    ServiceBase.Run(service);
                }
            }
        }
    }
}
