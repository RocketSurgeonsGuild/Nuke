using System.Xml.Linq;
using Nuke.Common.IO;
using Rocket.Surgery.Nuke.DotNetCore;

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
        where T : IHaveCodeCoverage, IComprehendTests
    {
        return target.Executes(
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

                ManageRunSettings(
                    runsettings,
                    build.IncludeModulePaths,
                    build.ExcludeModulePaths,
                    build.IncludeAttributes,
                    build.ExcludeAttributes,
                    build.IncludeNamespaces,
                    build.ExcludeNamespaces
                );
            }
        );
    }

    private static void ManageRunSettings(
        AbsolutePath runsettingsPath,
        IEnumerable<string> includeModulePaths,
        IEnumerable<string> excludeModulePaths,
        IEnumerable<string> includeAttributes,
        IEnumerable<string> excludeAttributes,
        IEnumerable<string> includeNamespaces,
        IEnumerable<string> excludeNamespaces
    )
    {
        var doc = XDocument.Load(runsettingsPath);

        var dataCollector = EnsureElement(doc.Root, "DataCollectionRunSettings")
                           .Element("DataCollectors")
                          ?.Element("DataCollector");

        if (dataCollector == null)
        {
            throw new InvalidOperationException("DataCollector element is missing in the runsettings file.");
        }

        var codeCoverage = EnsureElement(dataCollector, "Configuration")
           .Element("CodeCoverage");

        if (codeCoverage == null)
        {
            codeCoverage = new("CodeCoverage");
            dataCollector.Element("Configuration")?.Add(codeCoverage);
        }

        AddIncludeItems(codeCoverage, "ModulePaths", "ModulePath", includeModulePaths);
        AddExcludeItems(codeCoverage, "ModulePaths", "ModulePath", excludeModulePaths);
        AddIncludeItems(codeCoverage, "Attributes", "Attribute", includeAttributes.Select(TransformAttribute));
        AddExcludeItems(codeCoverage, "Attributes", "Attribute", excludeAttributes.Select(TransformAttribute));
        AddIncludeItems(codeCoverage, "Functions", "Function", includeNamespaces.Select(TransformNamespace));
        AddExcludeItems(codeCoverage, "Functions", "Function", excludeNamespaces.Select(TransformNamespace));

        DistinctAndOrganize(codeCoverage, "ModulePaths");
        DistinctAndOrganize(codeCoverage, "Attributes");
        DistinctAndOrganize(codeCoverage, "Functions");

        doc.Save(runsettingsPath);

        static string TransformAttribute(string ns)
        {
            return $"^{ns.Replace(".", "\\.")}$";
        }

        static string TransformNamespace(string ns)
        {
            return $"^{ns.Replace(".", "\\.")}.*";
        }
    }

    private static XElement EnsureElement(XElement parent, string name)
    {
        var element = parent.Element(name);
        if (element == null)
        {
            element = new(name);
            parent.Add(element);
        }

        return element;
    }

    private static void AddIncludeItems(XElement parent, string parentName, string childName, IEnumerable<string> values)
    {
        var parentElement = EnsureElement(parent, parentName);
        var element = EnsureElement(parentElement, "Include");

        element.RemoveAll();
        foreach (var value in values)
        {
            element.Add(new XElement(childName, value));
        }
    }

    private static void AddExcludeItems(XElement parent, string parentName, string childName, IEnumerable<string> values)
    {
        var parentElement = EnsureElement(parent, parentName);
        var element = EnsureElement(parentElement, "Exclude");

        element.RemoveAll();
        foreach (var value in values)
        {
            element.Add(new XElement(childName, value));
        }
    }

    private static void DistinctAndOrganize(XElement parent, string parentName)
    {
        var element = EnsureElement(parent, parentName);
        if (element.Element("Include") is { } include)
        {
            var values = include.Elements().DistinctBy(z => z.Value).OrderBy(x => x.Value).ToArray();
            include.RemoveAll();
            include.Add([..values]);
            if (!include.Elements().Any()) include.Remove();
        }

        if (element.Element("Exclude") is { } exclude)
        {
            var values = exclude.Elements().DistinctBy(z => z.Value).OrderBy(x => x.Value).ToArray();
            exclude.RemoveAll();
            exclude.Add([..values]);
            if (!exclude.Elements().Any()) exclude.Remove();
        }
    }
}
