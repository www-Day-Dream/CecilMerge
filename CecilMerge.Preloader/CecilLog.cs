using BepInEx.Logging;

namespace CecilMerge
{
    internal static class CecilLog
    {
        private static ManualLogSource LogSource { get; set; } =
            BepInEx.Logging.Logger.CreateLogSource(ChainPatcher.GenericName);

        internal static void LogInfo(params object[] objects)
        {
            foreach (var o in objects)
                LogSource.LogInfo(o);
        }
        internal static void LogWarning(params object[] objects)
        {
            foreach (var o in objects)
                LogSource.LogWarning(o);
        }
        internal static void LogError(params object[] objects)
        {
            foreach (var o in objects)
                LogSource.LogError(o);
        }

        internal static void LogVerbose(params object[] objects)
        {
            if (!Configuration.VerboseLogging) return;
            foreach(var o in objects)
                LogSource.LogWarning(o);
        }
    }
}