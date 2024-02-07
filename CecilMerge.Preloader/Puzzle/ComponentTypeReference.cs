using System.IO;
using Mono.Cecil;

namespace CecilMerge.Puzzle
{
    internal class ComponentTypeReference : ComponentModuleDefinition
    {
        public string TypeID;
        public string ScopeID;

        public TypeReference ResolvedTypeRef;

        public ComponentTypeReference()
        {
            
        }
        public ComponentTypeReference(TypeReference patchTypeRef) : base(patchTypeRef.Module)
        {
            if (!IsValidated || !IsResolved) return;
            IsValidated = IsResolved = false;

            TypeID = patchTypeRef.FullName;
            ScopeID = patchTypeRef.Scope.Name;
            IsValidated = true;
            ResolvedTypeRef = patchTypeRef;
            IsResolved = true;
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(TypeID);
            writer.Write(ScopeID);
        }

        public override void Load(BinaryReader reader)
        {
            base.Load(reader);
            if (!IsValidated) return;
            IsValidated = false;
            
            TypeID = reader.ReadString();
            ScopeID = reader.ReadString();
            IsValidated = true;
        }

        public override void Resolve(AssemblyDefinition resolveFrom)
        {
            base.Resolve(resolveFrom);
            if (!IsResolved) return;
            IsResolved = false;

            if (!ResolvedModuleDef.TryGetTypeReference(ScopeID, TypeID, out var typeRef))
                return;

            ResolvedTypeRef = typeRef;
            IsResolved = true;
        }

        protected override string ToIdentifierString() =>
            "{" + ModuleID + ":" + TypeID + "}";
    }
}