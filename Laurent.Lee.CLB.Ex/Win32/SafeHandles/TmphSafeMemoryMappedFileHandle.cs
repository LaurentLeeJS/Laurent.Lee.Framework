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

using Microsoft.Win32.SafeHandles;
using System;
using System.Security.Permissions;

namespace Laurent.Lee.CLB.Win32.SafeHandles
{
    /// <summary>
    /// 内存映射文件
    /// </summary>
    //[SecurityCritical(SecurityCriticalScope.Everything)]
    public sealed class TmphSafeMemoryMappedFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// 内存映射文件
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public TmphSafeMemoryMappedFileHandle() : base(true) { }

        /// <summary>
        /// 内存映射文件
        /// </summary>
        /// <param name="existingHandle">内存句柄</param>
        /// <param name="ownsHandle">是否拥有句柄</param>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public TmphSafeMemoryMappedFileHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        { base.SetHandle(handle); }

        /// <summary>
        /// 释放句柄
        /// </summary>
        /// <returns>是否成功</returns>
        protected override bool ReleaseHandle()
        {
            return TmphKernel32.CloseHandle(base.handle);
        }
    }
}