using System;
using System.IO;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.ContinuousIntegration
{
    public static class AzurePipelinesExtensions
    {
        public static void SetProgress(this AzurePipelines self, int percentage, string currentOperation = null)
        {
            self.WriteCommand(
                "task.setprogress",
                currentOperation,
                o => o.AddPair("value", percentage)
            );
        }

        public static void SetVariable(
            this AzurePipelines self,
            string variable,
            string value,
            bool? isSecret = null,
            bool? isOutput = null,
            bool? isReadOnly = null
        )
        {
            self.WriteCommand(
                "task.setvariable",
                value,
                o => o.AddPair(nameof(variable), variable)
                   .AddPairWhenValueNotNull(nameof(isSecret).ToLower(), isSecret?.ToString().ToLower())
                   .AddPairWhenValueNotNull(nameof(isOutput).ToLower(), isOutput?.ToString().ToLower())
                   .AddPairWhenValueNotNull(nameof(isReadOnly).ToLower(), isReadOnly?.ToString().ToLower())
            );
        }

        public static void UploadSummary(this AzurePipelines self, AbsolutePath path)
        {
            self.WriteCommand("task.uploadsummary", path);
        }

        public static void UploadFile(this AzurePipelines self, AbsolutePath path)
        {
            self.WriteCommand("task.uploadfile", path);
        }

        public static void PrependPath(this AzurePipelines self, AbsolutePath path)
        {
            self.WriteCommand("task.prependpath", path);
        }

        public static void AssociateArtifact(
            this AzurePipelines self,
            string artifactName,
            ArtifactType artifactType,
            string path
        )
        {
            self.WriteCommand(
                "artifact.associate",
                path,
                x => x
                   .AddPair(nameof(artifactName).ToLower(), artifactName)
                   .AddPair("type", artifactType.ToString().ToLower())
            );
        }

        public static void UploadArtifact(
            this AzurePipelines self,
            AbsolutePath packageDirectory,
            string containerFolder = null,
            string artifactName = null
        )
        {
            self.WriteCommand(
                "artifact.upload",
                packageDirectory,
                dictionaryConfigurator: x => x
                   .AddPair(nameof(containerFolder).ToLower(), containerFolder ?? Path.GetDirectoryName(packageDirectory))
                   .AddPair(nameof(artifactName).ToLower(), artifactName)
            );
        }
    }

    public enum ArtifactType
    {
        container,
        filepath,
        versioncontrol,
        gitref,
        tfvclabel
    }
}