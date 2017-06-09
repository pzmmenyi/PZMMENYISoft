using PZMMENYI.DependencyInjection.Extensions;
using PZMMENYI.Options;
using System;

namespace PZMMENYI.DependencyInjection {

    public static class OptionsServiceCollectionExtensions {
        /// <summary>
        /// 通过依赖注入将选项增加到<see cref="IServiceCollection"/>里。
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOptions(this IServiceCollection services) {
            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAdd(ServiceDescriptor.Singleton(typeof(IOptions<>), typeof(OptionsManager<>)));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(IOptionsMonitor<>), typeof(OptionsMonitor<>)));
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IOptionsSnapshot<>), typeof(OptionsSnapshot<>)));
            return services;
        }
        /// <summary>
        /// 通过依赖注入将选项增加到<see cref="IServiceCollection"/>里。
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection Configure<TOptions>(this IServiceCollection services, Action<TOptions> configureOptions)
            where TOptions : class {
            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null) {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddSingleton<IConfigureOptions<TOptions>>(new ConfigureOptions<TOptions>(configureOptions));
            return services;
        }
    }
}
