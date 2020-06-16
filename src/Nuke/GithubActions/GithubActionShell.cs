namespace Rocket.Surgery.Nuke.GithubActions
{
    public class GithubActionShell
    {
        public GithubActionShell(string shell)
        {
            Shell = shell;
        }
        public string Shell { get; }


        public static implicit operator GithubActionShell(string shell)
        {
            return new GithubActionShell(shell);
        }
        public static implicit operator string(GithubActionShell shell)
        {
            return shell?.Shell;
        }

        public static GithubActionShell Bash = new GithubActionShell("bash");
        public static GithubActionShell Pwsh = new GithubActionShell("pwsh");
        public static GithubActionShell Python = new GithubActionShell("python");
        public static GithubActionShell Sh = new GithubActionShell("sh");
        public static GithubActionShell Cmd = new GithubActionShell("cmd");
        public static GithubActionShell Powershell = new GithubActionShell("powershell");
        public override string ToString() => Shell;
    }
}