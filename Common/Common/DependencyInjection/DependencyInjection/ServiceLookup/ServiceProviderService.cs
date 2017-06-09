using System;
using System.Collections.Generic;

namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class ServiceProviderService : IService, IServiceCallSite {
        public IService Next { get; set; }

        public ServiceLifetime Lifetime {
            get { return ServiceLifetime.Transient; }
        }

        public Type ServiceType => typeof(IServiceProvider);

        public IServiceCallSite CreateCallSite(ServiceProvider provider, ISet<Type> callSiteChain) {
            return this;
        }
    }
}
