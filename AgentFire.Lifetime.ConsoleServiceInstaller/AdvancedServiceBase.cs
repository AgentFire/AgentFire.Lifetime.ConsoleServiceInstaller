using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceProcess;

namespace AgentFire.Lifetime.ConsoleServiceInstaller
{
    public abstract class AdvancedServiceBase : Installer
    {
        public abstract ServiceAccount Account { get; }
        public abstract ServiceStartMode StartType { get; }

        private const string BuiltInAdministratorsRole = @"BUILTIN\Administrators";

        protected AdvancedServiceBase(string serviceName)
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            // Instantiate installer for process and service.
            ServiceProcessInstaller processInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller1 = new ServiceInstaller();

            processInstaller.Account = Account;
            serviceInstaller1.StartType = StartType;
            serviceInstaller1.ServiceName = serviceName;

            // Add installer to collection. Order is not important if more than one service.
            Installers.Add(serviceInstaller1);
            Installers.Add(processInstaller);
        }

        #region Directory Access Permissions

        /// <summary>
        /// Adds specified access permissions to the directory in which the Entry Assembly (exe) is located to the <see cref="ServiceAccount.NetworkService"/> account.
        /// Run this on <see cref="Installer.OnAfterInstall"/> event if you want the <see cref="ServiceAccount.NetworkService"/> to be able to, at least, start your service.
        /// </summary>
        /// <param name="rights">Default value: FullControl minus TakeOwnership and ChangePermissions.</param>
        [PrincipalPermission(SecurityAction.Demand, Role = BuiltInAdministratorsRole)]
        protected void AddNetworkPermissions(FileSystemRights rights = FileSystemRights.FullControl ^ (FileSystemRights.TakeOwnership | FileSystemRights.ChangePermissions))
        {
            string serviceDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            DirectoryInfo dirInfo = new DirectoryInfo(serviceDir);
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();

            SecurityIdentifier id = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);

            dirSecurity.AddAccessRule(new FileSystemAccessRule(id, rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));

            dirInfo.SetAccessControl(dirSecurity);
        }

        /// <summary>
        /// Reverts changes made by <see cref="AddNetworkPermissions"/>.
        /// </summary>
        [PrincipalPermission(SecurityAction.Demand, Role = BuiltInAdministratorsRole)]
        protected void RemoveNetworkPermissions()
        {
            string serviceDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            DirectoryInfo dirInfo = new DirectoryInfo(serviceDir);
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();

            SecurityIdentifier id = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);

            dirSecurity.PurgeAccessRules(id);

            dirInfo.SetAccessControl(dirSecurity);
        }

        #endregion
        #region Network Namespace Reservation

        /// <summary>
        /// Reserves specified URLs for Network Service system account (for HTTP listen usage).
        /// Run this on <see cref="Installer.OnAfterInstall"/> event if you want the <see cref="ServiceAccount.NetworkService"/> to be able to register HTTP ports using the HTTP.SYS driver.
        /// </summary>
        /// <param name="urls">A list of URLs your HttpListener is going to use.</param>
        [PrincipalPermission(SecurityAction.Demand, Role = BuiltInAdministratorsRole)]
        protected void AddNamespaceReservations(params string[] urls)
        {
            try
            {
                SecurityIdentifier id = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
                string accName = id.Translate(typeof(NTAccount)).Value;

                foreach (string url in urls)
                {
                    using (Process proc = Process.Start("netsh", $"http add urlacl url={url} user=\"{accName}\""))
                    {
                        proc.Start();
                        proc.WaitForExit();
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Reverts changes made by <see cref="AddNamespaceReservations"/>.
        /// </summary>
        [PrincipalPermission(SecurityAction.Demand, Role = BuiltInAdministratorsRole)]
        protected void RemoveNamespaceReservations(params string[] urls)
        {
            try
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
            catch { }
        }

        #endregion
    }
}
