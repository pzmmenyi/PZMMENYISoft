using System;
using System.Collections.Generic;

namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class CallSiteValidator : CallSiteVisitor<CallSiteValidator.CallSiteValidatorState, Type> {
        private readonly Dictionary<Type, Type> _scopedServices = new Dictionary<Type, Type>();

        public void ValidateCallSite(Type serviceType, IServiceCallSite callSite) {
            var scoped = VisitCallSite(callSite, default(CallSiteValidatorState));
            if (scoped != null) {
                _scopedServices.Add(serviceType, scoped);
            }
        }

        public void ValidateResolution(Type serviceType, ServiceProvider serviceProvider) {
            Type scopedService;
            if (ReferenceEquals(serviceProvider, serviceProvider.Root)
                && _scopedServices.TryGetValue(serviceType, out scopedService)) {
                if (serviceType == scopedService) {
                    throw new InvalidOperationException(nameof(ServiceLifetime.Scoped).ToLowerInvariant()+
                                                     "从根上直接解析异常。");
                }

                throw new InvalidOperationException(nameof(ServiceLifetime.Scoped).ToLowerInvariant()+
                                                     "从根上解析异常。");
            }
        }

        protected override Type VisitTransient(TransientCallSite transientCallSite, CallSiteValidatorState state) {
            return VisitCallSite(transientCallSite.Service, state);
        }

        protected override Type VisitConstructor(ConstructorCallSite constructorCallSite, CallSiteValidatorState state) {
            Type result = null;
            foreach (var parameterCallSite in constructorCallSite.ParameterCallSites) {
                var scoped = VisitCallSite(parameterCallSite, state);
                if (result == null) {
                    result = scoped;
                }
            }
            return result;
        }

        protected override Type VisitClosedIEnumerable(ClosedIEnumerableCallSite closedIEnumerableCallSite,
            CallSiteValidatorState state) {
            Type result = null;
            foreach (var serviceCallSite in closedIEnumerableCallSite.ServiceCallSites) {
                var scoped = VisitCallSite(serviceCallSite, state);
                if (result == null) {
                    result = scoped;
                }
            }
            return result;
        }

        protected override Type VisitSingleton(SingletonCallSite singletonCallSite, CallSiteValidatorState state) {
            state.Singleton = singletonCallSite;
            return VisitCallSite(singletonCallSite.ServiceCallSite, state);
        }

        protected override Type VisitScoped(ScopedCallSite scopedCallSite, CallSiteValidatorState state) {
            // We are fine with having ServiceScopeService requested by singletons
            if (scopedCallSite.ServiceCallSite is ServiceScopeService) {
                return null;
            }
            if (state.Singleton != null) {
                throw new InvalidOperationException(nameof(state.Singleton)+ "范围调用点不能为单例调用点。");
            }
            return scopedCallSite.Key.ServiceType;
        }

        protected override Type VisitConstant(ConstantCallSite constantCallSite, CallSiteValidatorState state) => null;

        protected override Type VisitCreateInstance(CreateInstanceCallSite createInstanceCallSite, CallSiteValidatorState state) => null;

        protected override Type VisitInstanceService(InstanceService instanceCallSite, CallSiteValidatorState state) => null;

        protected override Type VisitServiceProviderService(ServiceProviderService serviceProviderService, CallSiteValidatorState state) => null;

        protected override Type VisitEmptyIEnumerable(EmptyIEnumerableCallSite emptyIEnumerableCallSite, CallSiteValidatorState state) => null;

        protected override Type VisitServiceScopeService(ServiceScopeService serviceScopeService, CallSiteValidatorState state) => null;

        protected override Type VisitFactoryService(FactoryService factoryService, CallSiteValidatorState state) => null;

        internal struct CallSiteValidatorState {
            public SingletonCallSite Singleton { get; set; }
        }
    }
}
