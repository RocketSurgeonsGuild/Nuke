using System.Collections.Immutable;
using System.Xml.Linq;

using Microsoft.Extensions.FileSystemGlobbing;

using Nuke.Common.IO;
using Nuke.Common.Tools.Git;

using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Looks at project, props, and target files and removes unused dependencies
/// </summary>
public interface IRemoveUnusedDependencies : INukeBuild
{
    /// <summary>
    ///     Looks at project, props, and target files and removes unused dependencies
    /// </summary>
    Target RemoveUnusedDependencies =>
        _ => _
               .TriggeredBy<ICanLint>(x => x.Lint)
               .Executes(
                    async () =>
                    {
                        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase)
                                     .AddInclude("**/*.*proj")
                                     .AddInclude("**/*.props")
                                     .AddInclude("**/*.targets");
                        var documents = await GitTasks
                                             .Git("ls-files", NukeBuild.RootDirectory, logOutput: false, logInvocation: false)
                                             .Select(z => z.Text.Trim())
                                             .Select(AbsolutePath.Create)
                                             .Match(matcher)
                                             .ToAsyncEnumerable()
                                             .Select(x => (Path: x, Document: XDocument.Load(x, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo)))
                                             .ToListAsync();

                        var xmlDocs = documents.ToImmutableDictionary(
                            z => RootDirectory.GetRelativePathTo(z.Path),
                            path => path.Document
                        );

                        var globalPackageReferences = xmlDocs
                                                     .Values
                                                     .SelectMany(x => x.Descendants("GlobalPackageReference"))
                                                     .Where(x => x.Attribute("Include") is { } || x.Attribute("Update") is { })
                                                     .Select(
                                                          x => new PackageVersion(
                                                              x.Attribute("Include")?.Value
                                                           ?? x.Attribute("Update")?.Value ?? "",
                                                              x.Attribute("Version")?.Value ?? "",
                                                              x
                                                          )
                                                      )
                                                     .Select(z => z.Name)
                                                     .ToImmutableHashSet();

                        var packageReferenceValues = xmlDocs
                                                    .Values
                                                    .SelectMany(x => x.Descendants("PackageReference"))
                                                    .Where(x => x.Attribute("Include") is { } || x.Attribute("Update") is { })
                                                    .Select(
                                                         x => new PackageVersion(
                                                             x.Attribute("Include")?.Value
                                                          ?? x.Attribute("Update")?.Value ?? "",
                                                             x.Attribute("Version")?.Value ?? "",
                                                             x
                                                         )
                                                     )
                                                    .ToImmutableArray();
                        var packageReferences = packageReferenceValues
                                               .Select(z => z.Name)
                                               .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

                        var packageVersions = xmlDocs
                                             .Values
                                             .SelectMany(x => x.Descendants("PackageVersion"))
                                             .Where(x => x.Attribute("Include") is { } || x.Attribute("Update") is { })
                                             .Select(
                                                  x => new PackageVersion(
                                                      x.Attribute("Include")?.Value
                                                   ?? x.Attribute("Update")?.Value ?? "",
                                                      x.Attribute("Version")?.Value ?? "",
                                                      x
                                                  )
                                              )
                                             .ToImmutableArray();

                        var unusedPackageVersions = packageVersions
                                                   .Where(x => !packageReferences.Contains(x.Name))
                                                   .ToImmutableArray();

                        var documentsUpdated = new HashSet<XDocument>();

                        foreach (var package in unusedPackageVersions)
                        {
                            Log.Information(
                                "Removing unused package version {PackageName} {PackageVersion}",
                                package.Name,
                                package.Version
                            );
                            if (package.Element.Document is { })
                                documentsUpdated.Add(package.Element.Document);
                            package.Element.Remove();
                        }

                        var usedGlobalPackageReferences = packageReferenceValues
                                                         .Where(p => globalPackageReferences.Contains(p.Name))
                                                         .ToImmutableArray();

                        foreach (var package in usedGlobalPackageReferences)
                        {
                            Log.Information("Removing ambiguous global package reference {PackageName}", package);
                            if (package.Element.Document is { })
                                documentsUpdated.Add(package.Element.Document);
                            package.Element.Remove();
                        }

                        foreach (var doc in xmlDocs.Where(z => documentsUpdated.Contains(z.Value)))
                        {
                            await using var file = File.Open(doc.Key, FileMode.Truncate, FileAccess.ReadWrite);
                            doc.Value.Declaration = null;
                            Log.Information("Saving {File}...", doc.Key);
                            await doc.Value.SaveAsync(file, SaveOptions.None, CancellationToken.None);
                        }
                    }
                );

    internal record PackageVersion(string Name, string Version, XElement Element);
}
