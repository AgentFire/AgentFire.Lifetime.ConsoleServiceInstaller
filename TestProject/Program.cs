using AgentFire.Lifetime.ConsoleServiceInstaller;
using System;
using System.ServiceProcess;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WinService service = new WinService())
            {
                if (Environment.UserInteractive)
                {
                    // Startup as console app.
                    Controller.Run(service.ServiceName, new RunParameters());
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
