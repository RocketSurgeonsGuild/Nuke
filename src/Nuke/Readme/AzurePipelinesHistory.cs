namespace Rocket.Surgery.Nuke.Readme;

internal class AzurePipelinesHistory : IHistorySection
{
    public (string badge, string history) Process(
        IDictionary<object, object?> config,
        IMarkdownReferences references,
        IHaveSolution build
    )
    {
        var url = references.AddReference(
            "azurepipelines",
            $"https://dev.azure.com/{config["account"]}/{config["teamproject"]}/_build/latest?definitionId={config["builddefinition"]}&branchName=master"
        );
        var badge = references.AddReference(
            "azurepipelines-badge",
            $"https://img.shields.io/azure-devops/build/{config["account"]}/{config["teamproject"]}/{config["builddefinition"]}.svg?color=98C6FF&label=azure%20pipelines&logo=azuredevops&logoColor=98C6FF&style=flat",
            "Azure Pipelines Status"
        );
        var historyUrl = references.AddReference(
            "azurepipelines-history",
            $"https://dev.azure.com/{config["account"]}/{config["teamproject"]}/_build?definitionId={config["builddefinition"]}&branchName=master"
        );
        var historyBadge = references.AddReference(
            "azurepipelines-history-badge",
            $"https://buildstats.info/azurepipelines/chart/{config["account"]}/{config["teamproject"]}/{config["builddefinition"]}?includeBuildsFromPullRequest=false",
            "Azure Pipelines History"
        );

        return ($"[!{badge}]{url}", $"[!{historyBadge}]{historyUrl}");
    }

    public string ConfigKey { get; } = "azurepipelines";
    public string Name { get; } = "Azure Pipelines";
}
