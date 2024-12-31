using System.Text.RegularExpressions;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Symbol store
/// </summary>
public static partial class Symbols
{
    /// <summary>
    ///     Add the symbol from the given step
    /// </summary>
    /// <param name="key"></param>
    /// <param name="symbol"></param>
    public static void AddSymbol(Regex key, string symbol) => DefaultSymbols.Add(key, symbol);

    /// <summary>
    ///     Configure the step name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string StepName(string name)
    {
        var symbol = DefaultSymbols.FirstOrDefault(z => z.Key.IsMatch(name)).Value;
        return string.IsNullOrWhiteSpace(symbol) ? name : $"{symbol} {name}";
    }

    /// <summary>
    ///     The default symbols
    /// </summary>
    public static readonly Dictionary<Regex, string> DefaultSymbols = new()
    {
        [MyRegex()] = "⚙️",
        [MyRegex1()] = "📦",
        [MyRegex2()] = "📫",
        [MyRegex3()] = "🔨",
        [MyRegex4()] = "📲",
        [MyRegex5()] = "🎁",
        [MyRegex2()] = "🚀",
        [MyRegex6()] = "⚒️",
        [MyRegex7()] = "🚒",
        [MyRegex8()] = "🚦",
        [MyRegex8()] = "💨",
    };

    [GeneratedRegex("(^Compile|^Build)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex MyRegex();

    [GeneratedRegex("^Pack", RegexOptions.IgnoreCase | RegexOptions.Compiled, "")]
    private static partial Regex MyRegex1();

    [GeneratedRegex("^Publish", RegexOptions.IgnoreCase | RegexOptions.Compiled, "")]
    private static partial Regex MyRegex2();

    [GeneratedRegex("^Use", RegexOptions.IgnoreCase | RegexOptions.Compiled, "")]
    private static partial Regex MyRegex3();

    [GeneratedRegex("^Install", RegexOptions.IgnoreCase | RegexOptions.Compiled, "")]
    private static partial Regex MyRegex4();

    [GeneratedRegex("^Restore", RegexOptions.IgnoreCase | RegexOptions.Compiled, "")]
    private static partial Regex MyRegex5();

    [GeneratedRegex(".*?Tool.*?", RegexOptions.IgnoreCase | RegexOptions.Compiled, "")]
    private static partial Regex MyRegex6();

    [GeneratedRegex(".*?Workload.*?", RegexOptions.IgnoreCase | RegexOptions.Compiled, "")]
    private static partial Regex MyRegex7();

    [GeneratedRegex(".*?Test.*?", RegexOptions.IgnoreCase | RegexOptions.Compiled, "")]
    private static partial Regex MyRegex8();
}
