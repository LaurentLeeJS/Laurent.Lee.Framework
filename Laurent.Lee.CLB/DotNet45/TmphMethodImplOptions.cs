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
using System.Runtime.InteropServices;

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     定义如何实现某方法的详细信息
    /// </summary>
    [Serializable]
    [Flags]
    [ComVisible(true)]
    internal enum TmphMethodImplOptions
    {
        /// <summary>
        ///     指定此方法是以非托管代码实现的。
        /// </summary>
        Unmanaged = 4,

        /// <summary>
        ///     指定此方法无法被内联。
        /// </summary>
        NoInlining = 8,

        /// <summary>
        ///     指定声明该方法，但其实现在其他地方提供。
        /// </summary>
        ForwardRef = 16,

        /// <summary>
        ///     指定同时只能由一个线程执行该方法。静态方法锁定类型，而实例方法锁定实例。在任何实例函数中只能执行一个线程，并且在类的任何静态函数中只能执行一个线程。
        /// </summary>
        Synchronized = 32,

        /// <summary>
        ///     指定在调试可能的代码生成问题时，该方法不是由实时 (JIT) 编译器或本机代码生成优化的（请参见 Ngen.exe）。
        /// </summary>
        NoOptimization = 64,

        /// <summary>
        ///     指定此方法签名完全按声明的样子导出。
        /// </summary>
        PreserveSig = 128,

        /// <summary>
        ///     指定一个内部调用。内部调用是对在公共语言运行时本身内部实现的方法的调用。
        /// </summary>
        InternalCall = 4096,

        /// <summary>
        ///     如果可能方法应内联。
        /// </summary>
        AggressiveInlining = 0
    }
}