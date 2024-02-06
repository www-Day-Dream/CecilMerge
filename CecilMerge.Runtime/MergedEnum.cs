using System;
using System.Linq;
using System.Reflection;

namespace CecilMerge
{
    public static class MergedEnum<TPatchedEnum> where TPatchedEnum : Enum
    {
        public static TPatchedEnum ValueOrDefault(string valueName)
        {
            var field = typeof(TPatchedEnum)
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.FieldType == typeof(TPatchedEnum))
                .FirstOrDefault(f => f.Name == valueName);
            
            return (TPatchedEnum)(field?.GetRawConstantValue() ?? 0);
        }
    }
}