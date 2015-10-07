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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     指定如何实现某方法的详细信息。无法继承此类。
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
    internal sealed class TmphMethodImplAttribute : Attribute
    {
        /// <summary>
        ///     一个 System.Runtime.CompilerServices.MethodCodeType 值，指示为此方法提供了哪种类型的实现。
        /// </summary>
        public MethodCodeType MethodCodeType = MethodCodeType.IL;

        /// <summary>
        /// </summary>
        internal TmphMethodImplOptions Val;

        /// <summary>
        ///     初始化 MethodImplAttribute 类的新实例。
        /// </summary>
        public TmphMethodImplAttribute()
        {
        }

        /// <summary>
        ///     使用指定的 System.Runtime.CompilerServices.TmphMethodImplOptions 值初始化 MethodImplAttribute类的新实例。
        /// </summary>
        /// <param name="methodImplOptions">一个 System.Runtime.CompilerServices.TmphMethodImplOptions 值，该值指定属性化方法的属性。</param>
        public TmphMethodImplAttribute(TmphMethodImplOptions methodImplOptions)
        {
            var options = TmphMethodImplOptions.InternalCall | TmphMethodImplOptions.PreserveSig |
                          TmphMethodImplOptions.NoOptimization | TmphMethodImplOptions.Synchronized |
                          TmphMethodImplOptions.ForwardRef | TmphMethodImplOptions.NoInlining | TmphMethodImplOptions.Unmanaged |
                          TmphMethodImplOptions.AggressiveInlining;
            Val = methodImplOptions & options;
        }

        /// <summary>
        ///     使用指定的 System.Runtime.CompilerServices.TmphMethodImplOptions 值初始化 MethodImplAttribute类的新实例。
        /// </summary>
        /// <param name="value">一个位屏蔽，表示所需的 System.Runtime.CompilerServices.TmphMethodImplOptions 值，该值指定属性化方法的属性。</param>
        public TmphMethodImplAttribute(short value)
        {
            Val = (TmphMethodImplOptions)value;
        }

        /// <summary>
        ///     获取描述属性化方法的 System.Runtime.CompilerServices.TmphMethodImplOptions 值。
        /// </summary>
        public TmphMethodImplOptions Value
        {
            get { return Val; }
        }
    }
}