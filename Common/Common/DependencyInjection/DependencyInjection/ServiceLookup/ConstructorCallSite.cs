using System.Reflection;

namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class ConstructorCallSite : IServiceCallSite {
        internal ConstructorInfo ConstructorInfo { get; }
        internal IServiceCallSite[] ParameterCallSites { get; }

        public ConstructorCallSite(ConstructorInfo constructorInfo, IServiceCallSite[] parameterCallSites) {
            ConstructorInfo = constructorInfo;
            ParameterCallSites = parameterCallSites;
        }
    }
}
