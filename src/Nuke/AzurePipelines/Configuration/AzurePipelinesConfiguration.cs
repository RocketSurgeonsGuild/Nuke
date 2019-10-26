using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static Nuke.Common.IO.PathConstruction;

namespace Rocket.Surgery.Nuke.AzurePipelines.Configuration
{
    public class AzurePipelinesConfiguration : AzurePipelinesConfigurationEntity
    {
        [YamlMember(Order = 0)]
        public AzurePipelinesStage[] Stages { get; set; }
        public AzurePipelinesJob[] Jobs { get; set; }
        // public AzurePipeliness[] Jobs { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            using (writer.WriteBlock("stages:"))
            {
                Stages.ForEach(x => x.Write(writer));
            }
        }
    }
#pragma warning disable CS1591
    public class Pipeline
    {
        [YamlMember(Alias = "name")]
        public string? BuildNumberingFormat { get; set; }

        public Resources? Resources { get; set; }

        public IList<Variable> Variables { get; set; } = new List<Variable>();

        public Trigger? Trigger { get; set; }

        [YamlMember(Alias = "pr")]
        public PullRequest? PullRequest { get; set; }

        public IList<Stage> Stages { get; set; }
        public IList<Job> Jobs { get; set; }
        public IList<Step> Steps { get; set; }

    }

    public class Resources
    {
        public IList<Container> Containers { get; set; } = new List<Container>();
        public IList<Repository> Repositories { get; set; } = new List<Repository>();
    }

    public class Container
    {
        [YamlMember(Alias = "container")]
        public string Name { get; set; }
        public string? Image { get; set; }
        public string? Options { get; set; }
        public string? Endpoint { get; set; }
        [YamlMember(Alias = "env")]
        public IDictionary<string, string> Environment { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public ISet<string> Ports { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public ISet<string> Volumes { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public class ContainerReference
    {
        public string Name { get; set; }
        public static implicit operator ContainerReference(Container container)
        {
            return new ContainerReference()
            {
                Name = container.Name
            };
        }

        public static implicit operator string(ContainerReference container)
        {
            return container.Name;
        }

        public static implicit operator ContainerReference(string container)
        {
            return new ContainerReference()
            {
                Name = container
            };
        }
    }

    public enum RepositoryType
    {
        [EnumMember(Value = "git")]
        Git,
        [EnumMember(Value = "github")]
        GitHub
    }
    public class Repository
    {
        [YamlMember(Alias = "repository")]
        public string Identifier { get; set; }
        public string Name { get; set; }
        public RepositoryType Type { get; set; }
        public string? Ref { get; set; }
        public string? Endpoint { get; set; }
    }

    public class RepositoryReference
    {
        [YamlMember(Alias = "repository")]
        public string Identifier { get; set; }

        public static implicit operator string(RepositoryReference repository)
        {
            return repository.Identifier;
        }

        public static implicit operator RepositoryReference(string identifier)
        {
            return new RepositoryReference()
            {
                Identifier = identifier
            };
        }

        public static implicit operator RepositoryReference(Repository repository)
        {
            return new RepositoryReference()
            {
                Identifier = repository.Identifier
            };
        }
    }

    public class TemplateReferenceConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return typeof(TemplateReference) == type;
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var templateReference = value as TemplateReference;
            if (templateReference!.Repository != null)
            {
                emitter.Emit(new Scalar(null, null, $"{templateReference.Template}@{templateReference.Repository}"));
            }
            else
            {
                emitter.Emit(new Scalar(null, null, templateReference.Template, ScalarStyle.SingleQuoted, true, true));
            }
        }
    }

    public class VmImageConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return typeof(VmImage) == type;
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var vmImage = value as VmImage;
            emitter.Emit(new Scalar(null, null, vmImage.ImageName, ScalarStyle.SingleQuoted, true, true));
        }
    }

    public class StageConverter : IYamlTypeConverter
    {
        public static IValueSerializer Serializer = new SerializerBuilder()
            .WithTypeConverter(new TemplateReferenceConverter())
            .WithTypeConverter(new JobConverter())
            .WithTypeConverter(new StepConverter())
            .WithTypeConverter(new VariableConverter())
            .WithTypeConverter(new VmImageConverter())
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .BuildValueSerializer();

        public bool Accepts(Type type)
        {
            return typeof(Stage) == type;
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var stage = value as Stage;
            if (string.IsNullOrWhiteSpace(stage.Name))
            {
                throw new NotSupportedException("Stages must have a name!");
            }
            if (!stage.DependsOn.Any()) stage.DependsOn = null!;
            if (!stage.Variables.Any()) stage.Variables = null!;
            if (stage.Template != null)
            {
                if (stage!.Repository != null)
                {
                    emitter.Emit(new Scalar(null, null, $"{stage.Template}@{stage.Repository}"));
                }
                else
                {
                    emitter.Emit(new Scalar(null, null, stage.Template, ScalarStyle.SingleQuoted, true, true));
                }
            }
            else
            {
                Serializer.SerializeValue(emitter, value, type);
            }
        }
    }

    public class JobConverter : IYamlTypeConverter
    {
        public static IValueSerializer Serializer = new SerializerBuilder()
            .WithTypeConverter(new TemplateReferenceConverter())
            .WithTypeConverter(new StepConverter())
            .WithTypeConverter(new VariableConverter())
            .WithTypeConverter(new VmImageConverter())
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .BuildValueSerializer();

        public bool Accepts(Type type)
        {
            return typeof(Job) == type;
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var job = value as Job;
            if (string.IsNullOrWhiteSpace(job.Name))
            {
                throw new NotSupportedException("Jobs must have a name!");
            }
            if (!job.DependsOn.Any()) job.DependsOn = null;
            if (!job.Variables.Any()) job.Variables = null;
            if (!job.Services.Any()) job.Services = null;
            if (job.Template != null)
            {
                if (job!.Repository != null)
                {
                    emitter.Emit(new Scalar(null, null, $"{job.Template}@{job.Repository}"));
                }
                else
                {
                    emitter.Emit(new Scalar(null, null, job.Template, ScalarStyle.SingleQuoted, true, true));
                }
            }
            else
            {
                Serializer.SerializeValue(emitter, value, type);
            }
        }
    }

    public class StepConverter : IYamlTypeConverter
    {
        public static IValueSerializer Serializer = new SerializerBuilder()
            .WithTypeConverter(new TemplateReferenceConverter())
            .WithTypeConverter(new VariableConverter())
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .BuildValueSerializer();

        public bool Accepts(Type type)
        {
            return typeof(Step).IsAssignableFrom(type);
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value is ScriptStep scriptStep && scriptStep.Environment?.Any() == false)
            {
                scriptStep.Environment = null!;
            }
            Serializer.SerializeValue(emitter, value, type);
        }
    }

    public class VariableConverter : IYamlTypeConverter
    {
        public static IValueSerializer Serializer = new SerializerBuilder()
            .WithTypeConverter(new TemplateReferenceConverter())
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .BuildValueSerializer();
        public bool Accepts(Type type)
        {
            return typeof(Variable) == type;
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            var variable = value as Variable;
            if (variable.Template != null)
            {
                if (variable!.Repository != null)
                {
                    emitter.Emit(new MappingStart(null, null, true, MappingStyle.Block));
                    emitter.Emit(new Scalar(null, null, "template", ScalarStyle.Plain, true, false));
                    emitter.Emit(new Scalar(null, null, $"{variable.Template}@{variable.Repository.Identifier}", ScalarStyle.SingleQuoted, true, false));
                    emitter.Emit(new MappingEnd());
                }
                else
                {
                    emitter.Emit(new MappingStart(null, null, true, MappingStyle.Block));
                    emitter.Emit(new Scalar(null, null, "template", ScalarStyle.Plain, true, false));
                    emitter.Emit(new Scalar(null, null, variable.Template, ScalarStyle.SingleQuoted, true, false));
                    emitter.Emit(new MappingEnd());
                }
            }
            else if (variable.Group != null)
            {
                emitter.Emit(new MappingStart(null, null, true, MappingStyle.Block));
                emitter.Emit(new Scalar(null, null, "group", ScalarStyle.Plain, true, false));
                emitter.Emit(new Scalar(null, null, variable.Group, ScalarStyle.SingleQuoted, true, false));
                emitter.Emit(new MappingEnd());
            }
            else
            {
                emitter.Emit(new MappingStart(null, null, true, MappingStyle.Block));
                // emitter.Emit(new MappingStart(null, null, true, MappingStyle.Block));
                emitter.Emit(new Scalar(null, null, "name", ScalarStyle.Plain, true, false));
                emitter.Emit(new Scalar(null, null, variable.Name, ScalarStyle.SingleQuoted, true, false));
                emitter.Emit(new Scalar(null, null, "value", ScalarStyle.Plain, true, false));
                emitter.Emit(new Scalar(null, null, variable.Value, ScalarStyle.SingleQuoted, true, false));
                // emitter.Emit(new MappingEnd());
                emitter.Emit(new MappingEnd());
            }
        }
    }

    public class TemplateReference
    {
        public string Template { get; set; }
        public RepositoryReference? Repository { get; set; }
    }

    public class Stage : TemplateReference
    {
        [YamlMember(Alias = "stage")]
        public string Name { get; set; }
        public string? DisplayName { get; set; }
        public ISet<string> DependsOn { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public string? Condition { get; set; }

        public IList<Variable> Variables { get; set; } = new List<Variable>();
        [YamlMember(Order = 0)]
        public IList<Job> Jobs { get; set; } = new List<Job>();

        public static implicit operator string(Stage stage) { return stage.Name; }
    }

    public class Job : TemplateReference
    {
        [YamlMember(Alias = "job")]
        public string Name { get; set; }
        public string? DisplayName { get; set; }
        public ISet<string> DependsOn { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public string? Condition { get; set; }
        public Strategy? Strategy { get; set; }
        public bool? ContinueOnError { get; set; }
        public Pool? Pool { get; set; }
        public Workspace? Workspace { get; set; }
        public ContainerReference? Container { get; set; }
        public int? TimeoutInMinutes { get; set; }
        public int? CancelTimeoutInMinutes { get; set; }
        public IList<Variable> Variables { get; set; } = new List<Variable>();
        public IDictionary<string, ContainerReference> Services { get; set; } = new Dictionary<string, ContainerReference>(StringComparer.OrdinalIgnoreCase);
        public IList<Step> Steps { get; set; } = new List<Step>();

        public static implicit operator string(Job job) { return job.Name; }
    }

    public abstract class Step
    {

    }

    public abstract class ScriptStep : Step
    {

        public string? DisplayName { get; set; }
        public string? Identifier { get; set; }
        public RelativePath? WorkingDirectory { get; set; }
        public bool? ContinueOnError { get; set; }
        public bool? Enabled { get; set; }
        public string? Condition { get; set; }
        public int? TimeoutInMinutes { get; set; }
        [YamlMember(Alias = "env")]
        public IDictionary<string, string> Environment { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public enum ErrorActionPreference
    {
        [EnumMember(Value = "stop")]
        Stop,
        [EnumMember(Value = "continue")]
        Continue,
        [EnumMember(Value = "silentlyContinue")]
        SilentlyContinue
    }
    public abstract class PowerShellScriptStep : ScriptStep
    {
        public bool? FailOnStderr { get; set; }
        public ErrorActionPreference? ErrorActionPreference { get; set; }
        [YamlMember(Alias = "ignoreLASTEXITCODE")]
        public bool? IgnoreLastExitCode { get; set; }
    }

    public class Bash : ScriptStep
    {
        [YamlMember(Alias = "bash")]
        public string Action { get; set; }
        public bool? FailOnStderr { get; set; }
    }
    public class Script : ScriptStep
    {
        [YamlMember(Alias = "script")]
        public string Action { get; set; }
        public bool? FailOnStderr { get; set; }
    }

    public class PowerShell : PowerShellScriptStep
    {
        [YamlMember(Alias = "powershell")]
        public string Action { get; set; }

    }
    public class Pwsh : PowerShellScriptStep
    {
        [YamlMember(Alias = "pwsh")]
        public string Action { get; set; }

    }

    public class PwshTaskInputs
    {
        public PwshTargetType? TargetType { get; set; }
        public string FilePath { get; set; }
        public string Arguments { get; set; }
        public string Script { get; set; }
        public bool? FailOnStderr { get; set; }
        public ErrorActionPreference? ErrorActionPreference { get; set; }
        [YamlMember(Alias = "ignoreLASTEXITCODE")]
        public bool? IgnoreLastExitCode { get; set; }
        public bool? Pwsh { get; set; }
    }
    public enum PwshTargetType
    {
        [EnumMember(Value = "filePath")]
        FilePath,
        [EnumMember(Value = "inline")]
        Inline
    }
    public class PwshTask : Task<PwshTaskInputs>
    {
        [YamlMember(Alias = "task")]
        public override string Name => "PowerShell@2";
    }

    public enum UseDotNetPackageType
    {
        [EnumMember(Value = "sdk")]
        Sdk,
        [EnumMember(Value = "runtime")]
        Runtime,
    }
    public class UseDotNetInputs
    {
        public UseDotNetPackageType PackageType { get; set; }
        public bool? UseGlobalJson { get; set; }
        public string? Version { get; set; }
        public bool? IncludePreviewVersions { get; set; }
        public string? InstallationPath { get; set; }
        public bool? PerformMultiLevelLookup { get; set; }
    }

    public class UseDotNet : Task<UseDotNetInputs>
    {
        [YamlMember(Alias = "task")]
        public override string Name => "UseDotNet@2";
    }
    // build, push, pack, publish, restore, run, test, custom
    public enum DotNetCoreCliCommand
    {
        [EnumMember(Value = "build")]
        Build,
        [EnumMember(Value = "push")]
        Push,
        [EnumMember(Value = "pack")]
        Pack,
        [EnumMember(Value = "publish")]
        Publish,
        [EnumMember(Value = "restore")]
        Restore,
        [EnumMember(Value = "run")]
        Run,
        [EnumMember(Value = "test")]
        Test,
        [EnumMember(Value = "custom")]
        Custom
    }

    public enum DotNetCoreCliFeeds
    {
        [EnumMember(Value = "select")]
        Select,
        [EnumMember(Value = "config")]
        Config
    }
    public enum DotNetCoreCliNuGetFeedType
    {
        [EnumMember(Value = "internal")]
        Internal,
        [EnumMember(Value = "external")]
        External
    }
    public enum DotNetCoreCliVersioningScheme
    {
        [EnumMember(Value = "off")]
        Off,
        [EnumMember(Value = "byPrereleaseNumber")]
        ByPrereleaseNumber,
        [EnumMember(Value = "byEnvVar")]
        ByEnvVar,
        [EnumMember(Value = "byBuildNumber")]
        ByBuildNumber
    }
    public enum DotNetCoreCliVerbosity
    {
        [EnumMember(Value = "config")]
        Quiet,
        [EnumMember(Value = "minimal")]
        Minimal,
        [EnumMember(Value = "normal")]
        Normal,
        [EnumMember(Value = "detailed")]
        Detailed,
        [EnumMember(Value = "diagnostic")]
        Diagnostic
    }

    public class DotNetCoreCliInputs
    {
        public DotNetCoreCliCommand? Command { get; set; }
        public bool? PublishWebProjects { get; set; }
        public string? Projects { get; set; }
        public string? Custom { get; set; }
        public string? Arguments { get; set; }
        public bool? PublishTestResults { get; set; }
        public string? TestRunTitle { get; set; }
        public bool? ZipAfterPublish { get; set; }
        public bool? ModifyOutputPath { get; set; }
        public DotNetCoreCliFeeds? FeedsToUse { get; set; }
        public string? VstsFeed { get; set; }
        public bool? IncludeNuGetOrg { get; set; }
        public RelativePath? NugetConfigPath { get; set; }
        public string? ExternalFeedCredentials { get; set; }
        public bool? NoCache { get; set; }
        public string? RestoreDirectory { get; set; }
        public DotNetVerbosity? VerbosityRestore { get; set; }
        public string? PackagesToPush { get; set; }
        public DotNetCoreCliNuGetFeedType? NuGetFeedType { get; set; }
        public string? PublishVstsFeed { get; set; }
        public bool? PublishPackageMetadata { get; set; }
        public string? PublishFeedCredentials { get; set; }
        public string? PackagesToPack { get; set; }
        public string? Configuration { get; set; }
        public string? PackDirectory { get; set; }
        public bool? Nobuild { get; set; }
        public bool? Includesymbols { get; set; }
        public bool? Includesource { get; set; }
        public DotNetCoreCliVersioningScheme? VersioningScheme { get; set; }
        public string? VersionEnvVar { get; set; }
        public char? NajorVersion { get; set; }
        public char? MinorVersion { get; set; }
        public char? PatchVersion { get; set; }
        public DotNetVerbosity? VerbosityPack { get; set; }
        public IDictionary<string, string>? BuildProperties { get; set; }
    }

    public class DotNetCoreCli : Task<DotNetCoreCliInputs>
    {
        [YamlMember(Alias = "task")]
        public override string Name => "DotNetCoreCLI@2";
    }

    public class Publish : Step
    {
        [YamlMember(Alias = "publish")]
        public RelativePath Path { get; set; }

        [YamlMember(Alias = "artifact")]
        public string Name { get; set; }
    }

    public enum DownloadType
    {

        [EnumMember(Value = "current")]
        Current,
        [EnumMember(Value = "none")]
        None,
    }

    public class Download : Step
    {

        [YamlMember(Alias = "download")]
        public DownloadType Type { get; set; }

        [YamlMember(Alias = "artifact")]
        public string? Name { get; set; }
        public ISet<string>? Patterns { get; set; }
    }

    public class Task : ScriptStep
    {
        [YamlMember(Alias = "task")]
        public string Name { get; set; }
        public IDictionary<string, string> Inputs { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public abstract class Task<T> : ScriptStep
    {
        [YamlMember(Alias = "task")]
        public abstract string Name { get; }
        public T Inputs { get; set; }
    }

    public enum CheckoutType
    {

        [EnumMember(Value = "self")]
        Self,
        [EnumMember(Value = "none")]
        None,
    }

    public enum SubmodulesType
    {

        [EnumMember(Value = "true")]
        True,
        [EnumMember(Value = "recursive")]
        Recursive,
    }

    public class Checkout : Step
    {
        [YamlMember(Alias = "checkout")]
        public Type Type { get; set; }
        public bool? Clean { get; set; }
        public int? FetchDepth { get; set; }
        public bool? Lfs { get; set; }
        public SubmodulesType? Submodules { get; set; }
        public string? Path { get; set; }
        public bool? PersistCredentials { get; set; }
    }

    public enum WorkspaceCleanType
    {
        [EnumMember(Value = "outputs")]
        Outputs,
        [EnumMember(Value = "resources")]
        Resources,
        [EnumMember(Value = "all")]
        All
    }

    public class Workspace
    {
        public WorkspaceCleanType? Clean { get; set; }
    }

    public class Strategy
    {
        public int? Parallel { get; set; }
        public IDictionary<string, IDictionary<string, string>> Matrix { get; set; } = new Dictionary<string, IDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        public int? MaxParallel { get; set; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class VmImage
    {
        public static VmImage WindowsLatest = new VmImage("Windows", "windows-latest");
        public static VmImage Windows2019 = new VmImage("Windows", "windows-2019");
        public static VmImage Vs2017Win2016 = new VmImage("Windows", "vs2017-win2016");
        public static VmImage Vs2015Win2012R2 = new VmImage("Windows", "vs2015-win2012r2");
        public static VmImage Win1803 = new VmImage("Windows", "win1803");
        public static VmImage Ubuntu1604 = new VmImage("Ubuntu", "ubuntu-16.04");
        public static VmImage Ubuntu1804 = new VmImage("Ubuntu", "ubuntu-18.04");
        public static VmImage UbuntuLatest = new VmImage("Ubuntu", "ubuntu-latest");
        public static VmImage MacOsLatest = new VmImage("macOS", "macOS-latest");
        public static VmImage MacOs1014 = new VmImage("macOS", "macOS-10.14");
        public static VmImage MacOs1013 = new VmImage("macOS", "macOS-10.13");
        public VmImage(string name, string imageName)
        {
            Name = name;
            ImageName = imageName;
        }

        public override string ToString()
        {
            return Name;
        }

        public string Name { get; }
        public string ImageName { get; }

        public static implicit operator string(VmImage image)
        {
            return image.ImageName;
        }
        public static implicit operator Pool(VmImage image)
        {
            return new Pool() { VmImage = image };
        }
    }

    public class Pool
    {
        public string? Name { get; set; }
        public VmImage? VmImage { get; set; }
        public IEnumerable<string> Demands { get; set; }
    }


    public class Variable : TemplateReference
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public string? Group { get; set; }
    }

    public class TriggerItem
    {
        public IList<string> Include { get; set; } = new List<string>();
        public IList<string> Exclude { get; set; } = new List<string>();
    }

    public class Trigger
    {
        public bool? Batch { get; set; }
        public TriggerItem? Branches { get; set; }
        public TriggerItem? Tags { get; set; }
        public TriggerItem? Paths { get; set; }
    }
    public class PullRequest
    {
        public bool? AutoCancel { get; set; }
        public TriggerItem? Branches { get; set; }
        public TriggerItem? Paths { get; set; }
    }
}
