using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.DependencyInjection {
    /// <summary>
    /// 类型的服务期
    /// </summary>
    public enum ServiceLifetime {
        /// <summary>
        /// 一个单例，以后每次调用的时候都返回该单例对象。
        /// </summary>
        Singleton,
        /// <summary>
        /// 在当前作用域内，不管调用多少次，都是一个实例。
        /// </summary>
        Scoped,
        /// <summary>
        /// 每次都重新创建一个实例。
        /// </summary>
        Transient
    }
}
