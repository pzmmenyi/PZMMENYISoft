namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class SingletonCallSite : ScopedCallSite {
        public SingletonCallSite(IService key, IServiceCallSite serviceCallSite) : base(key, serviceCallSite) {
        }
    }
}
