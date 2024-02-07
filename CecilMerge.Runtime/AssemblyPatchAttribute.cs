using System;
namespace CecilMerge
{
    [AttributeUsage(InjectionSites)]
    public class CecilPatchAttribute : Attribute
    {
        private const AttributeTargets InjectionSites = 
            AttributeTargets.Class | 
            AttributeTargets.Enum;
        
        public CecilPatchAttribute(Type patchType)
        {
        }
    }

    [AttributeUsage(InjectionSites)]
    public class CecilMergeMethodAttribute : Attribute
    {
        private const AttributeTargets InjectionSites =
            AttributeTargets.Method;
        
        public CecilMergeMethodAttribute(string targetName = default, bool injectBody = false)
        {
        }
    }
}