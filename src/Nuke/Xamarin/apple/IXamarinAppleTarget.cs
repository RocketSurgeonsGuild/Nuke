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

                Log.Verbose("Deserialized Plist:\n {@Plist}\n", plist);

                plist["CFBundleIdentifier"] = $"{BundleIdentifier}.{Suffix.ToLower()}".TrimEnd('.');
                Log.Information("CFBundleIdentifier: {CFBundleIdentifier}", plist["CFBundleIdentifier"]);

                plist["CFBundleShortVersionString"] = $"{GitVersion?.Major}.{GitVersion?.Minor}.{GitVersion?.Patch}";
                Log.Information(
                    "CFBundleShortVersionString: {CFBundleShortVersionString}",
                    plist["CFBundleShortVersionString"]
                );

                plist["CFBundleVersion"] = $"{GitVersion?.FullSemanticVersion()}";
                Log.Information("CFBundleVersion: {CFBundleVersion}", plist["CFBundleVersion"]);

                Plist.Serialize(InfoPlist, plist);
                Log.Verbose("Serialized Plist:\n {@Plist}\n", plist);
            }
        );
}
