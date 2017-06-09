using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.DependencyInjection {
    /// <summary>
    /// <see cref="IServiceCollection"/>扩展，通过把<see cref="ServiceDescriptor"/>加入其自身。
    /// 使用参数构造<see cref="ServiceDescriptor"/>并加入到<see cref="IServiceCollection"/>。
    /// 每个方法都对应设置一个<see cref="ServiceLifetime"/>枚举值。
    /// </summary>
    public static class ServiceCollectionServiceExtension {
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient(this IServiceCollection services,
                                                      Type serviceType,
                                                      Type implementationType) {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationType == null) {
                throw new ArgumentNullException(nameof(implementationType));
            }

            return Add(services, serviceType, implementationType, ServiceLifetime.Transient);
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationFactory"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient(this IServiceCollection services,
                                                      Type serviceType,
                                                      Func<IServiceProvider, object> implementationFactory) {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return Add(services, serviceType, implementationFactory, ServiceLifetime.Transient);
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services)
                                                      where TService : class
                                                      where TImplementation : class, TService {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddTransient(typeof(TService), typeof(TImplementation));
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType) {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            return services.AddTransient(serviceType, serviceType);
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services)
                                                      where TService : class {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddTransient(typeof(TService));
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="implementationFactory"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services,
                                                                Func<IServiceProvider, TService> implementationFactory)
                                                      where TService : class {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return services.AddTransient(typeof(TService), implementationFactory);
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="implementationFactory"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services,
                                                      Func<IServiceProvider, TImplementation> implementationFactory)
                                                      where TService : class
                                                      where TImplementation : class, TService {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return services.AddTransient(typeof(TService), implementationFactory);
        }


        /// <summary>
        /// 添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped(this IServiceCollection services,
                                                   Type serviceType,
                                                   Type implementationType) {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationType == null) {
                throw new ArgumentNullException(nameof(implementationType));
            }

            return Add(services, serviceType, implementationType, ServiceLifetime.Scoped);
        }
        /// <summary>
        ///  添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationFactory"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped(this IServiceCollection services,
                                                   Type serviceType,
                                                   Func<IServiceProvider, object> implementationFactory) {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return Add(services, serviceType, implementationFactory, ServiceLifetime.Scoped);
        }
        /// <summary>
        ///  添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services)
                                                   where TService : class
                                                   where TImplementation : class, TService {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddScoped(typeof(TService), typeof(TImplementation));
        }
        /// <summary>
        ///  添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType) {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            return services.AddScoped(serviceType, serviceType);
        }
        /// <summary>
        ///  添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped<TService>(this IServiceCollection services)
                                                   where TService : class {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddScoped(typeof(TService));
        }
        /// <summary>
        ///  添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="implementationFactory"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped<TService>(this IServiceCollection services,
                                                             Func<IServiceProvider, TService> implementationFactory)
                                                   where TService : class {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return services.AddScoped(typeof(TService), implementationFactory);
        }
        /// <summary>
        ///  添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="implementationFactory"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services,
                                                   Func<IServiceProvider, TImplementation> implementationFactory)
                                                   where TService : class
                                                   where TImplementation : class, TService {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return services.AddScoped(typeof(TService), implementationFactory);
        }


        /// <summary>
        ///  添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection services,
                                                      Type serviceType,
                                                      Type implementationType) {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationType == null) {
                throw new ArgumentNullException(nameof(implementationType));
            }

            return Add(services, serviceType, implementationType, ServiceLifetime.Singleton);
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationFactory"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection services,
                                                      Type serviceType,
                                                      Func<IServiceProvider, object> implementationFactory) {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return Add(services, serviceType, implementationFactory, ServiceLifetime.Singleton);
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services)
                                                      where TService : class
                                                      where TImplementation : class, TService {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddSingleton(typeof(TService), typeof(TImplementation));
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection services,
                                                      Type serviceType) {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            return services.AddSingleton(serviceType, serviceType);
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services)
                                                      where TService : class {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddSingleton(typeof(TService));
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="implementationFactory"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services,
                                                                Func<IServiceProvider, TService> implementationFactory)
                                                                where TService : class {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return services.AddSingleton(typeof(TService), implementationFactory);
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="implementationFactory"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services,
                                                      Func<IServiceProvider, TImplementation> implementationFactory)
                                                      where TService : class
                                                      where TImplementation : class, TService {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return services.AddSingleton(typeof(TService), implementationFactory);
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationInstance"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection services,
                                                      Type serviceType,
                                                      object implementationInstance) {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationInstance == null) {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            var serviceDescriptor = new ServiceDescriptor(serviceType, implementationInstance);
            services.Add(serviceDescriptor);

            return services;
        }
        /// <summary>
        /// 添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="implementationInstance"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services,
                                                                TService implementationInstance)
                                                      where TService : class {

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (implementationInstance == null) {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            return services.AddSingleton(typeof(TService), implementationInstance);
        }
        /// <summary>
        /// 添加指定<see cref="ServiceLifetime"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        private static IServiceCollection Add(IServiceCollection collection,
                                              Type serviceType,
                                              Type implementationType,
                                              ServiceLifetime lifetime) {

            var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);
            collection.Add(descriptor);

            return collection;
        }
        /// <summary>
        /// 添加指定<see cref="ServiceLifetime"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationFactory"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        private static IServiceCollection Add(IServiceCollection collection,
                                              Type serviceType,
                                              Func<IServiceProvider, object> implementationFactory,
                                              ServiceLifetime lifetime) {

            var descriptor = new ServiceDescriptor(serviceType, implementationFactory, lifetime);
            collection.Add(descriptor);

            return collection;
        }
    }
}
