using System;

namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal interface IGenericService {
        ServiceLifetime Lifetime { get; }

        IService GetService(Type closedServiceType);
    }
}
