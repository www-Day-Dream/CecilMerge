using System.IO;
using System.Linq;
using Mono.Cecil;

namespace CecilMerge.Puzzle
{
    internal class ComponentMethodDefinition : ComponentTypeDefinition
    {
        public int MethodID;
        public string TargetName;
        public bool CopyIL;

        public MethodDefinition ResolvedMethodDef;

        public ComponentMethodDefinition()
        {
            
        }
        public ComponentMethodDefinition(MethodDefinition methodDef) : base(methodDef.DeclaringType)
        {
            if (!IsValidated || !IsResolved) return;
            IsValidated = IsResolved = false;

            MethodID = methodDef.DeclaringType.Methods.IndexOf(methodDef);
            var cecilMergeAttr = methodDef.CustomAttributes
                .FirstOrDefault(attr => attr.AttributeType.FullName == typeof(CecilMergeMethodAttribute).FullName);
            
            TargetName = (string)(cecilMergeAttr?.ConstructorArguments[0].Value ?? methodDef.Name);
            CopyIL = (bool)(cecilMergeAttr?.ConstructorArguments[1].Value ?? false);
            IsValidated = true;
            ResolvedMethodDef = methodDef;
            IsResolved = true;
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            writer.Write(MethodID);
            writer.Write(TargetName);
            writer.Write(CopyIL);
        }

        public override void Load(BinaryReader reader)
        {
            base.Load(reader);
            if (!IsValidated) return;
            IsValidated = false;
            
            MethodID = reader.ReadInt32();
            TargetName = reader.ReadString();
            CopyIL = reader.ReadBoolean();
            IsValidated = true;
        }

        public override void Resolve(AssemblyDefinition resolveFrom)
        {
            base.Resolve(resolveFrom);
            if (!IsResolved) return;
            IsResolved = false;
            
            if (ResolvedTypeDef.Methods.Count <= MethodID)
            {
                CecilLog.LogError("Failed to resolve ComponentMethodDefinition {" + 
                                  ModuleID + ":" + TypeID + "." + MethodID + "} as " +
                                  "the TypeID is out of index of Module " + ResolvedModuleDef.Name + ".Types!");
                return;
            }

            ResolvedMethodDef = ResolvedTypeDef.Methods[MethodID];
            IsResolved = true;
        }

        protected override string ToIdentifierString() =>
            "{" + ModuleID + ":" + TypeID + "." + MethodID + "}";
    }
}