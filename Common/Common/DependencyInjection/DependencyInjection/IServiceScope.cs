using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.DependencyInjection {
    /// <summary>
    ///  <see cref="System.IDisposable.Dispose"/> 方法结束范围服务的生命期。 
    ///  <see cref="IServiceScope.ServiceProvider"/>被回收前都已被任何一个范围服务（Scope）解析过。
    /// </summary>
    public interface IServiceScope : IDisposable {

        /// <summary>
        /// <see cref = "IServiceProvider "/>用来解析范围服务（Scope）的依赖项。
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}
