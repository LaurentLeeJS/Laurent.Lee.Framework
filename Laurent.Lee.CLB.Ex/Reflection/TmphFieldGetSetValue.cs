/*
-------------------------------------------------- -----------------------------------------
The frame content is protected by copyright law. In order to facilitate individual learning,
allows to download the program source information, but does not allow individuals or a third
party for profit, the commercial use of the source information. Without consent,
does not allow any form (even if partial, or modified) database storage,
copy the source of information. If the source content provided by third parties,
which corresponds to the third party content is also protected by copyright.

If you are found to have infringed copyright behavior, please give me a hint. THX!

Here in particular it emphasized that the third party is not allowed to contact addresses
published in this "version copyright statement" to send advertising material.
I will take legal means to resist sending spam.
-------------------------------------------------- ----------------------------------------
The framework under the GNU agreement, Detail View GNU License.
If you think about this item affection join the development team,
Please contact me: LaurentLeeJS@gmail.com
-------------------------------------------------- ----------------------------------------
Laurent.Lee.Framework Coded by Laurent Lee
*/

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
