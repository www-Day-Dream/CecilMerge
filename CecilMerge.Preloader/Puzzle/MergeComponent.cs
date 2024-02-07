using System.IO;
using CecilMerge.Caching;
using Mono.Cecil;

namespace CecilMerge.Puzzle
{
    internal abstract class MergeComponent : ICecilCacheable, IResolvable<AssemblyDefinition>
    {
        protected bool ValidatedQualifier()
        {
            if (IsValidated) return true;
            CecilLog.LogError($"Failed to resolve {GetType().Name} w/ ID {ToIdentifierString()} as " +
                              "the cache hasn't validated it!");
            return false;

        }
        protected bool ResolvedQualifier(string errorMessage)
        {
            if (IsResolved) return true;
            CecilLog.LogError(errorMessage);
            return false;
        }
        
        public bool IsValidated { get; protected set; }
        public abstract void Save(BinaryWriter writer);
        public abstract void Load(BinaryReader reader);

        public bool IsResolved { get; protected set; }
        public abstract void Resolve(AssemblyDefinition resolveFrom);

        protected abstract string ToIdentifierString();
    }
}