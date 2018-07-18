using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace AgentFire.Lifetime.ConsoleServiceInstaller
{
    internal static class Tool
    {
        private static ServiceController GetService(string serviceName) => ServiceController.GetServices().Where(T => T.ServiceName == serviceName).SingleOrDefault();
        private static readonly string _location = Assembly.GetEntryAssembly().Location;
        public static bool IsDebugMode { get; } = Debugger.IsAttached;

        public static void InstallService()
        {
            Console.Write("Installing... ");

            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { _location });
                Console.WriteLine("Done!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static void UninstallService()
        {
            Console.Write("Uninstalling... ");

            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", _location });
                Console.WriteLine("Done!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void RunService(string serviceName)
        {
            try
            {
                GetService(serviceName).Start();
                GetService(serviceName).WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static void StopService(string serviceName)
        {
            try
            {
                GetService(serviceName).Stop();
                GetService(serviceName).WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static void GetServiceState(string serviceName, out bool isInstalled, out bool isRunning)
        {
            ServiceController sc = GetService(serviceName);

            isInstalled = sc != null;
            isRunning = isInstalled && sc.Status == ServiceControllerStatus.Running;
        }
    }
}
