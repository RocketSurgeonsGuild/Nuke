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

    internal void Run(string title, string output)
    {
        var report = GenerateReport(GenerateLibraryParameters(title));
        if (string.IsNullOrEmpty(report))
        {
            _logger.Error("Error, report generated no content");
            return;
        }

        var saved = SaveReport(report, output);
        if (!saved)
        {
            _logger.Error("Error, report unable to be saved");
        }
    }

    private bool SaveReport(string report, string location)
    {
        try
        {
            File.WriteAllText(location, report);
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.Error(e.Message);
        }
        catch (Exception e)
        {
            _logger.Error("Unexpected error occurred while saving report {Message}", e.Message);
        }

        return false;
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

        _logger.Information("Generating report");

        try
        {
            var reportGeneratorType = typeof(LibraryTestRun).Assembly.GetType("LiquidTestReports.Core.ReportGenerator")!;
            var reportGeneratorMethod = reportGeneratorType.GetMethod("GenerateReport")!;
            var reportGenerator = Activator.CreateInstance(reportGeneratorType, new LibraryTestRun { Run = run, Library = libraryDrop });

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
