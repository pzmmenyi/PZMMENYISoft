using System;

namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class ClosedIEnumerableCallSite : IServiceCallSite {
        internal Type ItemType { get; }
        internal IServiceCallSite[] ServiceCallSites { get; }

        public ClosedIEnumerableCallSite(Type itemType, IServiceCallSite[] serviceCallSites) {
            ItemType = itemType;
            ServiceCallSites = serviceCallSites;
        }
    }
}
