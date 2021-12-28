using System.Text.RegularExpressions;

namespace Rocket.Surgery.Nuke;

public class Symbols
{
    /// <summary>
    ///     The default symbols
    /// </summary>
    public static readonly Dictionary<Regex, string> DefaultSymbols = new()
    {
        [new Regex("(^Compile|Compile$|^Build|Build$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "⚙",
        [new Regex("(^Pack|Pack$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "📦",
        [new Regex("^Use", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🔨",
        [new Regex("^Install", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "📲",
        [new Regex("(^Restore|Restore$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🎁",
        [new Regex("(^Publish|Publish$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🐿",
        [new Regex(".*?Test.*?", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🚦",
        [new Regex("Tool", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🛠",
        [new Regex("Run", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "💨",
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