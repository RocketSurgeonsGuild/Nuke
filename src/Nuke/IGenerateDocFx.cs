using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DocFX;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Interface used during docs generation
/// </summary>
public interface IGenerateDocFx : IHaveDocs
{
    /// <summary>
    ///     Parameter to be used to serve documentation
    /// </summary>
    [Parameter("serve the docs")]
    public bool? Serve => EnvironmentInfo.GetVariable<bool?>("Serve")
                          // ?? ValueInjectionUtility.TryGetValue(() => Serve)
                       ?? false;

    /// <summary>
    ///     The core docs to generate documentation
    /// </summary>
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
