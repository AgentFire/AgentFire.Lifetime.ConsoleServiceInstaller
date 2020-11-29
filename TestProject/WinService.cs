using System.ServiceProcess;
using System.Threading;

namespace TestProject
{
    partial class WinService : ServiceBase
    {
        public WinService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.

        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
