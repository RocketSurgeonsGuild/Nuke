using System.Collections.Immutable;
using System.Xml.Linq;
using Nuke.Common.IO;
using Rocket.Surgery.Nuke.DotNetCore;
using Rocket.Surgery.Nuke.ProjectModel;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Methods used to allow common test tasks to be shared
/// </summary>
public static class TestMethodExtensions
{
    // ReSharper disable once CommentTypo
    /// <summary>
    ///     A method that ensures the given runsettings file exists or creates a default one
    /// </summary>
    /// <param name="target"></param>
    /// <param name="build"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    // ReSharper disable once StringLiteralTypo
    public static ITargetDefinition EnsureRunSettingsExists<T>(this ITargetDefinition target, T build)
        where T : IHaveCodeCoverage, IComprehendTests, IHaveSolution => target.Executes(
        async () =>
        {
            var runsettings = build.RunSettings;
            // ReSharper disable once IdentifierTypo
            // ReSharper disable once StringLiteralTypo
            if (!runsettings.FileExists())
            {
                // ReSharper disable once StringLiteralTypo
                runsettings = NukeBuild.TemporaryDirectory / "default.runsettings";
                await using var tempFile = File.Open(runsettings, runsettings.FileExists() ? FileMode.Truncate : FileMode.CreateNew);
                await typeof(ICanTestWithDotNetCore)
                     .Assembly
                      // ReSharper disable once NullableWarningSuppressionIsUsed
                     .GetManifestResourceStream("Rocket.Surgery.Nuke.default.runsettings")!.CopyToAsync(tempFile);
            }

            var projects = build
                          .Solution.AnalyzeAllProjects()
                          .ToImmutableArray();
            var includeNames = projects
                              .Select(z => z.GetProperty("AssemblyName") ?? "")
                              .Where(z => !string.IsNullOrWhiteSpace(z))
                              .Distinct()
                              .Select(z => ( z + ".dll" ).Replace(".", "\\."));
            var excludePackages = projects
                                 .SelectMany(z => z.PackageReferences)
                                 .Select(z => z.Name)
                                 .Where(z => !string.IsNullOrWhiteSpace(z))
                                 .Distinct()
                                 .Select(z => ( z + ".dll" ).Replace(".", "\\."));

            ManageRunSettings(
                build,
                runsettings,
                (
                    build.IncludeModulePaths.Union(includeNames),
                    build.ExcludeModulePaths.Union(excludePackages)
                ),
                ( build.IncludeAttributes, build.ExcludeAttributes ),
                ( build.IncludeNamespaces, build.ExcludeNamespaces ),
                ( build.IncludeSources, build.ExcludeSources )
            );
        }
    );

    private static void AddItems(XElement parent, string parentName, string childName, (IEnumerable<string> include, IEnumerable<string> exclude) values)
    {
        var parentElement = EnsureElement(parent, parentName);
        var include = EnsureElement(parentElement, "Include");
        var exclude = EnsureElement(parentElement, "Exclude");

        include.RemoveAll();
        exclude.RemoveAll();

        foreach (var value in values.include)
        {
            include.Add(new XElement(childName, value));
        }

        foreach (var value in values.exclude)
        {
            exclude.Add(new XElement(childName, value));
        }
    }

    private static void DistinctAndOrganize(XElement parent, string parentName)
    {
        if (parent.Element(parentName) is not { } item) return;
        if (item.Element("Include") is { } include)
        {
            var values = include.Elements().DistinctBy(z => z.Value).OrderBy(x => x.Value).ToArray();
            include.RemoveAll();
            include.Add([.. values]);
            if (!include.Elements().Any()) include.Remove();
        }

        if (item.Element("Exclude") is { } exclude)
        {
            var values = exclude.Elements().DistinctBy(z => z.Value).OrderBy(x => x.Value).ToArray();
            exclude.RemoveAll();
            exclude.Add([.. values]);
            if (!exclude.Elements().Any()) exclude.Remove();
        }

        if (!item.Elements().Any()) item.Remove();
    }

    private static XElement EnsureElement(XElement parent, string name)
    {
        var element = parent.Element(name);
        if (element is null)
        {
            element = new(name);
            parent.Add(element);
        }

        return element;
    }

    private static void ManageRunSettings<T>(
        T build,
        AbsolutePath runsettingsPath,
        (IEnumerable<string> include, IEnumerable<string> exclude) modulePaths,
        (IEnumerable<string> include, IEnumerable<string> exclude) attributes,
        (IEnumerable<string> include, IEnumerable<string> exclude) namespaces,
        (IEnumerable<string> include, IEnumerable<string> exclude) sources
    )
        where T : IHaveCodeCoverage, IComprehendTests
    {
        var doc = XDocument.Load(runsettingsPath);

        var dataCollector = EnsureElement(doc.Root, "DataCollectionRunSettings")
                           .Element("DataCollectors")
                          ?.Element("DataCollector")
         ?? throw new InvalidOperationException("DataCollector element is missing in the runsettings file.");
        var codeCoverage = EnsureElement(dataCollector, "Configuration")
           .Element("CodeCoverage");

        if (codeCoverage is null)
        {
            codeCoverage = new("CodeCoverage");
            dataCollector.Element("Configuration")?.Add(codeCoverage);
        }

        AddItems(codeCoverage, "Attributes", "Attribute", transform(attributes, transformAttribute));
        AddItems(codeCoverage, "Functions", "Function", transform(namespaces, transformNamespace));
        AddItems(codeCoverage, "ModulePaths", "ModulePath", transform(modulePaths, transformModulePath));
        AddItems(codeCoverage, "Sources", "Source", sources);

        build.CustomizeCoverageRunSettings(doc);

        DistinctAndOrganize(codeCoverage, "Attributes");
        DistinctAndOrganize(codeCoverage, "Functions");
        DistinctAndOrganize(codeCoverage, "ModulePaths");
        DistinctAndOrganize(codeCoverage, "Sources");

        doc.Save(runsettingsPath);

        static (IEnumerable<string> include, IEnumerable<string> exclude) transform(
            (IEnumerable<string> include, IEnumerable<string> exclude) attributes,
            Func<string, IEnumerable<string>> transformer
        )
        {
            return ( attributes.include.SelectMany(transformer), attributes.exclude.SelectMany(transformer) );
        }

        static IEnumerable<string> transformAttribute(string attr)
        {
            return [$"^{attr.Replace(".", "\\.")}$"];
        }

        static IEnumerable<string> transformModulePath(string ns)
        {
            return [$".*{ns}"];
        }

        static IEnumerable<string> transformNamespace(string ns)
        {
            return [$"^{ns.Replace(".", "\\.")}.*"];
        }
    }
}
