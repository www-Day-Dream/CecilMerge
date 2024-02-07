using System.IO;

namespace CecilMerge.Caching
{
    internal interface ICecilCacheable
    {
        bool IsValidated { get; }
        
        void Save(BinaryWriter writer);
        void Load(BinaryReader reader);
    }
}