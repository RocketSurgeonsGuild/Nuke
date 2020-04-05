using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Humanizer;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public class UsingStep : BaseGitHubActionsStep
    {
        public UsingStep(string name) : base(name)
        {
        }

        public string Uses { get; set; }
        public Dictionary<string, string> With { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        protected void WithProperties(Func<string, string> transformName)
        {
            foreach (var property in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
               .Where(z => z.CanRead && z.CanWrite && z.DeclaringType == GetType()))
            {
                var value = property.GetValue(this);
                if (value == null) continue;

                With.Add(transformName(property.Name), value switch
                {
                    null => "",
                    bool b => b.ToString().ToLowerInvariant(),
                    string s => s,
                    _ => value.ToString()
                });
            }
        }


        public override void Write(CustomFileWriter writer)
        {
            base.Write(writer);

            using (writer.Indent())
            {
                writer.WriteLine($"uses: {Uses}");

                if (With?.Any() == true)
                {
                    writer.WriteLine("with:");
                    using (writer.Indent())
                    {
                        With.ForEach(x => writer.WriteLine($"{x.Key}: '{x.Value}'"));
                    }
                }
            }
        }
    }
}