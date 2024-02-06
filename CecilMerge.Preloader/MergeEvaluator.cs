using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace CecilMerge
{
    internal class Merge
    {
        public int ModuleID;
        public int TypeID;
        
        public bool IsResolved;
        public ModuleDefinition Module;
        public TypeDefinition Type;
        
        public void Resolve(AssemblyDefinition assemblyDef)
        {
            if (assemblyDef.Modules.Count <= ModuleID) return;
            Module = assemblyDef.Modules[ModuleID];
            if (Module.Types.Count <= TypeID) return;
            Type = Module.Types[TypeID];
            
            IsResolved = true;
        }

        public void Save(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(ModuleID);
            binaryWriter.Write(TypeID);
        }

        public void Load(BinaryReader binaryReader)
        {
            ModuleID = binaryReader.ReadInt32();
            TypeID = binaryReader.ReadInt32();
        }
        public static Merge FromCecil(TypeDefinition typeDef)
        {
            if (!typeDef.HasCustomAttributes) return null;
            var newMerge = new Merge()
            {
                ModuleID = typeDef.Module.Assembly.Modules.IndexOf(typeDef.Module),
                TypeID = typeDef.Module.Types.IndexOf(typeDef),
            };
            newMerge.Resolve(typeDef.Module.Assembly);
            return newMerge;
        }
    }
    internal static class MergeEvaluator
    {
        internal static void Resolve(Merge[] merges, AssemblyDefinition assemblyDef)
        {
            foreach (var merge in merges)
                merge.Resolve(assemblyDef);
        }
        internal static Merge[] Evaluate(AssemblyDefinition assemblyDefinition) =>
            assemblyDefinition.Modules
                .SelectMany(module => module.Types)
                .Select(Merge.FromCecil)
                .Where(merge => merge != null).ToArray();
    }
}