﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;


namespace PZMMENYI.DependencyInjection.ServiceLookup {
    internal class CallSiteExpressionBuilder : CallSiteVisitor<ParameterExpression, Expression> {
        private static readonly MethodInfo CaptureDisposableMethodInfo = GetMethodInfo<Func<ServiceProvider, object, object>>((a, b) => a.CaptureDisposable(b));
        private static readonly MethodInfo TryGetValueMethodInfo = GetMethodInfo<Func<IDictionary<object, object>, object, object, bool>>((a, b, c) => a.TryGetValue(b, out c));
        private static readonly MethodInfo AddMethodInfo = GetMethodInfo<Action<IDictionary<object, object>, object, object>>((a, b, c) => a.Add(b, c));
        private static readonly MethodInfo MonitorEnterMethodInfo = GetMethodInfo<Action<object, bool>>((lockObj, lockTaken) => Monitor.Enter(lockObj, ref lockTaken));
        private static readonly MethodInfo MonitorExitMethodInfo = GetMethodInfo<Action<object>>(lockObj => Monitor.Exit(lockObj));
        private static readonly MethodInfo CallSiteRuntimeResolverResolve =
            GetMethodInfo<Func<CallSiteRuntimeResolver, IServiceCallSite, ServiceProvider, object>>((r, c, p) => r.Resolve(c, p));

        private static readonly ParameterExpression ProviderParameter = Expression.Parameter(typeof(ServiceProvider));

        private static readonly ParameterExpression ResolvedServices = Expression.Variable(typeof(IDictionary<object, object>),
            ProviderParameter.Name + "resolvedServices");
        private static readonly BinaryExpression ResolvedServicesVariableAssignment =
            Expression.Assign(ResolvedServices,
                Expression.Property(ProviderParameter, nameof(ServiceProvider.ResolvedServices)));

        private static readonly ParameterExpression CaptureDisposableParameter = Expression.Parameter(typeof(object));
        private static readonly LambdaExpression CaptureDisposable = Expression.Lambda(
                    Expression.Call(ProviderParameter, CaptureDisposableMethodInfo, CaptureDisposableParameter),
                    CaptureDisposableParameter);

        private readonly CallSiteRuntimeResolver _runtimeResolver;
        private bool _requiresResolvedServices;

        public CallSiteExpressionBuilder(CallSiteRuntimeResolver runtimeResolver) {
            if (runtimeResolver == null) {
                throw new ArgumentNullException(nameof(runtimeResolver));
            }
            _runtimeResolver = runtimeResolver;
        }

        public Func<ServiceProvider, object> Build(IServiceCallSite callSite) {
            if (callSite is SingletonCallSite) {
                return (provider) => _runtimeResolver.Resolve(callSite, provider);
            }
            return BuildExpression(callSite).Compile();
        }

        private Expression<Func<ServiceProvider, object>> BuildExpression(IServiceCallSite callSite) {
            var serviceExpression = VisitCallSite(callSite, ProviderParameter);

            var body = new List<Expression>();
            if (_requiresResolvedServices) {
                body.Add(ResolvedServicesVariableAssignment);
                serviceExpression = Lock(serviceExpression, ResolvedServices);
            }

            body.Add(serviceExpression);

            var variables = _requiresResolvedServices
                ? new[] { ResolvedServices }
                : Enumerable.Empty<ParameterExpression>();

            return Expression.Lambda<Func<ServiceProvider, object>>(
                Expression.Block(variables, body),
                ProviderParameter);
        }

        protected override Expression VisitSingleton(SingletonCallSite singletonCallSite, ParameterExpression provider) {

            return Expression.Call(
                Expression.Constant(_runtimeResolver),
                CallSiteRuntimeResolverResolve,
                Expression.Constant(singletonCallSite, typeof(IServiceCallSite)),
                provider);
        }

        protected override Expression VisitConstant(ConstantCallSite constantCallSite, ParameterExpression provider) {
            return Expression.Constant(constantCallSite.DefaultValue);
        }

        protected override Expression VisitCreateInstance(CreateInstanceCallSite createInstanceCallSite, ParameterExpression provider) {
            return Expression.New(createInstanceCallSite.Descriptor.ImplementationType);
        }

        protected override Expression VisitInstanceService(InstanceService instanceCallSite, ParameterExpression provider) {
            return Expression.Constant(
                instanceCallSite.Descriptor.ImplementationInstance,
                instanceCallSite.Descriptor.ServiceType);
        }

