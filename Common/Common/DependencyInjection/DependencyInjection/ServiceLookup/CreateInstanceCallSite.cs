namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class CreateInstanceCallSite : IServiceCallSite {
        internal ServiceDescriptor Descriptor { get; }

        public CreateInstanceCallSite(ServiceDescriptor descriptor) {
            Descriptor = descriptor;
        }
    }
}
