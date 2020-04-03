using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;
using Rocket.Surgery.Nuke;
using Rocket.Surgery.Nuke.DotNetCore;
using System;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.PathConstruction;
using Rocket.Surgery.Nuke.GithubActions;
using System.Reflection;
using System.IO;

[PublicAPI]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[AzurePipelinesSteps(
    InvokedTargets = new[] { nameof(Default) },
    NonEntryTargets = new[] { nameof(BuildVersion), nameof(Generate_Code_Coverage_Reports), nameof(Default) },
    ExcludedTargets = new[] { nameof(Restore), nameof(DotnetToolRestore) },
    Parameters = new[] { nameof(CoverageDirectory), nameof(ArtifactsDirectory), nameof(Verbosity), nameof(Configuration) }
)]
[GitHubActionsSteps("ci", GitHubActionsImage.MacOsLatest, GitHubActionsImage.WindowsLatest, GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    OnPushTags = new[] { "v*" },
    OnPushBranches = new[] { "master" },
    OnPullRequestBranches = new[] { "master" },
    InvokedTargets = new[] { nameof(Default) },
    NonEntryTargets = new[] { nameof(BuildVersion), nameof(Generate_Code_Coverage_Reports), nameof(Default) },
    ExcludedTargets = new[] { nameof(Restore), nameof(DotnetToolRestore) },
    Parameters = new[] { nameof(CoverageDirectory), nameof(ArtifactsDirectory), nameof(Verbosity), nameof(Configuration) }
)]
[PackageIcon(
    "https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png"
)]
[EnsurePackageSourceHasCredentials("RocketSurgeonsGuild")]
[EnsureGitHooks(GitHook.PreCommit)]
internal class Solution : DotNetCoreBuild, IDotNetCoreBuild
{
    /// <summary>
    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>
    public static int Main() => Execute<Solution>(x => x.Default);

    private Target Default => _ => _
       .DependsOn(Restore)
       .DependsOn(Build)
       .DependsOn(Test)
       .DependsOn(Pack);

    public Target Restore => _ => _.With(this, DotNetCoreBuild.Restore);

    public Target Build => _ => _.With(this, DotNetCoreBuild.Build);

    public Target Test => _ => _.With(this, DotNetCoreBuild.Test);

    public Target Pack => _ => _.With(this, DotNetCoreBuild.Pack);
}

