using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace PZMMENYI.DependencyInjection.Internal {
    /// <summary>
    ///实例化对象工厂类。
    /// </summary>
    /// <param name="serviceProvider">服务提供者，只包含一个方法<see cref="IServiceProvider.GetService(Type)"/>。</param>
    /// <param name="arguments">对象参数。</param>
    /// <returns></returns>
    internal delegate object ObjectFactory(IServiceProvider serviceProvider, object[] arguments);
    /// <summary>
    /// 实例化管理类。
    /// </summary>
    internal static class ActivatorUtilities {
        private static readonly MethodInfo GetServiceInfo =
            GetMethodInfo<Func<IServiceProvider, Type, Type, bool, object>>((sp, t, r, c) => GetService(sp, t, r, c));
        /// <summary>
        /// 创建实例化。
        /// </summary>
        /// <typeparam name="T">实例化类型。</typeparam>
        /// <param name="provider">服务提供者。</param>
        /// <param name="parameters">不定参数。</param>
        /// <returns>实例化的类型。</returns>
        internal static object CreateInstance(IServiceProvider provider, Type instanceType, params object[] parameters) {
            int bestLength = -1;
            ConstructorMatcher bestMatcher = null;

            foreach (var matcher in instanceType
                .GetTypeInfo()
                .DeclaredConstructors
                .Where(c => !c.IsStatic && c.IsPublic)
                .Select(constructor => new ConstructorMatcher(constructor))) {
                var length = matcher.Match(parameters);
                if (length == -1) {
                    continue;
                }
                if (bestLength < length) {
                    bestLength = length;
                    bestMatcher = matcher;
                }
            }

            if (bestMatcher == null) {
                var message = $"无法找到一个合适的构造函数{instanceType}类型。"
                                               + "确保实例化公共构造函数类型的所有参数。";
                throw new InvalidOperationException(message);
            }

            return bestMatcher.CreateInstance(provider);
        }
        /// <summary>
        /// 创建<see cref="ObjectFactory"/>实例工厂。
        /// </summary>
        /// <param name="instanceType">需创建的类型。</param>
        /// <param name="argumentTypes">参数类型数组。</param>
        /// <returns></returns>
        internal static ObjectFactory CreateFactory(Type instanceType, Type[] argumentTypes) {
            ConstructorInfo constructor;
            int?[] parameterMap;

            FindApplicableConstructor(instanceType, argumentTypes, out constructor, out parameterMap);

            var provider = Expression.Parameter(typeof(IServiceProvider), "provider");
            var argumentArray = Expression.Parameter(typeof(object[]), "argumentArray");
            var factoryExpressionBody = BuildFactoryExpression(constructor, parameterMap, provider, argumentArray);

            var factoryLamda = Expression.Lambda<Func<IServiceProvider, object[], object>>(
                factoryExpressionBody, provider, argumentArray);

            var result = factoryLamda.Compile();
            return result.Invoke;
        }

        /// <summary>
        /// 创建实例化。
        /// </summary>
        /// <typeparam name="T">实例化类型。</typeparam>
        /// <param name="provider">服务提供者。</param>
        /// <param name="parameters">不定参数。</param>
        /// <returns>实例化的类型。</returns>
        public static T CreateInstance<T>(IServiceProvider provider, params object[] parameters) {
            return (T)CreateInstance(provider, typeof(T), parameters);
        }
        /// <summary>
        /// 创建实例化。
        /// </summary>
        /// <typeparam name="T">实例化类型。</typeparam>
        /// <param name="provider">服务提供者。</param>
        /// <returns>实例化的类型。</returns>
        public static T GetServiceOrCreateInstance<T>(IServiceProvider provider) {
            return (T)GetServiceOrCreateInstance(provider, typeof(T));
        }
        /// <summary>
        /// 创建实例化。
        /// </summary>
        /// <param name="provider">服务提供者。</param>
        /// <param name="type">实例化目标类型。</param>
        /// <returns>实例化的类型。</returns>
        public static object GetServiceOrCreateInstance(IServiceProvider provider, Type type) {
            return provider.GetService(type) ?? CreateInstance(provider, type);
        }

        private static MethodInfo GetMethodInfo<T>(Expression<T> expr) {
            var mc = (MethodCallExpression)expr.Body;
            return mc.Method;
        }

        private static object GetService(IServiceProvider sp, Type type, Type requiredBy, bool isDefaultParameterRequired) {
            var service = sp.GetService(type);
            if (service == null && !isDefaultParameterRequired) {
                var message = $"当它试图实例化 '{requiredBy}'类时，无法解析类型{requiredBy}。";
                throw new InvalidOperationException(message);
            }
            return service;
        }

        private static Expression BuildFactoryExpression(
            ConstructorInfo constructor,
            int?[] parameterMap,
            Expression serviceProvider,
            Expression factoryArgumentArray) {
            var constructorParameters = constructor.GetParameters();
            var constructorArguments = new Expression[constructorParameters.Length];

            for (var i = 0; i < constructorParameters.Length; i++) {
                var parameterType = constructorParameters[i].ParameterType;

                if (parameterMap[i] != null) {
                    constructorArguments[i] = Expression.ArrayAccess(factoryArgumentArray, Expression.Constant(parameterMap[i]));
                }
                else {
                    var constructorParameterHasDefault = constructorParameters[i].HasDefaultValue;
                    var parameterTypeExpression = new Expression[] { serviceProvider,
                        Expression.Constant(parameterType, typeof(Type)),
                        Expression.Constant(constructor.DeclaringType, typeof(Type)),
                        Expression.Constant(constructorParameterHasDefault) };
                    constructorArguments[i] = Expression.Call(GetServiceInfo, parameterTypeExpression);
                }

                // Support optional constructor arguments by passing in the default value
                // when the argument would otherwise be null.
                if (constructorParameters[i].HasDefaultValue) {
                    var defaultValueExpression = Expression.Constant(constructorParameters[i].DefaultValue);
                    constructorArguments[i] = Expression.Coalesce(constructorArguments[i], defaultValueExpression);
                }

                constructorArguments[i] = Expression.Convert(constructorArguments[i], parameterType);
            }

            return Expression.New(constructor, constructorArguments);
        }

        private static void FindApplicableConstructor(
            Type instanceType,
            Type[] argumentTypes,
            out ConstructorInfo matchingConstructor,
            out int?[] parameterMap) {
            matchingConstructor = null;
            parameterMap = null;

            foreach (var constructor in instanceType.GetTypeInfo().DeclaredConstructors) {
                if (constructor.IsStatic || !constructor.IsPublic) {
                    continue;
                }

                int?[] tempParameterMap;
                if (TryCreateParameterMap(constructor.GetParameters(), argumentTypes, out tempParameterMap)) {
                    if (matchingConstructor != null) {
                        throw new InvalidOperationException($"已发现多个构造函数接受所有给定的参数类型的类型{instanceType},"
                        + "但这儿只能有一个适用的构造函数。");
                    }

                    matchingConstructor = constructor;
                    parameterMap = tempParameterMap;
                }
            }

            if (matchingConstructor == null) {
                var message = $"一个合适的构造类型{instanceType}不能被找到，确保所有参数类型是具体和服务注册公共构造函数。";
                throw new InvalidOperationException(message);
            }
        }

        // Creates an injective parameterMap from givenParameterTypes to assignable constructorParameters.
        // Returns true if each given parameter type is assignable to a unique; otherwise, false.
        private static bool TryCreateParameterMap(ParameterInfo[] constructorParameters, Type[] argumentTypes, out int?[] parameterMap) {
            parameterMap = new int?[constructorParameters.Length];

            for (var i = 0; i < argumentTypes.Length; i++) {
                var foundMatch = false;
                var givenParameter = argumentTypes[i].GetTypeInfo();

                for (var j = 0; j < constructorParameters.Length; j++) {
                    if (parameterMap[j] != null) {
                        // This ctor parameter has already been matched
                        continue;
                    }

                    if (constructorParameters[j].ParameterType.GetTypeInfo().IsAssignableFrom(givenParameter)) {
                        foundMatch = true;
                        parameterMap[j] = i;
                        break;
                    }
                }

                if (!foundMatch) {
                    return false;
                }
            }

            return true;
        }

        private class ConstructorMatcher {
            private readonly ConstructorInfo _constructor;
            private readonly ParameterInfo[] _parameters;
            private readonly object[] _parameterValues;
            private readonly bool[] _parameterValuesSet;

            public ConstructorMatcher(ConstructorInfo constructor) {
                _constructor = constructor;
                _parameters = _constructor.GetParameters();
                _parameterValuesSet = new bool[_parameters.Length];
                _parameterValues = new object[_parameters.Length];
            }

            public int Match(object[] givenParameters) {

                var applyIndexStart = 0;
                var applyExactLength = 0;
                for (var givenIndex = 0; givenIndex != givenParameters.Length; givenIndex++) {
                    var givenType = givenParameters[givenIndex] == null ? null : givenParameters[givenIndex].GetType().GetTypeInfo();
                    var givenMatched = false;

                    for (var applyIndex = applyIndexStart; givenMatched == false && applyIndex != _parameters.Length; ++applyIndex) {
                        if (_parameterValuesSet[applyIndex] == false &&
                            _parameters[applyIndex].ParameterType.GetTypeInfo().IsAssignableFrom(givenType)) {
                            givenMatched = true;
                            _parameterValuesSet[applyIndex] = true;
                            _parameterValues[applyIndex] = givenParameters[givenIndex];
                            if (applyIndexStart == applyIndex) {
                                applyIndexStart++;
                                if (applyIndex == givenIndex) {
                                    applyExactLength = applyIndex;
                                }
                            }
                        }
                    }

                    if (givenMatched == false) {
                        return -1;
                    }
                }
                return applyExactLength;
            }

            public object CreateInstance(IServiceProvider provider) {
                for (var index = 0; index != _parameters.Length; index++) {
                    if (_parameterValuesSet[index] == false) {
                        var value = provider.GetService(_parameters[index].ParameterType);
                        if (value == null) {
                            if (!_parameters[index].HasDefaultValue) {
                                throw new InvalidOperationException($" 在创建实例'{_constructor.DeclaringType}'时，无法" +
                                $"匹配 ' {_parameters[index].ParameterType}'参数类型。");
                            }
                            else {
                                _parameterValues[index] = _parameters[index].DefaultValue;
                            }
                        }
                        else {
                            _parameterValues[index] = value;
                        }
                    }
                }

                try {
                    return _constructor.Invoke(_parameterValues);
                }
                catch (Exception ex) {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    throw;
                }
            }
        }
    }
}
