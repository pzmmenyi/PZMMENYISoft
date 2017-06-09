using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.DependencyInjection {
    /// <summary>
    /// 服务集合接口，为<see cref="ServiceDescriptor"/>的集合
    /// </summary
    public interface IServiceCollection : IList<ServiceDescriptor> {

    }
}
