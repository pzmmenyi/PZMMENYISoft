using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.DependencyInjection {
    /// <summary>
    /// 提供了一个扩展点来创建指定的建造者和<see cref="IServiceProvider"/>的容器。
    /// </summary>
    public interface IServiceProviderFactory<TContainerBuilder> {

        /// <summary>
        /// 通过 <see cref="IServiceCollection"/>创建<see cref="{TContainerBuilder}"/>。
        /// </summary>
        /// <param name="services"></param>
        /// <returns><see cref="IServiceProvider"/>.</returns>
        TContainerBuilder CreateBuilder(IServiceCollection services);

        /// <summary>
        /// 通过<see cref="{TContainerBuilder}"/>创建 <see cref="IServiceProvider"/> 
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <returns></returns>
        IServiceProvider CreateServiceProvider(TContainerBuilder containerBuilder);
    }
}
