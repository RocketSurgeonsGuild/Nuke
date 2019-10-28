using System.Collections.Generic;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities.Collections;
using System.IO;
using System.Linq;
using System;
using Nuke.Common;
using System.Xml.Linq;
using Buildalyzer;
using NuGet.Protocol.Core.Types;
using NuGet.Configuration;
using System.Threading.Tasks;
using static Nuke.Common.IO.PathConstruction;
using NuGet.Protocol;
using System.Threading;

#if NETSTANDARD2_1
namespace Rocket.Surgery.Nuke.SyncPackages
{
    public static class PackageSync
    {
        public static async Task AddMissingPackages(
            AbsolutePath solutionPath,
            AbsolutePath packagesProps,
            CancellationToken cancellationToken
        )
        {
            XDocument document;
            {
                using var packagesFile = File.OpenRead(packagesProps);
                document = XDocument.Load(packagesFile);
            }
            var packageReferences = document.Descendants("PackageReference")
                .Concat(document.Descendants("GlobalPackageReference"))
                .Select(x => x.Attributes().First(x => x.Name == "Include" || x.Name == "Update").Value)
                .ToArray();

            var missingPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var am = new AnalyzerManager(solutionPath, new AnalyzerManagerOptions());
            foreach (var project in am.Projects.Values.SelectMany(project => project.Build()))
            {
                if (project.Items.TryGetValue("PackageReference", out var projectPackageReferences))
                {
                    foreach (var item in projectPackageReferences
                        .Where(x => !x.Metadata.ContainsKey("IsImplicitlyDefined") && !x.Metadata.ContainsKey("Version"))
                    )
                    {
                        if (packageReferences.Any(z => z.Equals(item.ItemSpec, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }
                        missingPackages.Add(item.ItemSpec);
                    }
                }
            }

            var itemGroups = document.Descendants("ItemGroup");
            var itemGroupToInsertInto = itemGroups.Count() > 2 ? itemGroups.Skip(1).Take(1).First() : itemGroups.Last();

            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());  // Add v3 API support
            var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
            var sourceRepository = new SourceRepository(packageSource, providers);
            var sourceCacheContext = new SourceCacheContext();

            foreach (var item in missingPackages.OrderBy(x => x))
            {
                var element = new XElement("PackageReference");
                element.SetAttributeValue("Update", item);
                {
                    var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>().ConfigureAwait(false);
                    var resolvedPackages = await dependencyInfoResource.ResolvePackages(item, sourceCacheContext, NuGet.Common.NullLogger.Instance, cancellationToken).ConfigureAwait(false);
                    var packageInfo = resolvedPackages.OrderByDescending(x => x.Identity.Version).First();
                    element.SetAttributeValue("Version", packageInfo.Identity.Version.ToString());
                }
                itemGroupToInsertInto.Add(element);
            }

            using var fileWrite = File.OpenWrite(packagesProps);

            await document.SaveAsync(fileWrite, SaveOptions.None, CancellationToken.None);
        }

        public static async Task RemoveExtraPackages(
            AbsolutePath solutionPath,
            AbsolutePath packagesProps,
            CancellationToken cancellationToken
        )
        {
            XDocument document;
            {
                using var packagesFile = File.OpenRead(packagesProps);
                document = XDocument.Load(packagesFile);
            }
            var packageReferences = document.Descendants("PackageReference")
                .Concat(document.Descendants("GlobalPackageReference"))
                .Select(x => x.Attributes().First(x => x.Name == "Include" || x.Name == "Update").Value)
                .ToList();

            var am = new AnalyzerManager(solutionPath, new AnalyzerManagerOptions());
            foreach (var project in am.Projects.Values.SelectMany(project => project.Build()))
            {
                if (project.Items.TryGetValue("PackageReference", out var projectPackageReferences))
                {
                    foreach (var item in projectPackageReferences)
                    {
                        packageReferences.Remove(item.ItemSpec);
                    }
                }
            }

            if (packageReferences.Count > 0)
            {
                foreach (var package in packageReferences)
                {
                    foreach (var item in document.Descendants("PackageReference")
                        .Concat(document.Descendants("GlobalPackageReference")
                        .Where(x => x.Attributes().First(x => x.Name == "Include" || x.Name == "Update").Value.Equals(package, StringComparison.OrdinalIgnoreCase)))
                        .ToArray())
                    {
                        item.Remove();
                    }
                }
            }

            using var fileWrite = File.Open(packagesProps, FileMode.Truncate);

            await document.SaveAsync(fileWrite, SaveOptions.None, CancellationToken.None);
        }
    }
}
#endif