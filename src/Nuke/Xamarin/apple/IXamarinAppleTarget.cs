using Serilog;

// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Represents an application with a apple head, and associated concerns.
/// </summary>
public partial interface IXamarinAppleTarget : IHaveBundleIdentifier, IHaveGitVersion, IHaveInfoPlist
{
    /// <summary>
    ///     modify info.plist
    /// </summary>
    Target ModifyInfoPlist => d => d
                                        .Executes(
                                             () =>
                                             {
                                                 Log.Verbose("Info.plist Path: {InfoPlist}", InfoPlist);
                                                 var plist = Plist.Deserialize(InfoPlist);

#pragma warning disable CA1304, CA1308
                                                 plist["CFBundleIdentifier"] = $"{BundleIdentifier}.{Suffix.ToLowerInvariant()}".TrimEnd('.');
#pragma warning restore CA1304, CA1308
                                                 Log.Information("CFBundleIdentifier: {CFBundleIdentifier}", plist["CFBundleIdentifier"]);

                                                 plist["CFBundleShortVersionString"] = GitVersion.MajorMinorPatch();
                                                 Log.Information(
                                                     "CFBundleShortVersionString: {CFBundleShortVersionString}",
                                                     plist["CFBundleShortVersionString"]
                                                 );

                                                 plist["CFBundleVersion"] = GitVersion.AssemblyVersion();
                                                 Log.Information("CFBundleVersion: {CFBundleVersion}", plist["CFBundleVersion"]);

                                                 Plist.Serialize(InfoPlist, plist);
                                             }
                                         );
}
