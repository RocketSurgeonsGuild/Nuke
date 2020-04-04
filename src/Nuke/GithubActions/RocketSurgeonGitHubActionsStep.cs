using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public abstract class BaseGitHubActionsStep : GitHubActionsStep
    {
        protected BaseGitHubActionsStep(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            StepName = name;
        }
        public string Id { get; set; }
        public string StepName { get; }
        public GithubActionCondition If { get; set; }
        public Dictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"- name: {GetStepName(StepName)}");
            using (writer.Indent())
            {
                if (!string.IsNullOrWhiteSpace(Id))
                {
                    writer.WriteLine($"id: {Id}");
                }
                if (!string.IsNullOrWhiteSpace(If))
                {
                    writer.WriteLine($"if: {If}");
                }
                if (Environment?.Any() == true)
                {
                    writer.WriteLine("env:");
                    using (writer.Indent())
                    {
                        Environment.ForEach(x => { writer.WriteLine($"{x.Key}: {x.Value}"); });
                    }
                }
            }
        }

        public static readonly Dictionary<Regex, string> DefaultSymbols = new Dictionary<Regex, string>()
        {
            [new Regex("(^Compile|Compile$|^Build|Build$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "⚙",
            [new Regex("(^Pack|Pack$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "📦",
            [new Regex("^Use", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🔨",
            [new Regex("^Install", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "📲",
            [new Regex("(^Restore|Restore$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "📪",
            [new Regex("(^Publish|Publish$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🐿",
            [new Regex(".*?Test.*?", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🚦",
            [new Regex("Tool", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "🛠",
            [new Regex("Run", RegexOptions.Compiled | RegexOptions.IgnoreCase)] = "💨",
        };

        public static void AddSymbol(Regex key, string symbol)
        {
            DefaultSymbols.Add(key, symbol);
        }

        protected virtual string GetStepName(string name)
        {
            var symbol = DefaultSymbols.FirstOrDefault(z => z.Key.IsMatch(name)).Value;
            if (string.IsNullOrWhiteSpace(symbol)) return name;

            return $"{symbol} {name}";
        }
    }
}