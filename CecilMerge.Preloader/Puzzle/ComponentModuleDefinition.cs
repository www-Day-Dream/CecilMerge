using System.IO;
using Mono.Cecil;

namespace CecilMerge.Puzzle
{
    internal class ComponentModuleDefinition : MergeComponent
    {
        public int ModuleID;

        public ModuleDefinition ResolvedModuleDef;

        public ComponentModuleDefinition()
        {
        }
        public ComponentModuleDefinition(ModuleDefinition moduleDef)
        {
            ModuleID = moduleDef.Assembly.Modules.IndexOf(moduleDef);
            IsValidated = true;
            ResolvedModuleDef = moduleDef;
            IsResolved = true;
        }
        public override void Save(BinaryWriter writer)
        {
            writer.Write(ModuleID);
        }

        public override void Load(BinaryReader reader)
        {
            ModuleID = reader.ReadInt32();
            IsValidated = true;
        }

        public override void Resolve(AssemblyDefinition resolveFrom)
        {
            if (!ValidatedQualifier()) return;
            if (resolveFrom.Modules.Count <= ModuleID)
            {
                CecilLog.LogError($"Failed to resolve ComponentModuleDefinition w/ ID {ToIdentifierString()} as " +
                                  "the ModuleID is out of index of Assembly " + resolveFrom.Name.Name + ".Modules!");
                return;
            }

            ResolvedModuleDef = resolveFrom.Modules[ModuleID];
            IsResolved = true;
        }

        protected override string ToIdentifierString() =>
            "{" + ModuleID + "}";
    }
}