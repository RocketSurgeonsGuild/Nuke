using System.Text;
using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke;

[PublicAPI]
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class Arguments : IArguments
{
    internal const string Redacted = "[REDACTED]";
    private const char Space = ' ';

    private readonly List<string> _secrets = [];
    private readonly List<KeyValuePair<string, List<string>>> _arguments = [];

    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    public Arguments Add(string argumentFormat, bool? condition = true) => condition.HasValue && ( condition.Value || argumentFormat.Contains("{value}") )
            ? Add(argumentFormat, (object)condition.Value)
            : this;

    public Arguments Add<T>(string argumentFormat, T? value, char? disallowed = null, bool secret = false)
        where T : struct
    {
        return value.HasValue ? Add(argumentFormat, value.Value, disallowed, secret) : this;
    }

    public Arguments Add(string argumentFormat, object? value, char? disallowed = null, bool secret = false) => Add(argumentFormat, value?.ToString() ?? "", disallowed, customValue: false, secret);

    public Arguments Add(
        string argumentFormat,
        string? value,
        char? disallowed = null,
        bool customValue = false,
        bool secret = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return this;
        }

        if (secret)
        {
            _secrets.Add(value);
        }

        argumentFormat = argumentFormat.Replace("{value}", "{0}");
        AddInternal(argumentFormat, customValue ? value : value.DoubleQuoteIfNeeded(disallowed, Space));

        return this;
    }

    public Arguments Add<T>(
        string argumentFormat,
        IEnumerable<T>? values,
        char? separator = null,
        char? disallowed = null,
        bool quoteMultiple = false)
    {
        var list = values?.ToList();
        if (list is null || list.Count == 0)
        {
            return this;
        }

        argumentFormat = argumentFormat.Replace("{value}", "{0}");

        string Format(T value) => value.ToString().DoubleQuoteIfNeeded(separator, disallowed, Space);

        AddInternal(
            argumentFormat,
             separator.HasValue
                ? [FormatMultiple(list, Format, separator.Value, quoteMultiple)]
                : list.Select(Format).ToArray());

        return this;
    }

    public Arguments Add<TKey, TValue>(
        string argumentFormat,
        IReadOnlyDictionary<TKey, TValue>? dictionary,
        string itemFormat,
        char? separator = null,
        char? disallowed = null,
        bool quoteMultiple = false)
    {
        if (dictionary is null || dictionary.Count == 0)
        {
            return this;
        }

        argumentFormat = argumentFormat.Replace("{value}", "{0}");
        var keyValueSeparator = itemFormat.Replace("{key}", "").Replace("{value}", "");
        Assert.True(keyValueSeparator.Length == 1);

        string Format(object value) => value.ToString().DoubleQuoteIfNeeded(separator, keyValueSeparator.Single(), disallowed, Space);

        string FormatPair(KeyValuePair<TKey, TValue> pair)
            => itemFormat
              .Replace("{key}", Format(pair!.Key))
              .Replace("{value}", Format(pair!.Value));

        var pairs = dictionary.Where(x =>  x.Value is not null ).ToList();

        AddInternal(
            argumentFormat,
             separator.HasValue
                ? [FormatMultiple(pairs, FormatPair, separator.Value, quoteMultiple)]
                : pairs.Select(FormatPair).ToArray());

        return this;
    }

    public Arguments Add<TKey, TValue>(
        string argumentFormat,
        ILookup<TKey, TValue>? lookup,
        string itemFormat,
        char? separator = null,
        char? disallowed = null,
        bool quoteMultiple = false)
    {
        if (lookup is null || lookup.Count == 0)
        {
            return this;
        }

        argumentFormat = argumentFormat.Replace("{value}", "{0}");
        var listSeparator = itemFormat.Replace("{key}", "").Replace("{value}", "");
        Assert.True(listSeparator.Length == 1);

        string Format(object value) => value?.ToString().DoubleQuoteIfNeeded(separator, listSeparator.Single(), disallowed, Space);

        string FormatLookup(TKey key, string values)
            => itemFormat
              .Replace("{key}", Format(key!))
              .Replace("{value}", values);

        foreach (var list in lookup)
        {
            AddInternal(
                argumentFormat,
                 separator.HasValue
                    ? [FormatLookup(list.Key, FormatMultiple(list, x => Format(x!), separator.NotNull().Value, quoteMultiple))]
                    : list.Select(x => FormatLookup(list.Key, Format(x!))).ToArray());
        }

        return this;
    }

    public Arguments Concatenate(Arguments arguments)
    {
        _arguments.AddRange(arguments._arguments);
        _secrets.AddRange(arguments._secrets);
        return this;
    }

    private void AddInternal(string format, params string[] values)
    {
        var list = _arguments.LastOrDefault(x => x.Key.Equals(format, StringComparison.OrdinalIgnoreCase)).Value;
        if (list is null)
        {
            list = [];
            _arguments.Add(new KeyValuePair<string, List<string>>(format, list));
        }

        list.AddRange(values);
    }

    private string FormatMultiple<T>(IEnumerable<T> items, Func<T, string> format, char separator, bool quoteMultiple)
    {
        var values = items.Select(format).Join(separator);
        return !quoteMultiple
            ? values
            : values.DoubleQuoteIfNeeded();
    }

    public string FilterSecrets(string text) => _secrets.Aggregate(text, (str, s) => str.Replace(s, Redacted));

    private string Render(bool forOutput)
    {
        string Format(string argument)
            => !_secrets.Contains(argument) || !forOutput
                ? argument
                : Redacted;

        var builder = new StringBuilder();
        foreach (var argumentPair in _arguments)
        {
            foreach (var argument in argumentPair.Value)
            {
                builder.AppendFormat(argumentPair.Key, Format(argument)).Append(Space);
            }
        }

        return builder.ToString().TrimEnd();
    }

    public string RenderForExecution() => Render(forOutput: false);

    public string RenderForOutput() => Render(forOutput: true);

    public ArgumentStringHandler RenderForStringHandler()
    {
        var handler = new ArgumentStringHandler();
        if (RenderForExecution() is { Length: > 0 } args)
        handler.AppendLiteral(args);
        return handler;
    }

    public override string ToString() => RenderForOutput();
}
