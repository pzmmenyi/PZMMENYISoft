namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class ConstantCallSite : IServiceCallSite {
        internal object DefaultValue { get; }

        public ConstantCallSite(object defaultValue) {
            DefaultValue = defaultValue;
        }
    }
}
