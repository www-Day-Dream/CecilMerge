using System;

namespace CecilMerge.Internal
{
    public class CecilMergeAttributeAttribute : Attribute
    {
        internal const AttributeTargets InjectionSites = AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Delegate;

        protected CecilMergeType MergeType { get; private set; }

        protected CecilMergeAttributeAttribute(Type patchType = default)
        {
            MergeType = new CecilMergeType(patchType);
        }
    }
}