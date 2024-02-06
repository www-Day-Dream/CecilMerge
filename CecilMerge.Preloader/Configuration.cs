using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace CecilMerge
{
    internal static class Configuration
    {
        
        private static ConfigFile Config { get; set; } =
            new ConfigFile(Path.Combine(Paths.ConfigPath, ChainPatcher.GenericName + ".cfg"), true);

        internal static bool ChainCaching => CacheChainPatcher.Value;
        private static readonly ConfigEntry<bool> CacheChainPatcher =
            Config.Bind("Caching", "CacheChainPatcher", true, 
                "True if the CecilMerge.ChainPatcher should cache information analyzed " +
                "from assemblies until there is a file update.");

        internal static bool VerboseLogging => DebugLogging.Value;
        private static readonly ConfigEntry<bool> DebugLogging =
            Config.Bind("Logging", "DebugLogging", false,
                "If we should log additional debug information related to CecilMerging.");
    }
}