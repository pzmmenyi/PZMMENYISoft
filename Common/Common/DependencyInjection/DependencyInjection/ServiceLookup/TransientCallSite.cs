namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class TransientCallSite : IServiceCallSite {
        internal IServiceCallSite Service { get; }

        public TransientCallSite(IServiceCallSite service) {
            Service = service;
        }
    }
}
