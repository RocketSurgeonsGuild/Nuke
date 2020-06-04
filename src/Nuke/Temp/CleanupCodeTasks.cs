// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

// ReSharper disable once CheckNamespace
namespace Temp.CleanupCode
{
    public static partial class CleanupCodeTasks
    {
        private static string GetPackageExecutable()
            => EnvironmentInfo.Is64Bit ? "cleanupcode.exe" : "cleanupcode.x86.exe";

        private static void PreProcess(ref CleanupCodeSettings toolSettings)
        {
            var installedPlugins = GetInstalledPlugins();
            if (installedPlugins.Count == 0 && toolSettings.Extensions.Count == 0)
            {
                return;
            }

            var shadowDirectory = GetShadowDirectory(installedPlugins);

            FileSystemTasks.CopyDirectoryRecursively(
                Path.GetDirectoryName(toolSettings.ToolPath).NotNull(),
                shadowDirectory,
                DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer
            );

            installedPlugins.Select(x => x.FileName)
               .ForEach(x => File.Copy(x, Path.Combine(shadowDirectory, Path.GetFileName(x).NotNull()), true));

            toolSettings.Extensions.ForEach(
                x => HttpTasks.HttpDownloadFile(
                    $"https://resharper-plugins.jetbrains.com/api/v2/package/{x}",
                    Path.Combine(shadowDirectory, $"{x}.nupkg")
                )
            );
        }

        [CanBeNull]
        private static IProcess StartProcess(CleanupCodeSettings toolSettings)
        {
            var installedPackages = GetInstalledPlugins();
            if (toolSettings.Extensions.Count > 0 || installedPackages.Count > 0)
            {
                toolSettings = toolSettings.SetToolPath(
                    Path.Combine(
                        GetShadowDirectory(installedPackages),
                        GetPackageExecutable()
                    )
                );
            }

            return ProcessTasks.StartProcess(toolSettings);
        }

        // TODO [3]: validation of wave version?

        private static IReadOnlyCollection<NuGetPackageResolver.InstalledPackage> GetInstalledPlugins()
            => NuGetPackageResolver.GetLocalInstalledPackages(ToolPathResolver.NuGetPackagesConfigFile)
               .Where(x => x.Metadata.GetDependencyGroups().SelectMany(y => y.Packages).Any(y => y.Id == "Wave"))
               .ToList();

        private static string GetShadowDirectory(
            IReadOnlyCollection<NuGetPackageResolver.InstalledPackage> installedPlugins
        )
        {
            var hashCode = installedPlugins.Select(x => x.Id).OrderBy(x => x).JoinComma().GetMD5Hash();
            return Path.Combine(NukeBuild.TemporaryDirectory, $"CleanupCode-{hashCode.Substring(0, 4)}");
        }
    }
}