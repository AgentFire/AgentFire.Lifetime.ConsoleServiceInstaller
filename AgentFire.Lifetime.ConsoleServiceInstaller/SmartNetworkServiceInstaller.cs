using System.Collections;
using System.ServiceProcess;

namespace AgentFire.Lifetime.ConsoleServiceInstaller
{
    /// <summary>
    /// Installs as auto-startup <see cref="ServiceAccount.NetworkService"/> with added access permissions to the service's directory and a system-reserved list of http-listen prefixes.
    /// </summary>
    public abstract class SmartNetworkServiceInstaller : AdvancedServiceBase
    {
        public override ServiceAccount Account => ServiceAccount.NetworkService;
        public override ServiceStartMode StartType => ServiceStartMode.Automatic;

        private string[] Urls { get; }

        protected SmartNetworkServiceInstaller(string serviceName, params string[] urls) : base(serviceName)
        {
            Urls = urls;
        }
        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
            AddNetworkPermissions();
            AddNamespaceReservations(Urls);
        }
        protected override void OnAfterUninstall(IDictionary savedState)
        {
            base.OnAfterUninstall(savedState);
            RemoveNetworkPermissions();
            RemoveNamespaceReservations(Urls);
        }
    }
}
