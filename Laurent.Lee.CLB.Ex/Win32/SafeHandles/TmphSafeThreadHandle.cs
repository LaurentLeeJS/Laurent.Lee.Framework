using Microsoft.Win32.SafeHandles;
using System;
using System.Security;

namespace Laurent.Lee.CLB.Win32.SafeHandles
{
    /// <summary>
    /// 线程句柄
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public sealed class TmphSafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// 线程句柄
        /// </summary>
        public TmphSafeThreadHandle() : base(true) { }

        /// <summary>
        /// 设置句柄
        /// </summary>
        /// <param name="handle">线程句柄</param>
        public void InitialSetHandle(IntPtr handle)
        {
            base.SetHandle(handle);
        }

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