        protected override Expression VisitServiceProviderService(ServiceProviderService serviceProviderService, ParameterExpression provider) {
            return provider;
        }

        protected override Expression VisitEmptyIEnumerable(EmptyIEnumerableCallSite emptyIEnumerableCallSite, ParameterExpression provider) {
            return Expression.Constant(
                emptyIEnumerableCallSite.ServiceInstance,
                emptyIEnumerableCallSite.ServiceType);
        }

        protected override Expression VisitServiceScopeService(ServiceScopeService serviceScopeService, ParameterExpression provider) {
            return Expression.New(typeof(ServiceScopeFactory).GetTypeInfo()
                    .DeclaredConstructors
                    .Single(),
                provider);
        }

        protected override Expression VisitFactoryService(FactoryService factoryService, ParameterExpression provider) {
            return Expression.Invoke(Expression.Constant(factoryService.Descriptor.ImplementationFactory), provider);
        }

        protected override Expression VisitClosedIEnumerable(ClosedIEnumerableCallSite callSite, ParameterExpression provider) {
            return Expression.NewArrayInit(
                callSite.ItemType,
                callSite.ServiceCallSites.Select(cs =>
                    Expression.Convert(
                        VisitCallSite(cs, provider),
                        callSite.ItemType)));
        }

        protected override Expression VisitTransient(TransientCallSite callSite, ParameterExpression provider) {
            return Expression.Invoke(GetCaptureDisposable(provider),
                VisitCallSite(callSite.Service, provider));
        }

        protected override Expression VisitConstructor(ConstructorCallSite callSite, ParameterExpression provider) {
            var parameters = callSite.ConstructorInfo.GetParameters();
            return Expression.New(
                callSite.ConstructorInfo,
                callSite.ParameterCallSites.Select((c, index) =>
                        Expression.Convert(VisitCallSite(c, provider), parameters[index].ParameterType)));
        }

        protected override Expression VisitScoped(ScopedCallSite callSite, ParameterExpression provider) {
            var keyExpression = Expression.Constant(
                callSite.Key,
                typeof(object));

            var resolvedExpression = Expression.Variable(typeof(object), "resolved");

            var resolvedServices = GetResolvedServices(provider);

            var tryGetValueExpression = Expression.Call(
                resolvedServices,
                TryGetValueMethodInfo,
                keyExpression,
                resolvedExpression);

            var assignExpression = Expression.Assign(
                resolvedExpression, VisitCallSite(callSite.ServiceCallSite, provider));

            var addValueExpression = Expression.Call(
                resolvedServices,
                AddMethodInfo,
                keyExpression,
                resolvedExpression);

            var blockExpression = Expression.Block(
                typeof(object),
                new[] {
                    resolvedExpression
                },
                Expression.IfThen(
                    Expression.Not(tryGetValueExpression),
                    Expression.Block(assignExpression, addValueExpression)),
                resolvedExpression);

            return blockExpression;
        }

        private static MethodInfo GetMethodInfo<T>(Expression<T> expr) {
            var mc = (MethodCallExpression)expr.Body;
            return mc.Method;
        }

        public Expression GetCaptureDisposable(ParameterExpression provider) {
            if (provider != ProviderParameter) {
                throw new NotSupportedException("GetCaptureDisposable方法仅支持主要提供者。");
            }
            return CaptureDisposable;
        }

        public Expression GetResolvedServices(ParameterExpression provider) {
            if (provider != ProviderParameter) {
                throw new NotSupportedException("GetCaptureDisposable方法仅支持主要提供者。");
            }
            _requiresResolvedServices = true;
            return ResolvedServices;
        }

        private static Expression Lock(Expression body, Expression syncVariable) {
            var lockWasTaken = Expression.Variable(typeof(bool), "lockWasTaken");

            var monitorEnter = Expression.Call(MonitorEnterMethodInfo, syncVariable, lockWasTaken);
            var monitorExit = Expression.Call(MonitorExitMethodInfo, syncVariable);

            var tryBody = Expression.Block(monitorEnter, body);
            var finallyBody = Expression.IfThen(lockWasTaken, monitorExit);

            return Expression.Block(
                typeof(object),
                new[] { lockWasTaken },
                Expression.TryFinally(tryBody, finallyBody));
        }
    }
}
