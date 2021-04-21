using System;
using System.Collections.Generic;
using System.IO;
using Nuke.Common;
using Nuke.Common.Execution;
using Rocket.Surgery.Nuke.Readme;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Ensures that the given git hooks are defined in the .git directory
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class EnsureReadmeIsUpdatedAttribute : BuildExtensionAttributeBase, IOnBuildInitialized
    {
        public EnsureReadmeIsUpdatedAttribute(string fileName)
        {
            ReadmeFilePath = fileName;
        }

        public EnsureReadmeIsUpdatedAttribute()
        {
            ReadmeFilePath = "Readme.md";
        }

        public string ReadmeFilePath { get; set; }

        public void OnBuildInitialized(NukeBuild build, IReadOnlyCollection<ExecutableTarget> executableTargets, IReadOnlyCollection<ExecutableTarget> executionPlan)
        {
            if (NukeBuild.IsLocalBuild && build is IHaveSolution buildSolution)
            {
                using var block = Logger.Block("Update Readme");

                var readmeContent = File.ReadAllText(NukeBuild.RootDirectory / ReadmeFilePath);
                readmeContent = new ReadmeUpdater().Process(readmeContent, buildSolution);
                File.WriteAllText(NukeBuild.RootDirectory / ReadmeFilePath, readmeContent);
            }
        }
    }
}