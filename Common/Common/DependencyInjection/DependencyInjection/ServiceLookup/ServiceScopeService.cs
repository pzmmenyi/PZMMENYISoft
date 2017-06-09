using System;
using System.Collections.Generic;

namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class ServiceScopeService : IService, IServiceCallSite {
        public IService Next { get; set; }

        public ServiceLifetime Lifetime {
            get { return ServiceLifetime.Scoped; }
        }

        public Type ServiceType => typeof(IServiceScopeFactory);

        public IServiceCallSite CreateCallSite(ServiceProvider provider, ISet<Type> callSiteChain) {
            return this;
        }
    }
}
