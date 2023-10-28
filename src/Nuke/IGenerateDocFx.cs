using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DocFX;
using Nuke.Common.Tools.DotNet;

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
    
    public Tool Docfx => DotnetTool.GetTool("docfx"); 

    /// <summary>
    ///     The core docs to generate documentation
    /// </summary>
    public Target CoreDocs => _ => _
                                  .OnlyWhenStatic(() => DocumentationDirectory.DirectoryExists())
                                  .OnlyWhenStatic(() => ( DocumentationDirectory / "docfx.json" ).FileExists())
                                  .Executes(
                                       () =>
                                       {
                                           
                                           if (Serve == true)
                                           {
                                               Task.Run(() => Docfx($"{DocumentationDirectory / "docfx.json"} --serve"));

                                               var watcher = new FileSystemWatcher(DocumentationDirectory) { EnableRaisingEvents = true };
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
