using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceProcess;

namespace AgentFire.Lifetime.ConsoleServiceInstaller
{
    public abstract class AdvancedServiceBase : Installer
    {
        private static readonly SecurityIdentifier _networkServiceId = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);

        public abstract ServiceAccount Account { get; }
        public abstract ServiceStartMode StartType { get; }

        protected AdvancedServiceBase(string serviceName)
        {
            // Instantiate installer for process and service.
            ServiceProcessInstaller processInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();
            
            processInstaller.Account = Account;
            serviceInstaller.StartType = StartType;
            serviceInstaller.ServiceName = serviceName;

            // Set the dependencies.
            serviceInstaller.ServicesDependedOn = EnumerateDependencies().ToArray();

            // Add installer to collection. Order is not important if more than one service.
            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }

        #region Directory Access Permissions

        /// <summary>
        /// Adds specified access permissions to the directory in which the Entry Assembly (exe) is located to the <see cref="ServiceAccount.NetworkService"/> account.
        /// Run this on <see cref="Installer.OnAfterInstall"/> event if you want the <see cref="ServiceAccount.NetworkService"/> to be able to, at least, start your service.
        /// </summary>
        /// <param name="rights">Default value: FullControl minus TakeOwnership and ChangePermissions.</param>
        protected void AddNetworkPermissions(FileSystemRights rights = FileSystemRights.FullControl ^ (FileSystemRights.TakeOwnership | FileSystemRights.ChangePermissions))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(ServiceHelper.ServiceDir);
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();

            dirSecurity.AddAccessRule(new FileSystemAccessRule(_networkServiceId, rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));

            dirInfo.SetAccessControl(dirSecurity);
        }

        /// <summary>
        /// Reverts changes made by <see cref="AddNetworkPermissions"/>.
        /// </summary>
        protected void RemoveNetworkPermissions()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(ServiceHelper.ServiceDir);
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();

            dirSecurity.PurgeAccessRules(_networkServiceId);

            dirInfo.SetAccessControl(dirSecurity);
        }

        #endregion
        #region Network Namespace Reservation

        /// <summary>
        /// Reserves specified URLs for Network Service system account (for HTTP listen usage).
        /// Run this on <see cref="Installer.OnAfterInstall"/> event if you want the <see cref="ServiceAccount.NetworkService"/> to be able to register HTTP ports using the HTTP.SYS driver.
        /// </summary>
        /// <param name="urls">A list of URLs your HttpListener is going to use.</param>
        protected void AddNamespaceReservations(params string[] urls)
        {
            string accName = _networkServiceId.Translate(typeof(NTAccount)).Value;

            foreach (string url in urls)
            {
                using (Process proc = Process.Start("netsh", $"http add urlacl url={url} user=\"{accName}\""))
                {
                    proc.Start();
                    proc.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Reverts changes made by <see cref="AddNamespaceReservations"/>.
        /// </summary>
        protected void RemoveNamespaceReservations(params string[] urls)
        {
            foreach (string url in urls)
            {
                using (Process proc = Process.Start("netsh", $"http delete urlacl url={url}"))
                {
                    proc.Start();
                    proc.WaitForExit();
                }

            }
        }

        #endregion

        protected virtual IEnumerable<string> EnumerateDependencies() => Enumerable.Empty<string>();
    }
}
