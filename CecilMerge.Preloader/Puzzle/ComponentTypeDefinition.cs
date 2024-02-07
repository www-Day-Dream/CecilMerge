using System.IO;
using Mono.Cecil;

namespace CecilMerge.Puzzle
{
    internal class ComponentTypeDefinition : ComponentModuleDefinition
    {
        public int TypeID;

        public TypeDefinition ResolvedTypeDef;

        public ComponentTypeDefinition()
        {
        }
        public ComponentTypeDefinition(TypeDefinition typeDef) : base(typeDef.Module)
        {
            if (!IsValidated || !IsResolved) return;
            IsValidated = IsResolved = false;
            
            TypeID = typeDef.Module.Types.IndexOf(typeDef);
            IsValidated = true;
            ResolvedTypeDef = typeDef;
            IsResolved = true;
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(TypeID);
        }

        public override void Load(BinaryReader reader)
        {
            base.Load(reader);
            if (!IsValidated) return;
            IsValidated = false;
            
            TypeID = reader.ReadInt32();
            IsValidated = true;
        }

        public override void Resolve(AssemblyDefinition resolveFrom)
        {
            base.Resolve(resolveFrom);
            if (!IsResolved) return;
            IsResolved = false;
            
            if (ResolvedModuleDef.Types.Count <= TypeID)
            {
                CecilLog.LogError("Failed to resolve ComponentTypeDefinition {" + ModuleID + ":" + TypeID + "} as " +
                                  "the TypeID is out of index of Module " + ResolvedModuleDef.Name + ".Types!");
                return;
            }

            ResolvedTypeDef = ResolvedModuleDef.Types[TypeID];
            IsResolved = true;
        }

        protected override string ToIdentifierString() =>
            "{" + ModuleID + ":" + TypeID + "}";
    }
}