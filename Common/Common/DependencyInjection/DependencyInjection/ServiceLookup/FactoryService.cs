﻿using System;
using System.Collections.Generic;

namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class FactoryService : IService, IServiceCallSite {
        internal ServiceDescriptor Descriptor { get; }

        public FactoryService(ServiceDescriptor descriptor) {
            Descriptor = descriptor;
        }

        public IService Next { get; set; }

        public ServiceLifetime Lifetime {
            get { return Descriptor.Lifetime; }
        }

        public Type ServiceType => Descriptor.ServiceType;

        public IServiceCallSite CreateCallSite(ServiceProvider provider, ISet<Type> callSiteChain) {
            return this;
        }
    }
}
