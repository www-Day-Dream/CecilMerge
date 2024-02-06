using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using Mono.Cecil;

//using CecilMerge.Runtime.Preloader;

namespace CecilMerge
{
    public static class ChainPatcher
    {
        internal const string GenericName = "CecilMerge";
        internal static AssemblyCache Cache { get; private set; } = new AssemblyCache();

        
        public static IEnumerable<string> TargetDLLs => new string[] { "Assembly-CSharp.dll" };
        
        public static void Initialize()
        {
            Cache.CacheAssemblyInformation(Paths.PluginPath);
            foreach (var keyValuePair in Cache.Data)
            {
                CecilLog.LogVerbose(
                    "File: '" + keyValuePair.Key + "'",
                    keyValuePair.Value.Merges.Aggregate("Merges: ", 
                        (s, merge) => s + (s.Length > "Merges: ".Length ? "\n" : "") + 
                                      merge.Module.Name + "::" + merge.Type.FullName));
            }
        }

        public static void Patch(AssemblyDefinition assemblyToPatch)
        {
        }

        public static void Finish()
        {
            Cache.Dispose();
        }
    }
}