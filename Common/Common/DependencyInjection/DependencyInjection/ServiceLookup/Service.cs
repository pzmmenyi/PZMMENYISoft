﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class Service : IService {
        private readonly ServiceDescriptor _descriptor;

        public Service(ServiceDescriptor descriptor) {
            _descriptor = descriptor;
        }

        public IService Next { get; set; }

        public ServiceLifetime Lifetime {
            get { return _descriptor.Lifetime; }
        }

        public IServiceCallSite CreateCallSite(ServiceProvider provider, ISet<Type> callSiteChain) {
            var constructors = _descriptor.ImplementationType.GetTypeInfo()
                .DeclaredConstructors
                .Where(constructor => constructor.IsPublic)
                .ToArray();

            IServiceCallSite[] parameterCallSites = null;

            if (constructors.Length == 0) {
                throw new InvalidOperationException("未发现公共构造函数。");
            }
            else if (constructors.Length == 1) {
                var constructor = constructors[0];
                var parameters = constructor.GetParameters();
                if (parameters.Length == 0) {
                    return new CreateInstanceCallSite(_descriptor);
                }

                parameterCallSites = PopulateCallSites(
                    provider,
                    callSiteChain,
                    parameters,
                    throwIfCallSiteNotFound: true);

                return new ConstructorCallSite(constructor, parameterCallSites);
            }

            Array.Sort(constructors,
                (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));

            ConstructorInfo bestConstructor = null;
            HashSet<Type> bestConstructorParameterTypes = null;
            for (var i = 0; i < constructors.Length; i++) {
                var parameters = constructors[i].GetParameters();

                var currentParameterCallSites = PopulateCallSites(
                    provider,
                    callSiteChain,
                    parameters,
                    throwIfCallSiteNotFound: false);

                if (currentParameterCallSites != null) {
                    if (bestConstructor == null) {
                        bestConstructor = constructors[i];
                        parameterCallSites = currentParameterCallSites;
                    }
                    else {
                        // Since we're visiting constructors in decreasing order of number of parameters,
                        // we'll only see ambiguities or supersets once we've seen a 'bestConstructor'.

                        if (bestConstructorParameterTypes == null) {
                            bestConstructorParameterTypes = new HashSet<Type>(
                                bestConstructor.GetParameters().Select(p => p.ParameterType));
                        }

                        if (!bestConstructorParameterTypes.IsSupersetOf(parameters.Select(p => p.ParameterType))) {
                            // Ambigious match exception
                            var message = string.Join("匹配参数异常。",
                                constructors[i]);
                            throw new InvalidOperationException(message);
                        }
                    }
                }
            }

            if (bestConstructor == null) {
                throw new InvalidOperationException("实例化异常。");
            }
            else {
                Debug.Assert(parameterCallSites != null);
                return parameterCallSites.Length == 0 ?
                    (IServiceCallSite)new CreateInstanceCallSite(_descriptor) :
                    new ConstructorCallSite(bestConstructor, parameterCallSites);
            }
        }

        public Type ServiceType => _descriptor.ServiceType;

        private bool IsSuperset(IEnumerable<Type> left, IEnumerable<Type> right) {
            return new HashSet<Type>(left).IsSupersetOf(right);
        }

        private IServiceCallSite[] PopulateCallSites(
            ServiceProvider provider,
            ISet<Type> callSiteChain,
            ParameterInfo[] parameters,
            bool throwIfCallSiteNotFound) {
            var parameterCallSites = new IServiceCallSite[parameters.Length];
            for (var index = 0; index < parameters.Length; index++) {
                var callSite = provider.GetServiceCallSite(parameters[index].ParameterType, callSiteChain);

                if (callSite == null && parameters[index].HasDefaultValue) {
                    callSite = new ConstantCallSite(parameters[index].DefaultValue);
                }

                if (callSite == null) {
                    if (throwIfCallSiteNotFound) {
                        throw new InvalidOperationException("不能解析服务。");
                    }

                    return null;
                }

                parameterCallSites[index] = callSite;
            }

            return parameterCallSites;
        }
    }
}
