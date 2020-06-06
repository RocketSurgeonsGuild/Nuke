using System;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.ContinuousIntegration
{
    public static class AzurePipelinesExtensions
    {
        public static void SetProgress(
            this AzurePipelines self,
            int percentage,
            string currentOperation = null
        )
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
            bool? issecret = null ,
            bool? isoutput  = null ,
            bool? isreadonly = null
        )
        {
            self.WriteCommand(
                "task.setvariable",
                value,
                o => o.AddPair(nameof(variable), variable)
                   .AddPairWhenValueNotNull(nameof(issecret), issecret?.ToString().ToLower())
                   .AddPairWhenValueNotNull(nameof(isoutput), isoutput?.ToString().ToLower())
                   .AddPairWhenValueNotNull(nameof(isreadonly), isreadonly?.ToString().ToLower())
            );
        }

        public static void CompleteTimeline(
            this AzurePipelines self,
            AzurePipelinesTaskResult? result,
            string message = null
        )
        {
            self.WriteCommand(
                "task.complete",
                message,
                o => o.AddPairWhenValueNotNull(nameof(result), result?.ToString())
            );
        }

        public static AzurePipelinesTask CreateTask(
            this AzurePipelines self,
            string type,
            string name,
            int order,
            AzurePipelinesTaskState? state = null,
            int? progress = null,
            AzurePipelinesTask? parent = null,
            string message = null
        )
        {
            var task = new AzurePipelinesTask();
            self.WriteCommand(
                "task.logdetail",
                message,
                o => o
                   .AddPair("id", task.Id)
                   .AddPairWhenValueNotNull("parentid", parent?.Id)
                   .AddPairWhenValueNotNull(nameof(type), type)
                   .AddPairWhenValueNotNull(nameof(name), name)
                   .AddPairWhenValueNotNull(nameof(order), Math.Max(1, order))
                   .AddPairWhenValueNotNull(nameof(state), state?.ToString())
                   .AddPairWhenValueNotNull(nameof(progress), progress)
            );
            return task;
        }

        public static AzurePipelinesTask UpdateTask(
            this AzurePipelines self,
            AzurePipelinesTask task,
            int? progress = null,
            AzurePipelinesTaskState? state = null,
            string message = null
        )
        {
            self.WriteCommand(
                "task.logdetail",
                message,
                o => o
                   .AddPair("id", task.Id)
                   .AddPairWhenValueNotNull(nameof(state), state?.ToString())
                   .AddPairWhenValueNotNull(nameof(progress), progress)
            );
            return task;
        }

        public static AzurePipelinesTask CompleteTask(
            this AzurePipelines self,
            AzurePipelinesTask task,
            AzurePipelinesTaskResult result,
            string message = null
        )
        {
            self.WriteCommand(
                "task.logdetail",
                message,
                o => o
                   .AddPair("id", task.Id)
                   .AddPairWhenValueNotNull("progress", 100)
                   .AddPairWhenValueNotNull(nameof(result), result?.ToString())
            );
            return task;
        }
    }
}