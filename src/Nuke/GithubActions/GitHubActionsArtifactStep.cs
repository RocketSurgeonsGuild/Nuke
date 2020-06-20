using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;
using Humanizer;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public class UploadArtifactStep : UsingStep
    {
        public UploadArtifactStep(string name) : base(name)
        {
            Uses = "actions/upload-artifact@v2";
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            WithProperties(x => x.Underscore().Camelize().ToLowerInvariant());
            base.Write(writer);
        }

        protected override string GetStepName(string name)
        {
            return $"🏺 {name}";
        }
    }
}