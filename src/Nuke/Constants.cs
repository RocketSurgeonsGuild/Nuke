namespace Rocket.Surgery.Nuke;

internal static class Constants
{
    public const string ReportGeneratorFramework =
#if NET6_0_OR_GREATER
            "net6.0"
#elif NET5_0_OR_GREATER
            "net5.0"
#else
            "netcoreapp3.1"
#endif
        ;
    public const string GitVersionFramework =
#if NET6_0_OR_GREATER
            "net6.0"
#elif NET5_0_OR_GREATER
            "net5.0"
#else
            "netcoreapp3.1"
#endif
        ;
}
