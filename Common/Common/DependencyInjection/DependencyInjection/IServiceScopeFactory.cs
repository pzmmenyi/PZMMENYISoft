using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.DependencyInjection {
    /// <summary>
    /// <see cref="IServiceScope"/>工厂接口。只含有<see cref="IServiceScopeFactory.CreateScope()"/>唯一方法。
    /// </summary>
    public interface IServiceScopeFactory {

        /// <summary>
        /// 创建<see cref="IServiceScope"/>类型。
        /// </summary>
        /// <returns></returns>
        IServiceScope CreateScope();
    }
}
