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
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace Laurent.Lee.CLB.Win32.SafeHandles
{
    /// <summary>
    /// 安全缓冲区
    /// </summary>
    [SecurityCritical]
    public abstract class TmphSafeBuffer : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// 缓冲区长度错误值
        /// </summary>
        private static readonly UIntPtr errorLength = UIntPtr.Size == 4 ? (UIntPtr)uint.MaxValue : (UIntPtr)ulong.MaxValue;

        /// <summary>
        /// 缓冲区长度
        /// </summary>
        private UIntPtr bufferLength;

        /// <summary>
        /// 安全缓冲区
        /// </summary>
        /// <param name="ownsHandle">是否拥有句柄</param>
        protected TmphSafeBuffer(bool ownsHandle)
            : base(ownsHandle)
        {
            bufferLength = errorLength;
        }

        /// <summary>
        /// 设置缓冲区长度
        /// </summary>
        /// <param name="numBytes">缓冲区长度</param>
        public void Initialize(ulong bufferLength)
        {
            this.bufferLength = (UIntPtr)bufferLength;
        }

        /// <summary>
        /// 获取缓冲区指针
        /// </summary>
        /// <returns>缓冲区指针</returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public unsafe byte* AcquirePointer()
        {
            bool success = false;
            base.DangerousAddRef(ref success);
            return success ? (byte*)base.handle : null;
        }

        /// <summary>
        /// 释放缓冲区指针
        /// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void ReleasePointer()
        {
            base.DangerousRelease();
        }
    }
}