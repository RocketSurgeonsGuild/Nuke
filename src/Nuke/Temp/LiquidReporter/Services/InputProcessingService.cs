using LiquidTestReports.Cli.adapters;
using LiquidTestReports.Core.Drops;
using LiquidTestReports.Core.Models;
using Rocket.Surgery.Nuke.Temp.LiquidReporter.Loaders;

namespace Rocket.Surgery.Nuke.Temp.LiquidReporter.Services;

/// <summary>
///     Manage loading and mapping for each test report input
/// </summary>
internal class InputProcessingService
{
    private readonly IEnumerable<string> _inputs;

    internal InputProcessingService(IEnumerable<string> inputs)
    {
        _inputs = inputs;
    }

    internal TestRunDrop Process()
    {
        var testRunDrop = new TestRunDrop
        {
            ResultSets = new TestResultSetDropCollection(),
            TestRunStatistics = new TestRunStatisticsDrop(),
        };

        foreach (var input in _inputs)
        {
            var results = TrxLoader.FromFile(input);
            TrxMapper.Map(results, testRunDrop, new Input(new[] { input }, Constants.DefaultTitle, ""));
        }

        return testRunDrop;
    }

    private class Input : IReportInput
    {
        public Input(IEnumerable<string> files, string groupTitle, string testSuffix)
        {
            Files = files.Select(z => new FileInfo(z)).ToArray();
            GroupTitle = groupTitle;
            TestSuffix = testSuffix;
        }

        public IEnumerable<FileInfo> Files { get; }
        public InputFormatType Format => InputFormatType.Trx;
        public string GroupTitle { get; }
        public string TestSuffix { get; }
    }
}
