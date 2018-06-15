namespace AgentFire.Lifetime.ConsoleServiceInstaller
{
    public sealed class RunParameters
    {
        public string WelcomeText { get; set; } = "Welcome to the console service installer.";
        public bool AllowContinue { get; set; } = false;
        public bool ShowHelpAtStartup { get; set; } = true;
        public bool SayGoodbye { get; set; } = false;
    }
}
