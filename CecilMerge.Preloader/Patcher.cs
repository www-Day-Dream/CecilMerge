using System.Collections.Generic;
using BepInEx;
using Mono.Cecil;

//using CecilMerge.Runtime.Preloader;

namespace CecilMerge
{
    internal static class ChainPatcher
    {
        internal static PluginAnalyzer Analyzer { get; private set; } = new PluginAnalyzer();
        public static IEnumerable<string> TargetDLLs => new string[] { "Assembly-CSharp.dll" };
        public static void Initialize()
        {
            Analyzer.Analyze(Paths.PluginPath);
        }

        public static void Patch(AssemblyDefinition assemblyToPatch)
        {
        }

        public static void Finish()
        {
            Analyzer.Dispose();
        }
    }
}