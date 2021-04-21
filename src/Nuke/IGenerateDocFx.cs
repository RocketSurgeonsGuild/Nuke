using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DocFX;
using Nuke.Common.ValueInjection;
using System.IO;
using System.Threading.Tasks;
using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke
{
    public interface IGenerateDocFx : IHaveDocs
    {
        [Parameter("serve the docs")]
        public bool? Serve => EnvironmentInfo.GetVariable<bool?>("Serve")
         // ?? ValueInjectionUtility.TryGetValue(() => Serve)
         ?? false;

        public Target CoreDocs => _ => _
           .OnlyWhenStatic(() => FileSystemTasks.DirectoryExists(DocumentationDirectory))
           .OnlyWhenStatic(() => FileSystemTasks.FileExists(DocumentationDirectory / "docfx.json"))
           .Executes(
                () =>
                {
                    DocFXTasks.DocFXMetadata(
                        z => z.SetProcessWorkingDirectory(DocumentationDirectory)
                    );
                    DocFXTasks.DocFXBuild(
                        z => z.SetProcessWorkingDirectory(DocumentationDirectory)
                           .SetOutputFolder(ArtifactsDirectory)
                    );

                    if (Serve == true)
                    {
                        Task.Run(
                            () =>
                                DocFXTasks.DocFXServe(
                                    z => z.SetProcessWorkingDirectory(DocumentationDirectory)
                                       .SetFolder(DocumentationsOutputDirectory)
                                )
                        );

                        var watcher = new FileSystemWatcher(DocumentationDirectory) { EnableRaisingEvents = true };
                        while (true)
                        {
                            watcher.WaitForChanged(WatcherChangeTypes.All);
                            DocFXTasks.DocFXBuild(
                                z => z.SetProcessWorkingDirectory(DocumentationDirectory)
                                   .SetOutputFolder(ArtifactsDirectory)
                            );
                        }
                    }
                }
            );
    }
}