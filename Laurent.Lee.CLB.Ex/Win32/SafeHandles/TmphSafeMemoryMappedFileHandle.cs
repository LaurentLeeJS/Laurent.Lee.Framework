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