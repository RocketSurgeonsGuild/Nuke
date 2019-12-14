namespace Rocket.Surgery.Nuke
{
#pragma warning disable CA1717 // Only FlagsAttribute enums should have plural names
    /// <summary>
    /// What files to include in the binary log
    /// </summary>
    public enum MSBuildBinaryLogImports
#pragma warning restore CA1717 // Only FlagsAttribute enums should have plural names
    {
        /// <summary>Don't specify imports</summary>
        Unspecified = 0,

        /// <summary>Do not collect project and imports files</summary>
        None = 2,

        /// <summary>Embed in the binlog file</summary>
        Embed = 3,

        /// <summary>Produce a separate .ProjectImports.zip</summary>
        ZipFile = 4
    }
}