using Microsoft.Win32.SafeHandles;
using System;
using System.Security;
using System.Security.Permissions;

namespace Laurent.Lee.CLB.Win32.SafeHandles
{
    /// <summary>
    /// 内存句柄
    /// </summary>
    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public sealed class TmphSafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// 内存句柄
        /// </summary>
        public TmphSafeLocalMemHandle() : base(true) { }

        /// <summary>
        /// 内存句柄
        /// </summary>
        /// <param name="existingHandle">内存句柄</param>
        /// <param name="ownsHandle">是否拥有句柄</param>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public TmphSafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle)
            : base(ownsHandle)
        {
            base.SetHandle(existingHandle);
        }

        /// <summary>
        /// 释放句柄
        /// </summary>
        /// <returns>是否成功</returns>
        protected override bool ReleaseHandle()
        {
            return TmphKernel32.LocalFree(base.handle) == IntPtr.Zero;
        }
    }
}