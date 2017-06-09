using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.DependencyInjection {
    /// <summary>
    /// 服务描述。
    /// </summary>
    public class ServiceDescriptor {
        /// <summary>
        /// 生命周期。
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// 服务类型，记录基类类型。
        /// <summary>
        public Type ServiceType { get; }

        /// <summary>
        /// 要实现的类型，继承<see cref="ServiceDescriptor.ServiceType"/>类型。如果有多个继承值，
        /// 但只在<see cref="ServiceCollection"/>注册类型里返回最后的类型。
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// 实现了<see cref="ServiceDescriptor.ServiceType"/>类型的对象，用此属性注册必须时单例的生命周期。
        /// </summary>
        public object ImplementationInstance { get; }

        /// <summary>
        /// 实现工厂。通过<see cref="IServiceProvider"/>返回<see cref="ServiceDescriptor.ImplementationInstance"/>的<see cref="Func{T, TResult}"/>委托。
        /// </summary>
        public Func<IServiceProvider, object> ImplementationFactory { get; }
        /// <summary>
        /// 获得<see cref="ImplementationType"/>、<see cref="ImplementationInstance"/>、<see cref="ImplementationFactory"/>
        /// 的<see cref="Type"/>类型。
        /// </summary>
        /// <returns></returns>
        internal Type GetImplementationType() {

            if (ImplementationType != null) {

                return ImplementationType;
            }
            if (ImplementationInstance != null) {

                return ImplementationInstance.GetType();
            }
            if (ImplementationFactory != null) {

                var Typearguments = ImplementationFactory.GetType().GenericTypeArguments;
                if(Typearguments.Length != 2) {
                    throw new Exception("ImplementationFactory只能提供两个泛型");
                }

                return Typearguments[1];
            }

            return null;
        }
        /// <summary>
        /// 通过<paramref name="implementationType">实例化<see cref="ServiceDescriptor"/>。
        /// </summary>
        /// <param name="serviceType">服务类型，记录基类类型。</param>
        /// <param name="implementationType">要实现的类型，继承<see cref="ServiceDescriptor.ServiceType"/>类型。</param>
        /// <param name="lifetime">生命期。</param>
        public ServiceDescriptor(Type serviceType,
                                 Type implementationType,
                                 ServiceLifetime lifetime)
                                 : this(serviceType, lifetime) {

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationType == null) {
                throw new ArgumentNullException(nameof(implementationType));
            }

            ImplementationType = implementationType;
        }

        /// <summary>
        /// 通过<paramref name="instance">实例化<see cref="ServiceDescriptor"/>。
        /// 生命期为<see cref="ServiceLifetime.Singleton"/>模式。
        /// </summary>
        /// <param name="serviceType">服务类型，记录基类类型。</param>
        /// <param name="instance"> 实现了<see cref="ServiceDescriptor.ServiceType"/>类型的对象。</param>
        public ServiceDescriptor(Type serviceType, object instance)
                                : this(serviceType, ServiceLifetime.Singleton) {

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            ImplementationInstance = instance;
        }

        /// <summary>
        /// 通过<paramref name="factory">实例化<see cref="ServiceDescriptor"/>。
        /// </summary>
        /// <param name="serviceType">服务类型，记录基类类型。</param>
        /// <param name="factory"> 实现工厂。通过<see cref="IServiceProvider"/>返回<see cref="ServiceDescriptor.ImplementationInstance"/>的<see cref="Func{T, TResult}"/>委托。</param>
        /// <param name="lifetime">生命期。</param>
        public ServiceDescriptor(Type serviceType,
                                 Func<IServiceProvider, object> factory,
                                 ServiceLifetime lifetime)
                                 : this(serviceType, lifetime) {

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (factory == null) {
                throw new ArgumentNullException(nameof(factory));
            }

            ImplementationFactory = factory;
        }

        private ServiceDescriptor(Type serviceType, ServiceLifetime lifetime) {

            ServiceType = serviceType;
            Lifetime = lifetime;
        }
        #region 静态创建ServiceDescriptor类型。通过 ServiceLifetime 创建Transient，Scoped,Singleton。
        /// <summary>
        /// 创建生命期为<see cref="ServiceLifetime.Transient"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation">要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。</typeparam>
        /// <returns></returns>
        public static ServiceDescriptor Transient<TService, TImplementation>()
                                                  where TService : class
                                                  where TImplementation : class, TService {

            return Describe<TService, TImplementation>(ServiceLifetime.Transient);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceLifetime.Transient"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>
        /// </summary>
        /// <param name="service"></param>
        /// <param name="implementationType">>要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。</param>
        /// <returns></returns>
        public static ServiceDescriptor Transient(Type service, Type implementationType) {

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationType == null) {
                throw new ArgumentNullException(nameof(implementationType));
            }

            return Describe(service, implementationType, ServiceLifetime.Transient);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceLifetime.Transient"/>的实例，
        /// 通过<see cref="ServiceDescriptor.ImplementationFactory"/>属性。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation">要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。</typeparam>
        /// <param name="implementationFactory">实现工厂。通过<see cref="IServiceProvider"/>返回<see cref="object"/>的<see cref="Func{T, TResult}"/>委托。</param>
        /// <returns></returns>
        public static ServiceDescriptor Transient<TService, TImplementation>(
                                                 Func<IServiceProvider, TImplementation> implementationFactory)
                                                 where TService : class
                                                 where TImplementation : class, TService {

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return Describe(typeof(TService), implementationFactory, ServiceLifetime.Transient);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceLifetime.Transient"/>的实例，
        /// 通过<see cref="ServiceDescriptor.ImplementationFactory"/>属性。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="implementationFactory">实现工厂。通过<see cref="IServiceProvider"/>返回<see cref="{TService}"/>的<see cref="Func{T, TResult}"/>委托。</param>
        /// <returns></returns>
        public static ServiceDescriptor Transient<TService>(Func<IServiceProvider, TService> implementationFactory)
                                                  where TService : class {

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

                return Describe(typeof(TService), implementationFactory, ServiceLifetime.Transient);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceLifetime.Transient"/>的实例，
        /// 通过<see cref="ServiceDescriptor.ImplementationFactory"/>属性。
        /// </summary>
        /// <param name="service"></param>
        /// <param name="implementationFactory">实现工厂。通过<see cref="IServiceProvider"/>返回<see cref="object"/>的<see cref="Func{T, TResult}"/>委托。</param>
        /// <returns></returns>
        public static ServiceDescriptor Transient(Type service,
                                                  Func<IServiceProvider, object> implementationFactory) {

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return Describe(service, implementationFactory, ServiceLifetime.Transient);
        }
        /// <summary>
        ///  创建生命期为<see cref="ServiceLifetime.Scoped"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation">要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。</typeparam>
        /// <returns></returns>
        public static ServiceDescriptor Scoped<TService, TImplementation>()
                                               where TService : class
                                               where TImplementation : class, TService {

            return Describe<TService, TImplementation>(ServiceLifetime.Scoped);
        }
        /// <summary>
        ///  创建生命期为<see cref="ServiceLifetime.Scoped"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。
        /// </summary>
        /// <param name="service"></param>
        /// <param name="implementationType">要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。</param>
        /// <returns></returns>
        public static ServiceDescriptor Scoped(Type service, Type implementationType) {
            return Describe(service, implementationType, ServiceLifetime.Scoped);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceLifetime.Scoped"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationFactory"/>。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation">要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。</typeparam>
        /// <param name="implementationFactory">实现工厂。通过<see cref="IServiceProvider"/>返回<see cref="ServiceDescriptor.ImplementationType"/>的<see cref="Func{T, TResult}"/>委托。</param>
        /// <returns></returns>
        public static ServiceDescriptor Scoped<TService, TImplementation>(
                                               Func<IServiceProvider, TImplementation> implementationFactory)
                                               where TService : class
                                               where TImplementation : class, TService {

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return Describe(typeof(TService), implementationFactory, ServiceLifetime.Scoped);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceLifetime.Scoped"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationFactory"/>。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="implementationFactory">实现工厂。通过<see cref="IServiceProvider"/>返回返回<see cref="{TService}"/>的<see cref="Func{T, TResult}"/>委托。</param>
        /// <returns></returns>
        public static ServiceDescriptor Scoped<TService>(Func<IServiceProvider, TService> implementationFactory)
                                               where TService : class {

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return Describe(typeof(TService), implementationFactory, ServiceLifetime.Scoped);
        }
        /// <summary>
        ///  创建生命期为<see cref="ServiceLifetime.Scoped"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationFactory"/>。
        /// </summary>
        /// <param name="service"></param>
        /// <param name="implementationFactory">实现工厂。通过<see cref="IServiceProvider"/>返回<see cref="ServiceDescriptor.ImplementationInstance"/>的<see cref="Func{T, TResult}"/>委托。</param>
        /// <returns></returns>
        public static ServiceDescriptor Scoped(Type service,
                                               Func<IServiceProvider, object> implementationFactory) {

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return Describe(service, implementationFactory, ServiceLifetime.Scoped);
        }
        /// <summary>
        ///  创建生命期为<see cref="ServiceLifetime.Singleton"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.GetImplementationType"/>。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation">要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。</typeparam>
        /// <returns></returns>
        public static ServiceDescriptor Singleton<TService, TImplementation>()
                                                  where TService : class
                                                  where TImplementation : class, TService {

            return Describe<TService, TImplementation>(ServiceLifetime.Singleton);
        }
        /// <summary>
        ///  创建生命期为<see cref="ServiceLifetime.Singleton"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.GetImplementationType"/>。
        /// </summary>
        /// <param name="service"></param>
        /// <param name="implementationType">要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。</param>
        /// <returns></returns>
        public static ServiceDescriptor Singleton(Type service, Type implementationType) {

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationType == null) {
                throw new ArgumentNullException(nameof(implementationType));
            }

            return Describe(service, implementationType, ServiceLifetime.Singleton);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceLifetime.Singleton"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationFactory"/>。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation">要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。</typeparam>
        /// <param name="implementationFactory">实现工厂。通过<see cref="IServiceProvider"/>返回<see cref="ServiceDescriptor.ImplementationInstance"/>的<see cref="Func{T, TResult}"/>委托。</param>
        /// <returns></returns>
        public static ServiceDescriptor Singleton<TService, TImplementation>(
                                                  Func<IServiceProvider, TImplementation> implementationFactory)
                                                  where TService : class
                                                  where TImplementation : class, TService {

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return Describe(typeof(TService), implementationFactory, ServiceLifetime.Singleton);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceLifetime.Singleton"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationFactory"/>。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="implementationFactory">实现工厂。通过<see cref="IServiceProvider"/>返回返回<see cref="{TService}"/>的<see cref="Func{T, TResult}"/>委托。</param>
        /// <returns></returns>
        public static ServiceDescriptor Singleton<TService>(Func<IServiceProvider, TService> implementationFactory)
                                                  where TService : class {

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return Describe(typeof(TService), implementationFactory, ServiceLifetime.Singleton);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceLifetime.Singleton"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationFactory"/>。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationFactory">实现工厂。通过<see cref="IServiceProvider"/>返回<see cref="object"/>的<see cref="Func{T, TResult}"/>委托。</param>
        /// <returns></returns>
        public static ServiceDescriptor Singleton(Type serviceType,
                                                  Func<IServiceProvider, object> implementationFactory) {


            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            return Describe(serviceType, implementationFactory, ServiceLifetime.Singleton);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceLifetime.Singleton"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationInstance"/>。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="implementationInstance"> 实现了<see cref="ServiceDescriptor.ServiceType"/>类型的对象，用此属性注册必须时单例的生命周期。</param>
        /// <returns></returns>
        public static ServiceDescriptor Singleton<TService>(TService implementationInstance)
                                                  where TService : class {

            if (implementationInstance == null) {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            return Singleton(typeof(TService), implementationInstance);
        }
        /// <summary>
        ///  创建生命期为<see cref="ServiceLifetime.Singleton"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationInstance"/>。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationInstance"> 实现了<see cref="ServiceDescriptor.ServiceType"/>类型的对象，用此属性注册必须时单例的生命周期。</param>
        /// <returns></returns>
        public static ServiceDescriptor Singleton(Type serviceType,
                                                  object implementationInstance) {

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationInstance == null) {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            return new ServiceDescriptor(serviceType, implementationInstance);
        }
        /// <summary>
        ///  创建生命期为<see cref="ServiceDescriptor"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation">要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。</typeparam>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        private static ServiceDescriptor Describe<TService, TImplementation>(ServiceLifetime lifetime)
                                                  where TService : class
                                                  where TImplementation : class, TService {

            return Describe(typeof(TService), typeof(TImplementation), lifetime: lifetime);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceDescriptor"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType">要实现的类型<see cref="ServiceDescriptor.ImplementationType"/>。</param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static ServiceDescriptor Describe(Type serviceType,
                                                 Type implementationType,
                                                 ServiceLifetime lifetime) {

            return new ServiceDescriptor(serviceType, implementationType, lifetime);
        }
        /// <summary>
        /// 创建生命期为<see cref="ServiceDescriptor"/>的实例，
        /// 通过要实现的类型<see cref="ServiceDescriptor.ImplementationFactory"/>。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationFactory">实现工厂。通过<see cref="IServiceProvider"/>返回<see cref="object"/>的<see cref="Func{T, TResult}"/>委托。</param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static ServiceDescriptor Describe(Type serviceType,
                                                 Func<IServiceProvider, object> implementationFactory,
                                                 ServiceLifetime lifetime) {

            return new ServiceDescriptor(serviceType, implementationFactory, lifetime);
        }
        #endregion 静态创建ServiceDescriptor类型。通过 ServiceLifetime 创建Transient，Scoped,Singleton。
    }
}
