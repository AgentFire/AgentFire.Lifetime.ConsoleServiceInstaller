Usage:

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
                    // Use any or your services you want. The args is passed from Windows correctly and can be used.
                    ServiceBase.Run(service);
                }
            }
        }
    }
    
Notes:

1. Make sure your `Installer` class is located in the Entry Assembly (exe).
2. Make sure your `Installer` class has `[RunInstaller(true)]` attribute.
3. Make sure your `Installer` class is **not** located in some other class, that is, nested.
4. Use `Environment.UserInteractive` property to detect whether or not your program is run as service or as desktop-user-started.
5. `using (WinService service = new WinService())` is not necessary. I just like to dispose of things. :)
6. `SmartNetworkServiceInstaller` isn't the only thing in there.
