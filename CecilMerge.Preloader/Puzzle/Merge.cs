using System.IO;
using System.Linq;
using CecilMerge.Caching;
using Mono.Cecil;

namespace CecilMerge.Puzzle
{
    internal class Merge : ICecilCacheable, IResolvable<AssemblyDefinition>
    {
        public ComponentTypeDefinition DeclaringType;
        public ComponentTypeReference PatchedType;

        public ComponentMethodDefinition[] MethodMerges;
        

        internal static void Resolve(Merge[] merges, AssemblyDefinition assemblyDef)
        {
            foreach (var merge in merges)
                merge.Resolve(assemblyDef);
        }
        internal static Merge[] Evaluate(AssemblyDefinition assemblyDefinition) =>
            assemblyDefinition.Modules
                .SelectMany(module => module.Types)
                .Select(FromCecil)
                .Where(merge => merge != null).ToArray();
        
        internal static Merge FromCecil(TypeDefinition typeDef)
        {
            if (!typeDef.HasCustomAttributes) return null;
            
            var cecilPatchAttr = typeDef.CustomAttributes
                .FirstOrDefault(attr => attr.AttributeType.FullName == typeof(CecilPatchAttribute).FullName);
            if (cecilPatchAttr == null) return null;

            var patchTypeRef = (TypeReference)cecilPatchAttr.ConstructorArguments[0].Value ?? typeDef.BaseType;

            if (patchTypeRef == null)
            {
                CecilLog.LogError("[CecilPatch] Lacks a Type parameter and the type doesn't " +
                                  "inherit from a valid TypeReference!");
                return null;
            }
            
            var newMerge = new Merge
            {
                IsValidated = true,
                DeclaringType = new ComponentTypeDefinition(typeDef),
                PatchedType = new ComponentTypeReference(patchTypeRef),
                MethodMerges = typeDef.Methods
                    .Where(method => method.HasCustomAttributes && method.CustomAttributes
                        .Any(attr => attr.AttributeType.FullName == typeof(CecilMergeMethodAttribute).FullName))
                    .Select(method => new ComponentMethodDefinition(method))
                    .ToArray()
            };

            return newMerge;
        }

        public bool IsValidated { get; private set; }
        public void Save(BinaryWriter binaryWriter)
        {
            DeclaringType.Save(binaryWriter);
            PatchedType.Save(binaryWriter);
            binaryWriter.Write(MethodMerges.Length);
            foreach (var methodMerge in MethodMerges)
                methodMerge.Save(binaryWriter);
        }
        public void Load(BinaryReader binaryReader)
        {
            DeclaringType = new ComponentTypeDefinition();
            DeclaringType.Load(binaryReader);
            if (!DeclaringType.IsValidated) return;
            PatchedType = new ComponentTypeReference();
            PatchedType.Load(binaryReader);
            if (!PatchedType.IsValidated) return;
            var numOfMerges = binaryReader.ReadInt32();
            MethodMerges = new ComponentMethodDefinition[numOfMerges];
            for (var i = 0; i < numOfMerges; i++)
            {
                var methodMerge = new ComponentMethodDefinition();
                methodMerge.Load(binaryReader);
                if (!methodMerge.IsValidated) return;
                MethodMerges[i] = methodMerge;
            }
            
            IsValidated = true;
        }

        public bool IsResolved { get; set; }
        public void Resolve(AssemblyDefinition resolveFrom)
        {
            DeclaringType.Resolve(resolveFrom);
            if (!DeclaringType.IsResolved) return;
            PatchedType.Resolve(resolveFrom);
            if (!PatchedType.IsResolved) return;

            foreach (var methodMerge in MethodMerges)
            {
                methodMerge.Resolve(resolveFrom);
                if (!methodMerge.IsResolved) return;
            }
            
            IsResolved = true;
        }
    }
}