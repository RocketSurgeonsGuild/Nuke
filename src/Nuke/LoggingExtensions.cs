using System;
using System.Collections.Generic;
using System.Text;
using Nuke.Common;

namespace Rocket.Surgery.Nuke
{
    public static class LoggingExtensions
    {
        public static void Log(string log) => Logger.Normal(log);

        public static void Debug(string info) { Logger.Trace(info); }

        public static void Debug(string info, params object[] args) { Logger.Trace(info, args); }

        public static void Information(string info) { Logger.Info(info); }

        public static void Information(string info, params object[] args) { Logger.Info(info, args); }

        public static void Warning(string info) { Logger.Warn(info); }

        public static void Warning(string info, params object[] args) { Logger.Warn(info, args); }

        public static void Error(string info) { Logger.Error(info); }

        public static void Error(string info, params object[] args) { Logger.Error(info, args); }

        public static string EnvironmentVariable(string key) => Environment.GetEnvironmentVariable(key);
    }
}
