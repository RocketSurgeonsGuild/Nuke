using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DocFX;
using Nuke.Common.ValueInjection;
using System.IO;
using System.Threading.Tasks;

namespace Rocket.Surgery.Nuke
{
    public interface IGenerateDocFx : IHaveArtifacts
    {
        public AbsolutePath DocumentationDirectory => NukeBuild.RootDirectory / "docs";

        public AbsolutePath DocumentationsOutputDirectory => ArtifactsDirectory / "docs";

        [Parameter("serve the docs")]
        public bool? Serve => EnvironmentInfo.GetVariable<bool?>("Serve")
         ?? ValueInjectionUtility.TryGetValue(() => Serve)
         ?? false;

        public Target CoreDocs => _ => _
           .OnlyWhenStatic(() => FileSystemTasks.DirectoryExists(DocumentationDirectory))
           .OnlyWhenStatic(() => FileSystemTasks.FileExists(DocumentationDirectory / "docfx.json"))
           .Executes(
                () =>
                {
                    DocFXTasks.DocFXMetadata(
                        z => global::Nuke.Common.Tooling.ToolSettingsExtensions.SetProcessWorkingDirectory<DocFXMetadataSettings>(z, (string)DocumentationDirectory)
                    );
                    DocFXTasks.DocFXBuild(
                        z => global::Nuke.Common.Tooling.ToolSettingsExtensions.SetProcessWorkingDirectory<DocFXBuildSettings>(z, (string)DocumentationDirectory)
                           .SetOutputFolder(ArtifactsDirectory)
                    );

                    if (Serve == true)
                    {
                        Task.Run(
                            () =>
                                DocFXTasks.DocFXServe(
                                    z => global::Nuke.Common.Tooling.ToolSettingsExtensions.SetProcessWorkingDirectory<DocFXServeSettings>(z, (string)DocumentationDirectory)
                                       .SetFolder(DocumentationsOutputDirectory)
                                )
                        );

                        var watcher = new FileSystemWatcher(DocumentationDirectory) { EnableRaisingEvents = true };
                        while (true)
                        {
                            watcher.WaitForChanged(WatcherChangeTypes.All);
                            DocFXTasks.DocFXBuild(
                                z => global::Nuke.Common.Tooling.ToolSettingsExtensions.SetProcessWorkingDirectory<DocFXBuildSettings>(z, (string)DocumentationDirectory)
                                   .SetOutputFolder(ArtifactsDirectory)
                            );
                        }
                    }
                }
            );
    }
}