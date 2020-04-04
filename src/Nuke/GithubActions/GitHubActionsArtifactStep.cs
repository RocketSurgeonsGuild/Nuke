using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public class GitHubActionsArtifactStep : GitHubActionsStep
    {
        public string ArtifactName { get; set; }
        public string Path { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- uses: actions/upload-artifact@v1");

            using (writer.Indent())
            {
                writer.WriteLine($"name: {Name}");
                if (!string.IsNullOrWhiteSpace(If))
                    writer.WriteLine($"if: {If}");
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine($"name: {ArtifactName}");
                    writer.WriteLine($"path: {Path}");
                }
            }
        }
    }
}