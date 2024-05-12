using System.Globalization;
using System.Reflection;
using DotLiquid;
using DotLiquid.Exceptions;
using LiquidTestReports.Core.Drops;
using Rocket.Surgery.Nuke.Temp.LiquidReporter.Services;
using Serilog;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

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
            return string.Empty;
        }

        var report = string.Empty;

        try
        {
            _logger.Information("Generating report");
            // ReSharper disable once NullableWarningSuppressionIsUsed
            var reportGeneratorType = typeof(LibraryTestRun).Assembly.GetType("LiquidTestReports.Core.ReportGenerator")!;
            // ReSharper disable once NullableWarningSuppressionIsUsed
            var reportGeneratorMethod = reportGeneratorType.GetMethod("GenerateReport", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var reportGenerator = Activator.CreateInstance(
                reportGeneratorType, BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.NonPublic, null,
                new object[] { new LibraryTestRun { Run = run, Library = libraryDrop } }, CultureInfo.InvariantCulture
            );
            // ReSharper disable once NullableWarningSuppressionIsUsed
            using var stream = GetType().Assembly.GetManifestResourceStream("MdMultiReport.md")!;
            using var template = new StreamReader(stream);
            var parameters = new object?[] { template.ReadToEnd(), null };
            // ReSharper disable once NullableWarningSuppressionIsUsed
            report = (string)reportGeneratorMethod.Invoke(reportGenerator, parameters)!;
            // ReSharper disable once NullableWarningSuppressionIsUsed
            var errors = (IList<Exception>)parameters[1]!;
            foreach (var error in errors)
            {
                _logger.Verbose(error.Message);
            }

            _logger.Information("Finished generating report");
        }
        catch (SyntaxException e)
        {
            _logger.Error(e.Message);
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
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
