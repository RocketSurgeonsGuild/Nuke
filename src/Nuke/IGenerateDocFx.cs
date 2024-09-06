using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Rocket.Surgery.Nuke.GithubActions;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Interface used during docs generation
/// </summary>
[PublicAPI]
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
    ///     The docfx tool
    /// </summary>
    public Tool Docfx => DotNetTool.GetTool("docfx");

    /// <summary>
    ///     The core docs to generate documentation
    /// </summary>
    [NonEntryTarget]
    public Target GenerateDocFx => d => d
                                  .TryDependentFor<IHaveDocs>(z => z.Docs)
                                  .OnlyWhenStatic(() => DocumentationDirectory.DirectoryExists())
                                  .OnlyWhenStatic(() => ( DocumentationDirectory / "docfx.json" ).FileExists())
                                  .Executes(
                                       () =>
                                       {
                                           if (Serve == true)
                                           {
                                               Task.Run(() => Docfx($"{DocumentationDirectory / "docfx.json"} --serve"));

                                               var watcher = new FileSystemWatcher(DocumentationDirectory) { EnableRaisingEvents = true, };
                                               while (true)
                                               {
                                                   watcher.WaitForChanged(WatcherChangeTypes.All);
                                                   Docfx($"{DocumentationDirectory / "docfx.json"}");
                                               }
                                           }

                                           Docfx($"{DocumentationDirectory / "docfx.json"}");
                                       }
                                   );
}
