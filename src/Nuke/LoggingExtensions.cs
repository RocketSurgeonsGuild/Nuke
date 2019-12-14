using System;
using Nuke.Common;

namespace Rocket.Surgery.Nuke
{
    /// <inheritdoc />
    public static class LoggingExtensions
    {
        /// <inheritdoc />
        public static void Log(string log) => Logger.Normal(log);

        /// <inheritdoc />
        public static void Debug(string info) => Logger.Trace(info);

        /// <inheritdoc />
        public static void Debug(string info, params object[] args) => Logger.Trace(info, args);

        /// <inheritdoc />
        public static void Information(string info) => Logger.Info(info);

        /// <inheritdoc />
        public static void Information(string info, params object[] args) => Logger.Info(info, args);

        /// <inheritdoc />
        public static void Warning(string info) => Logger.Warn(info);

        /// <inheritdoc />
        public static void Warning(string info, params object[] args) => Logger.Warn(info, args);

        /// <inheritdoc />
        public static void Error(string info) => Logger.Error(info);

        /// <inheritdoc />
        public static void Error(string info, params object[] args) => Logger.Error(info, args);

        /// <inheritdoc />
        public static string? EnvironmentVariable(string key) => Environment.GetEnvironmentVariable(key);
    }
}