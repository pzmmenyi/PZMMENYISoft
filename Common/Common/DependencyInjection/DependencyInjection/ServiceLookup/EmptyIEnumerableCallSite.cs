using System;

namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class EmptyIEnumerableCallSite : IServiceCallSite {
        internal object ServiceInstance { get; }
        internal Type ServiceType { get; }

        public EmptyIEnumerableCallSite(Type serviceType, object serviceInstance) {
            ServiceType = serviceType;
            ServiceInstance = serviceInstance;
        }
    }
}
