using System;
using CecilMerge.Internal;

namespace CecilMerge
{
    public class CecilMergeType
    {
        public Type PatchType { get; internal set; }
        public string[] BeforeGUIDs { get; internal set; }
        public string[] AfterGUIDs { get; internal set; }

        public CecilMergeType(Type patchType)
        {
            PatchType = patchType;
        }
    }

    [AttributeUsage(InjectionSites)]
    public class CecilPatchAttribute : CecilMergeAttributeAttribute
    {
        public CecilPatchAttribute(Type patchType = default) : base(patchType)
        {
        }
    }

    [AttributeUsage(InjectionSites)]
    public class CecilBeforeAttribute : CecilMergeAttributeAttribute
    {
        public CecilBeforeAttribute(params string[] pluginGuids)
        {
            MergeType.BeforeGUIDs = pluginGuids;
        }
    }
    
    [AttributeUsage(InjectionSites)]
    public class CecilAfterAttribute : CecilMergeAttributeAttribute
    {
        public CecilAfterAttribute(params string[] pluginGuids)
        {
            MergeType.AfterGUIDs = pluginGuids;
        }
    }

    [AttributeUsage(InjectionSites)]
    public class CecilAppendAttribute : CecilMergeAttributeAttribute
    {
    }
    [AttributeUsage(InjectionSites)]
    public class CecilPrependAttribute : CecilMergeAttributeAttribute
    {
    }
}