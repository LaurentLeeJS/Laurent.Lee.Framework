using System;
using System.Security.Permissions;

namespace Laurent.Lee.CLB.Win32.SafeHandles
{
    /// <summary>
    /// 内存映射文件视图
    /// </summary>
    //[SecurityCritical(SecurityCriticalScope.Everything)]
    public sealed class TmphSafeMemoryMappedViewHandle : TmphSafeBuffer
    {
        /// <summary>
        /// 内存映射文件视图
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public TmphSafeMemoryMappedViewHandle() : base(true) { }

        /// <summary>
        /// 内存映射文件视图
        /// </summary>
        /// <param name="handle">内存句柄</param>
        /// <param name="ownsHandle">是否拥有句柄</param>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public TmphSafeMemoryMappedViewHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            base.SetHandle(handle);
        }

        /// <summary>
        /// 释放内存映射文件视图句柄
        /// </summary>
        /// <returns>是否成功</returns>
        protected override bool ReleaseHandle()
        {
            if (TmphKernel32.UnmapViewOfFile(base.handle))
            {
                base.handle = IntPtr.Zero;
                return true;
            }
            return false;
        }
    }
}