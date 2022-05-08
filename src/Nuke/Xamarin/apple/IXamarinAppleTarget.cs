using System.Text.RegularExpressions;
using Serilog;

// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Represents an application with a apple head, and associated concerns.
/// </summary>
public interface IXamarinAppleTarget : IHaveBundleIdentifier, IHaveGitVersion, IHaveInfoPlist
{
    /// <summary>
    ///     modify info.plist
    /// </summary>
    public Target ModifyInfoPlist => _ => _
       .Executes(
            () =>
            {
                Log.Verbose("Info.plist Path: {InfoPlist}", InfoPlist);
                var plist = Plist.Deserialize(InfoPlist);

                plist["CFBundleIdentifier"] = $"{BundleIdentifier}.{Suffix.ToLower()}".TrimEnd('.');
                Log.Information("CFBundleIdentifier: {CFBundleIdentifier}", plist["CFBundleIdentifier"]);

                plist["CFBundleShortVersionString"] = Regex.Replace(GitVersion.MajorMinorPatch(), "[^0-9.]", "");
                Log.Information(
                    "CFBundleShortVersionString: {CFBundleShortVersionString}",
                    plist["CFBundleShortVersionString"]
                );

                plist["CFBundleVersion"] = GitVersion.FullSemanticVersion();
                Log.Information("CFBundleVersion: {CFBundleVersion}", plist["CFBundleVersion"]);

                Plist.Serialize(InfoPlist, plist);
            }
        );
}
