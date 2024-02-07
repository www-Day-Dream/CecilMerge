using System.Collections.Generic;
using System.Linq;
using BepInEx;
using CecilMerge.Caching;
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
                        (s, merge) => s + "\n" + merge.DeclaringType.ResolvedTypeDef.Module.Name + ": " + 
                                      merge.DeclaringType.ResolvedTypeDef.Name + " --> " + merge.PatchedType.ResolvedTypeRef.FullName +
                                      merge.MethodMerges.Aggregate("", (s1, methodMerge) => s1 + (s1.Length > 0 ? ", " : "") + 
                                          methodMerge.TargetName + " " + methodMerge.CopyIL)));
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