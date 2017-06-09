using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.DependencyInjection {
    /// <summary>
    ///  如果由<see cref="IServiceProvider"/>支持，就使用
    /// <see cref="ServiceProviderServiceExtension.GetRequiredService{T}(IServiceProvider)"/>去解析服务。
    /// </summary>
    public interface ISupportRequiredService {

        /// <summary>
        ///  通过 <see cref="IServiceProvider"/> 接口的实例获得<paramref name="serviceType"/>类型的对象。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns>创建<paramref name="serviceType"/>类型的实例。
        /// 如果<see cref="IServiceProvider"/>不能创建对象时，抛出异常。</returns>
        object GetRequiredService(Type serviceType);
    }
}
