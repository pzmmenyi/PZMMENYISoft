namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class ScopedCallSite : IServiceCallSite {
        internal IService Key { get; }
        internal IServiceCallSite ServiceCallSite { get; }

        public ScopedCallSite(IService key, IServiceCallSite serviceCallSite) {
            Key = key;
            ServiceCallSite = serviceCallSite;
        }
    }
}
