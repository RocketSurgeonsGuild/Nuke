using System.Text.RegularExpressions;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Symbol store
/// </summary>
public static class Symbols
{
    /// <summary>
    ///     The default symbols
    /// </summary>
    public static readonly Dictionary<Regex, string> DefaultSymbols = new()
    {
        [new("(^Compile|^Build)", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "⚙️",
        [new("^Pack", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "📦",
        [new("^Publish", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "📫",
        [new("^Use", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🔨",
        [new("^Install", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "📲",
        [new("^Restore", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🎁",
        [new("^Publish", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🚀",
        [new(".*?Tool.*?", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "⚒️",
        [new(".*?Workload.*?", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🚒",
        [new(".*?Test.*?", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🚦",
        [new(".*?Test.*?", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "💨",
    };

    /// <summary>
    ///     Add the symbol from the given step
    /// </summary>
    /// <param name="key"></param>
    /// <param name="symbol"></param>
    public static void AddSymbol(Regex key, string symbol)
    {
        DefaultSymbols.Add(key, symbol);
    }

    /// <summary>
    ///     Configure the step name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string StepName(string name)
    {
        var symbol = DefaultSymbols.FirstOrDefault(z => z.Key.IsMatch(name)).Value;
        if (string.IsNullOrWhiteSpace(symbol)) return name;

        return $"{symbol} {name}";
    }
}