[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class GitHubActionsStepsAttribute : ChainedConfigurationAttributeBase
{
    private readonly string _name;
    private readonly GitHubActionsImage[] _images;

    public GitHubActionsStepsAttribute(
        string name,
        GitHubActionsImage image,
        params GitHubActionsImage[] images)
    {
        _name = name;
        _images = new[] { image }.Concat(images).ToArray();
    }

    private string ConfigurationFile => NukeBuild.RootDirectory / ".github" / "workflows" / $"{_name}.yml";

    public string[] InvokedTargets { get; set; } = new string[0];
    public string[] Parameters { get; set; } = new string[0];

    public override string IdPostfix => _name;
    public override HostType HostType => HostType.GitHubActions;
    public override IEnumerable<string> GeneratedFiles => new[] { ConfigurationFile };
    public override IEnumerable<string> RelevantTargetNames => InvokedTargets;
    // public override IEnumerable<string> IrrelevantTargetNames => new string[0];

    public GitHubActionsTrigger[] On { get; set; } = new GitHubActionsTrigger[0];
    public string[] OnPushBranches { get; set; } = new string[0];
    public string[] OnPushTags { get; set; } = new string[0];
    public string[] OnPushIncludePaths { get; set; } = new string[0];
    public string[] OnPushExcludePaths { get; set; } = new string[0];
    public string[] OnPullRequestBranches { get; set; } = new string[0];
    public string[] OnPullRequestTags { get; set; } = new string[0];
    public string[] OnPullRequestIncludePaths { get; set; } = new string[0];
    public string[] OnPullRequestExcludePaths { get; set; } = new string[0];
    public string OnCronSchedule { get; set; }

    public string[] ImportSecrets { get; set; } = new string[0];
    public string ImportGitHubTokenAs { get; set; }

    public override CustomFileWriter CreateWriter()
    {
        return new CustomFileWriter(ConfigurationFile, indentationFactor: 2, commentPrefix: "#");
    }

    public override ConfigurationEntity GetConfiguration(
        NukeBuild build,
        IReadOnlyCollection<ExecutableTarget> relevantTargets
    )
    {
        var paramList = new List<GithubActionsParameter>();
        var parameters =
            build.GetType().GetMembers(
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public |
                    BindingFlags.FlattenHierarchy
                )
               .Where(x => x.GetCustomAttribute<ParameterAttribute>() != null);
        foreach (var parameter in parameters)
        {
            if (Parameters.Any(
                z => z.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase) || z.Equals(
                    parameter.GetCustomAttribute<ParameterAttribute>().Name,
                    StringComparison.OrdinalIgnoreCase
                )
            ))
            {
                var value = parameter.GetValue(build);
                if (value is AbsolutePath)
                {
                    value = null;
                }

                paramList.Add(
                    new GithubActionsParameter()
                    {
                        Name = parameter.GetCustomAttribute<ParameterAttribute>().Name ?? parameter.Name,
                        Default = value?.ToString() ?? "",
                    }
                );
            }
        }

        var lookupTable = new LookupTable<ExecutableTarget, GitHubActionsNukeStep>();
        var steps = relevantTargets
           .Select(x => (ExecutableTarget: x, Job: GetStep(x, lookupTable)))
           .ForEachLazy(x => lookupTable.Add(x.ExecutableTarget, x.Job))
           .Select(x => x.Job).ToArray();

        return new GitHubActionsConfiguration()
        {
            Name = _name,
            ShortTriggers = On,
            DetailedTriggers = GetTriggers().ToArray(),
            Jobs = new[] {
                new GitHubActionsJob
                {
                    Name = "Build",
                    Steps = steps.ToArray(),
                    Images = _images,
                    Parameters = paramList.ToArray()
                }
            }
        };
    }

    protected virtual GitHubActionsNukeStep GetStep(
        ExecutableTarget executableTarget,
        LookupTable<ExecutableTarget, GitHubActionsNukeStep> jobs
    )
    {
        var chainLinkNames = GetInvokedTargets(executableTarget).ToArray();
        var buildcmd = Path.ChangeExtension(NukeBuild.RootDirectory.GlobFiles("build.ps1", "build.sh")
                    .Select(x => NukeBuild.RootDirectory.GetUnixRelativePathTo(x))
                    .FirstOrDefault()
                    .NotNull("Must have a build script of build.ps1 or build.sh"), ".ps1");
        return new GitHubActionsNukeStep
        {
            ScriptPath = buildcmd,
            InvokedTargets = chainLinkNames,
            Imports = GetImports().ToDictionary(x => x.key, x => x.value),
            Name = GetStepName(executableTarget.Name),
        };
    }

    // private IEnumerable<GitHubActionsStep> GetSteps(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    // {
    //     yield return new GitHubActionsUsingStep
    //     {
    //         Using = "actions/checkout@v1"
    //     };

    //     yield return new GitHubActionsRunStep
    //     {
    //         Command = $"./{BuildCmdPath} {InvokedTargets.JoinSpace()}",
    //         Imports = GetImports().ToDictionary(x => x.key, x => x.value)
    //     };

    //     var artifacts = relevantTargets
    //         .SelectMany(x => ArtifactExtensions.ArtifactProducts[x.Definition])
    //         .Select(x => (AbsolutePath)x)
    //         // TODO: https://github.com/actions/upload-artifact/issues/11
    //         .Select(x => x.DescendantsAndSelf(y => y.Parent).FirstOrDefault(y => !y.ToString().ContainsOrdinalIgnoreCase("*")))
    //         .Distinct().ToList();
    //     foreach (var artifact in artifacts)
    //     {
    //         yield return new GitHubActionsArtifactStep
    //         {
    //             Name = artifact.ToString().TrimStart(artifact.Parent.ToString()).TrimStart('/', '\\'),
    //             Path = NukeBuild.RootDirectory.GetUnixRelativePathTo(artifact)
    //         };
    //     }
    // }

    protected virtual IEnumerable<(string key, string value)> GetImports()
    {
        string GetSecretValue(string secret) => $"${{{{ secrets.{secret} }}}}";

        if (ImportGitHubTokenAs != null)
            yield return (ImportGitHubTokenAs, GetSecretValue("GITHUB_TOKEN"));

        foreach (var secret in ImportSecrets)
            yield return (secret, GetSecretValue(secret));
    }

    protected virtual IEnumerable<GitHubActionsDetailedTrigger> GetTriggers()
    {
        if (OnPushBranches.Length > 0 ||
            OnPushTags.Length > 0 ||
            OnPushIncludePaths.Length > 0 ||
            OnPushExcludePaths.Length > 0)
        {
            yield return new GitHubActionsVcsTrigger
            {
                Kind = GitHubActionsTrigger.Push,
                Branches = OnPushBranches,
                Tags = OnPushTags,
                IncludePaths = OnPushIncludePaths,
                ExcludePaths = OnPushExcludePaths
            };
        }

        if (OnPullRequestBranches.Length > 0 ||
            OnPullRequestTags.Length > 0 ||
            OnPullRequestIncludePaths.Length > 0 ||
            OnPullRequestExcludePaths.Length > 0)
        {
            yield return new GitHubActionsVcsTrigger
            {
                Kind = GitHubActionsTrigger.PullRequest,
                Branches = OnPullRequestBranches,
                Tags = OnPullRequestTags,
                IncludePaths = OnPullRequestIncludePaths,
                ExcludePaths = OnPullRequestExcludePaths
            };
        }

        if (OnCronSchedule != null)
            yield return new GitHubActionsScheduledTrigger { Cron = OnCronSchedule };
    }

    private readonly Dictionary<string, string> _defaultSymbols = new Dictionary<string, string>()
    {
        ["Build"] = "⚙",
        ["Compile"] = "⚙",
        ["Test"] = "🚦",
        ["Pack"] = "📦",
        ["Restore"] = "📪",
        ["Publish"] = "🚢",
    };

    protected virtual string GetStepName(string name)
    {
        var symbol = _defaultSymbols.FirstOrDefault(z => z.Key.EndsWith(name, StringComparison.OrdinalIgnoreCase)).Value;
        if (string.IsNullOrWhiteSpace(symbol)) return name;

        return $"{symbol} {name}";
    }
    public class GitHubActionsJob : ConfigurationEntity
    {
        public GitHubActionsJob() => SetupSteps = new GitHubActionsStep[]
            {
                new GitHubActionsUsingStep
                {
                    Using = "actions/checkout@v1"
                }
            };
        public GithubActionsParameter[] Parameters { get; set; }
        public string Name { get; set; }
        public GitHubActionsImage[] Images { get; set; }
        public GitHubActionsStep[] SetupSteps { get; set; }
        public GitHubActionsNukeStep[] Steps { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"{Name}:");

            using (writer.Indent())
            {
                writer.WriteLine($"strategy:");
                using (writer.Indent())
                {
                    writer.WriteLine($"matrix:");

                    using (writer.Indent())
                    {
                        var images = string.Join(", ", Images.Select(image => image.GetValue().Replace(".", "_")));
                        writer.WriteLine($"os: [{images}]");
                    }
                }
                /*
                strategy:
                  matrix:
    node: [6, 8, 10]
                */
                // writer.WriteLine($"name: {Name}");
                writer.WriteLine($"runs-on: ${{{{ matrix.os }}}}");
                writer.WriteLine("steps:");
                using (writer.Indent())
                {
#pragma warning disable CA1308
                    var parameters = Parameters.Select(z => $"--{z.Name.ToLowerInvariant()} '${{{{ parameters.{z.Name} }}}}'")
                       .ToArray()
                       .JoinSpace();
#pragma warning restore CA1308

                    foreach (var step in Steps)
                    {
                        step.Write(writer, parameters);
                    }
                }
            }
        }
    }

    public abstract class GitHubActionsStep : ConfigurationEntity
    {
        public string Name { get; set; }
    }

    public class GitHubActionsUsingStep : GitHubActionsStep
    {
        public string Using { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"- name: {Name}");
            writer.WriteLine($"  uses: {Using}");
        }
    }
    public class GitHubActionsArtifactStep : GitHubActionsStep
    {
        public string Path { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- uses: actions/upload-artifact@v1");

            using (writer.Indent())
            {
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine($"name: {Name}");
                    writer.WriteLine($"path: {Path}");
                }
            }
        }
    }

    public class GitHubActionsNukeStep
    {
        public string Name { get; set; }
        public string ScriptPath { get; set; }
        public Dictionary<string, string> Imports { get; set; }
        public string[] InvokedTargets { get; set; }

        public void Write(CustomFileWriter writer, string parameters)
        {
            writer.WriteLine($"- name: '{Name}'");
            writer.WriteLine($"  shell: pwsh");
            writer.WriteLine($"  run: ./{ScriptPath} {InvokedTargets.JoinSpace()} --skip {parameters}".TrimEnd());

            if (Imports.Count > 0)
            {
                using (writer.Indent())
                {
                    writer.WriteLine("env:");
                    using (writer.Indent())
                    {
                        Imports.ForEach(x => writer.WriteLine($"  {x.Key}: {x.Value}"));
                    }
                }
            }
        }
    }

    public class GitHubActionsConfiguration : ConfigurationEntity
    {
        public string Name { get; set; }

        public GitHubActionsTrigger[] ShortTriggers { get; set; }
        public GitHubActionsDetailedTrigger[] DetailedTriggers { get; set; }
        public GitHubActionsJob[] Jobs { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"name: {Name}");
            writer.WriteLine();

            if (ShortTriggers.Length > 0)
                writer.WriteLine($"on: [{ShortTriggers.Select(x => x.GetValue().ToLowerInvariant()).JoinComma()}]");
            else
            {
                writer.WriteLine("on:");
                using (writer.Indent())
                {
                    DetailedTriggers.ForEach(x => x.Write(writer));
                }
            }

            writer.WriteLine();

            writer.WriteLine("jobs:");
            using (writer.Indent())
            {
                Jobs.ForEach(x => x.Write(writer));
            }
        }
    }
}
