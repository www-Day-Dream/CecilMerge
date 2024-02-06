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
            Cache.CacheAssemblyInformation(Paths.PluginPath).ToArray();
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