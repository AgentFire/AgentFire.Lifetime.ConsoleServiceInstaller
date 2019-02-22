using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AgentFire.Lifetime.ConsoleServiceInstaller
{
    public static class ServiceHelper
    {
        internal static string ServiceDir { get; } = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

        /// <summary>
        /// Sets <see cref="Environment.CurrentDirectory"/> to the directory in which the entry assembly (exe) is located.
        /// <remarks>
        /// Which is essentially this code
        /// <para/>
        /// Environment.CurrentDirectory = ServiceDir
        /// </remarks>
        public static void SetLocalAsCurrentDirectory()
        {
            Environment.CurrentDirectory = ServiceDir;
        }

        public static IEnumerable<string> GetInstalledMssqlServerInstances()
        {
            // Based on: https://stackoverflow.com/questions/995403/how-can-i-define-a-dependency-on-sql-server-in-a-windows-service-that-works-with

            foreach (RegistryView view in new[] { RegistryView.Registry32, RegistryView.Registry64 })
            {
                using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                using (RegistryKey rk = localMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server"))
                {
                    string[] instances = (string[])rk?.GetValue("InstalledInstances");

                    if (instances == null)
                    {
                        continue;
                    }

                    foreach (string instance in instances)
                    {
                        yield return instance;
                    }
                }
            }

        }
    }
}
