using Newtonsoft.Json.Linq;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Custom msbuild helper extensions
/// </summary>
public static class ToolSettingsExtensions
{
    /// <summary>
    ///     <para>Call a target definition in context of a build script</para>
    ///     <para>if the build scripts aren't setup correctly the nuke extensions will not detect them.</para>
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="target">The target.</param>
    /// <param name="func">The function.</param>
    /// <param name="value">The value.</param>
    /// <returns>The target definition.</returns>
    [Obsolete("use With(T value, Func<ITargetDefinition, T, ITargetDefinition> func) version")]
    public static ITargetDefinition With<T>(
        this ITargetDefinition target,
        Func<ITargetDefinition, T, ITargetDefinition> func,
        T value
    ) =>
        func(target, value);

    /// <summary>
    ///     <para>Call a target definition in context of a build script</para>
    ///     <para>if the build scripts aren't setup correctly the nuke extensions will not detect them.</para>
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="target">The target.</param>
    /// <param name="value">The value.</param>
    /// <param name="func">The function.</param>
    /// <returns>The target definition.</returns>
    public static ITargetDefinition With<T>(
        this ITargetDefinition target,
        T value,
        Func<ITargetDefinition, T, ITargetDefinition> func
    ) =>
        func(target, value);

    /// <summary>
    ///     Configures binary and file logging for MSBuild
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="path"></param>
    /// <param name="verbosity"></param>
    public static T SetDefaultLoggers<T>(this T settings, AbsolutePath path, MSBuildVerbosity? verbosity = null)
        where T : ToolSettings
    {
        if (NukeBuild.IsLocalBuild)
        {
            var existingArgs = settings.ProcessArgumentConfigurator;
            settings = settings.SetProcessArgumentConfigurator(args => existingArgs(args).Add("/tl"));
        }
        return settings
              .SetBinaryLogger((AbsolutePath)Path.ChangeExtension(path, "binlog"))
              .SetFileLogger((AbsolutePath)Path.ChangeExtension(path, "log"), verbosity);
    }

    /// <summary>
    ///     Configures binary logging for MSBuild
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="path"></param>
    public static T SetBinaryLogger<T>(this T settings, AbsolutePath path)
        where T : ToolSettings => SetBinaryLogger(settings, path, NukeBuild.IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed);

    /// <summary>
    ///     Configures binary logging for MSBuild
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="path"></param>
    /// <param name="imports"></param>
    public static T SetBinaryLogger<T>(this T settings, AbsolutePath path, MSBuildBinaryLogImports imports)
        where T : ToolSettings
    {
        var existingArgs = settings.ProcessArgumentConfigurator;
        return settings.SetProcessArgumentConfigurator(
            args =>
                existingArgs(args).Add($"/bl:\"{path}\";ProjectImports={imports}")
        );
    }

    /// <summary>
    ///     Configures a file logger for MSBuild
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="path"></param>
    /// <param name="verbosity"></param>
    public static T SetFileLogger<T>(this T settings, AbsolutePath path, MSBuildVerbosity? verbosity = null)
        where T : ToolSettings
    {
        var existingArgs = settings.ProcessArgumentConfigurator;
        verbosity ??= NukeBuild.Verbosity.MapVerbosity(MSBuildVerbosity.Normal);

        return settings.SetProcessArgumentConfigurator(
            args =>
                existingArgs(args)
                   .Add(
                        $"/fileLogger /fileloggerparameters:ShowTimestamp;Verbosity={verbosity};LogFile=\"{path}\""
                    )
        );
    }

    /// <summary>
    ///     Ensures all the gitversion values are available as environment values (GITVERISON_*)
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="gitVersion"></param>
    public static T SetGitVersionEnvironment<T>(this T settings, GitVersion? gitVersion)
        where T : ToolSettings
    {
        if (gitVersion == null) return settings;

        foreach (var item in JObject.FromObject(gitVersion))
        {
            var key = $"gitversion_{item.Key}".ToUpperInvariant();
            if (settings.ProcessEnvironmentVariables.TryGetValue(key, out _)) continue;

            settings = settings.AddProcessEnvironmentVariable(key, item.Value?.ToString());
        }

        return settings;
    }
}
