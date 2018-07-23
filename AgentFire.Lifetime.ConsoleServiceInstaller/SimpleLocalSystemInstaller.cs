using System.ServiceProcess;

namespace AgentFire.Lifetime.ConsoleServiceInstaller
{
    /// <summary>
    /// Installs as auto-startup <see cref="ServiceAccount.LocalSystem"/>.
    /// </summary>
    public abstract class SimpleLocalSystemInstaller : AdvancedServiceBase
    {
        public override ServiceAccount Account => ServiceAccount.LocalSystem;
        public override ServiceStartMode StartType => ServiceStartMode.Automatic;

        protected SimpleLocalSystemInstaller(string serviceName) : base(serviceName) { }
    }
}
