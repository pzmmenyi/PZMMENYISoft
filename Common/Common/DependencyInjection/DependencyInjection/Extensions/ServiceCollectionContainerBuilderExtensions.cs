using System;

namespace PZMMENYI.DependencyInjection {
    /// <summary>
    /// <see cref="ServiceCollection"/>容器建造者扩展。
    /// </summary>
    public static class ServiceCollectionContainerBuilderExtension {
        /// <summary>
        /// 创建<see cref="ServiceProvider"/>，通过自身，不开启范围服务验证。
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceProvider BuildServiceProvider(this IServiceCollection services) {

            return BuildServiceProvider(services, validateScopes: false);
        }
        /// <summary>
        /// 创建<see cref="ServiceProvider"/>，通过自身。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="validateScopes">是否开启范围服务验证。</param>
        /// <returns></returns>
        public static IServiceProvider BuildServiceProvider(this IServiceCollection services, bool validateScopes) {

            return new ServiceProvider(services, validateScopes);
        }
    }
}
