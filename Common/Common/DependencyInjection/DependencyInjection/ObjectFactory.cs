using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.DependencyInjection {
    /// <summary>
    /// 对象工厂委托，创建对象，并实例化。
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public delegate object ObjectFactory(IServiceProvider provider, object[] arguments);
}
