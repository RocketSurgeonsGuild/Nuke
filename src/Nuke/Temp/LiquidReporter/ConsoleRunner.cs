using System.Globalization;
using System.Reflection;
using DotLiquid;
using DotLiquid.Exceptions;
using LiquidTestReports.Core.Drops;
using Rocket.Surgery.Nuke.Temp.LiquidReporter.Services;
using Serilog;

namespace Rocket.Surgery.Nuke.Temp.LiquidReporter;

/// <summary>
///     Runs report generation process with console output.
/// </summary>
internal class LiquidReporter
{
    private readonly string[] _inputs;
    private readonly ILogger _logger;

    internal LiquidReporter(IEnumerable<string> inputs, ILogger logger)
    {
        _inputs = inputs.ToArray();
        _logger = logger;
    }

    internal string Run(string title)
    {
        return GenerateReport(GenerateLibraryParameters(title));
    }

    private string GenerateReport(LibraryDrop libraryDrop)
    {
        var inputProcessor = new InputProcessingService(_inputs);
        TestRunDrop run;
        try
        {
            run = inputProcessor.Process();
        }
        catch (InvalidDataException e)
        {
            _logger.Error(e.Message);
            return null;
        }

        string report = null;

        try
        {
            _logger.Information("Generating report");
            var reportGeneratorType = typeof(LibraryTestRun).Assembly.GetType("LiquidTestReports.Core.ReportGenerator")!;
            var reportGeneratorMethod = reportGeneratorType.GetMethod("GenerateReport", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var reportGenerator = Activator.CreateInstance(
                reportGeneratorType, BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.NonPublic, null,
                new object[] { new LibraryTestRun { Run = run, Library = libraryDrop } }, CultureInfo.InvariantCulture
            );
            using var stream = GetType().Assembly.GetManifestResourceStream("MdMultiReport.md")!;
            using var template = new StreamReader(stream);
            var parameters = new object?[] { template.ReadToEnd(), null };
            reportGeneratorMethod.Invoke(reportGenerator, parameters);
            var errors = (IList<Exception>)parameters[1]!;
            foreach (var error in errors)
                _logger.Error(error.Message);
            _logger.Information("Finished generating report");
        }
        catch (SyntaxException e)
        {
            _logger.Error(e.Message);
        }
        catch (Exception e)
        {
            _logger.Error("Unexpected error occurred while generating report {Message}", e.Message);
        }

        return report;
    }

    private static LibraryDrop GenerateLibraryParameters(string title)
    {
        return new LibraryDrop
        {
            Parameters = new Dictionary<string, object>(Template.NamingConvention.StringComparer)
            {
                { Constants.TitleKey, title }
            }
        };
    }
}
