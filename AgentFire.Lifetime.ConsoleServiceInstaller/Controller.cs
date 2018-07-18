using System;
using System.Configuration.Install;
using System.Threading;

namespace AgentFire.Lifetime.ConsoleServiceInstaller
{
    public static class Controller
    {
        /// <summary>
        /// Runs the console-mode service manager, blocking the call until the user finishes working with it.
        /// Note: it is essential to have your <see cref="Installer"/> class located in the entry assembly (executable) since the installutil.exe will look for it in there.
        /// </summary>
        /// <param name="serviceName">The exact name of the Windows Service.</param>
        /// <param name="parameters">Some params.</param>
        /// <returns>True if user selected to conitnue in console mode (if enabled in <see cref="RunParameters"/>), otherwise false.</returns>
        public static bool Run(string serviceName, RunParameters parameters, string singleCommand = null)
        {
            bool exit = false;
            bool @continue = false;

            if (parameters.WelcomeText != null)
            {
                Console.WriteLine(parameters.WelcomeText);
            }

            if (parameters.ShowHelpAtStartup)
            {
                PrintUsage(parameters);
            }

            do
            {
                Tool.GetServiceState(serviceName, out bool isInstalled, out bool isRunning);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Service state: {(isInstalled ? $"installed, {(isRunning ? "running" : "stopped")}" : "not installed")}.");
                Console.ForegroundColor = ConsoleColor.Gray;

                bool validInput;

                do
                {
                    validInput = true;
                    Console.Write("Your input: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    string input;

                    if (singleCommand == null)
                    {
                        input = Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine(singleCommand);
                        input = singleCommand;
                    }

                    Console.ForegroundColor = ConsoleColor.Gray;

                    switch (input)
                    {
                        case "h":
                            PrintUsage(parameters);
                            break;
                        case "i":
                            Tool.InstallService();
                            break;
                        case "u":
                            Tool.UninstallService();
                            break;
                        case "r":
                            Tool.RunService(serviceName);
                            break;
                        case "s":
                            Tool.StopService(serviceName);
                            break;
                        case "c":
                            if (!parameters.AllowContinue)
                            {
                                validInput = false;
                            }

                            @continue = true;
                            exit = true;

                            break;
                        case "q":
                            if (parameters.SayGoodbye)
                            {
                                Console.WriteLine("See you!");
                                Thread.Sleep(500);
                            }

                            exit = true;
                            break;
                        default:
                            validInput = false;
                            break;
                    }

                    if (!validInput)
                    {
                        Console.WriteLine($"N{new string('o', Math.Max(1, input.Length))}pe.");
                    }
                } while (!validInput);

                if (singleCommand != null)
                {
                    exit = true;
                }
            } while (!exit);

            return @continue;
        }

        private static void PrintUsage(RunParameters parameters)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("[h] - show help (usage).");
            Console.WriteLine("[i] - install service.");
            Console.WriteLine("[u] - uninstall service.");
            Console.WriteLine("[r] - run (start) service.");
            Console.WriteLine("[s] - stop service.");

            if (parameters.AllowContinue)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[c] - continue in console mode.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.WriteLine("[q] - exit.");
        }
    }
}
