using System;
using System.Collections.Generic;

namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal interface IService {
        IService Next { get; set; }

        ServiceLifetime Lifetime { get; }

        IServiceCallSite CreateCallSite(ServiceProvider provider, ISet<Type> callSiteChain);

        Type ServiceType { get; }
    }
}
