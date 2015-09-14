using System;
using System.Reflection;

namespace Laurent.Lee.CLB.Reflection
{
    /// <summary>
    /// 字段获取&设置器
    /// </summary>
    public sealed class TmphFieldGetSetValue
    {
        /// <summary>
        /// 字段获取器
        /// </summary>
        public Func<object, object> Getter;
        /// <summary>
        /// 字段获取器
        /// </summary>
        public Action<object, object> Setter;
        /// <summary>
        /// 字段获取&设置器 创建器
        /// </summary>
        public sealed class TmphCreateFieldGetSetValue : TmphCreateField<TmphFieldGetSetValue>
        {
            /// <summary>
            /// 创建字段获取&设置器
            /// </summary>
            /// <param name="field">字段信息</param>
            /// <returns>字段获取&设置器</returns>
            public override TmphFieldGetSetValue Create(FieldInfo field)
            {
                if (field != null)
                {
                    return new TmphFieldGetSetValue { Getter = TmphFieldGetValue.Creator.Create(field), Setter = TmphFieldSetValue.Creator.Create(field) };
                }
                return default(TmphFieldGetSetValue);
            }
        }
        /// <summary>
        /// 字段获取&设置器 创建器
        /// </summary>
        public static readonly TmphCreateFieldGetSetValue Creator = new TmphCreateFieldGetSetValue();
    }
}
