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

using Laurent.Lee.CLB.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Laurent.Lee.CLB.Diagnostics
{
    /// <summary>
    ///     作业
    /// </summary>
    public sealed class TmphJob : IDisposable
    {
        /// <summary>
        ///     限制状态标识
        /// </summary>
        [Flags]
        public enum TmphLimitFlags : uint
        {
            /// <summary>
            ///     活动进程数量限制,JOBOBJECT_BASIC_LIMIT_INFORMATION.ActiveProcessLimit
            /// </summary>
            JobObjectLimitActiveProcess = 0x00000008,

            /// <summary>
            ///     CUP亲和限制,JOBOBJECT_BASIC_LIMIT_INFORMATION.Affinity
            /// </summary>
            JobObjectLimitAffinity = 0x00000010,

            /// <summary>
            ///     子进程关联作业限制
            /// </summary>
            JobObjectLimitBreakawayOk = 0x00000800,

            /// <summary>
            ///     异常处理限制
            /// </summary>
            JobObjectLimitDieOnUnhandledException = 0x00000400,

            /// <summary>
            ///     作业虚拟内存的限制,JOBOBJECT_EXTENDED_LIMIT_INFORMATION.JobMemoryLimit
            /// </summary>
            JobObjectLimitJobMemory = 0x00000200,

            /// <summary>
            ///     作业用时限制,JOBOBJECT_BASIC_LIMIT_INFORMATION.PerJobUserTimeLimit
            /// </summary>
            JobObjectLimitJobTime = 0x00000004,

            /// <summary>
            ///     作业终止时关闭所有进程
            /// </summary>
            JobObjectLimitKillOnJobClose = 0x00002000,

            /// <summary>
            ///     保留作业用时限制,与JOB_OBJECT_LIMIT_JOB_TIME冲突
            /// </summary>
            JobObjectLimitPreserveJobTime = 0x00000040,

            /// <summary>
            ///     优先级修改类型,JOBOBJECT_BASIC_LIMIT_INFORMATION.PriorityClass
            /// </summary>
            JobObjectLimitPriorityClass = 0x00000020,

            /// <summary>
            ///     进程虚拟内存的限制,JOBOBJECT_EXTENDED_LIMIT_INFORMATION.ProcessMemoryLimit
            /// </summary>
            JobObjectLimitProcessMemory = 0x00000100,

            /// <summary>
            ///     进程用时限制,JOBOBJECT_BASIC_LIMIT_INFORMATION.PerProcessUserTimeLimit
            /// </summary>
            JobObjectLimitProcessTime = 0x00000002,

            /// <summary>
            ///     调度限制,JOBOBJECT_BASIC_LIMIT_INFORMATION.SchedulingClass
            /// </summary>
            JobObjectLimitSchedulingClass = 0x00000080,

            /// <summary>
            ///     允许子进程不关联
            /// </summary>
            JobObjectLimitSilentBreakawayOk = 0x00001000,

            /// <summary>
            ///     最小工作集,最大工作集
            /// </summary>
            JobObjectLimitWorkingset = 0x00000001

            ///// <summary>
            ///// 允许CPU亲和进程子集
            ///// </summary>
            //JOB_OBJECT_LIMIT_SUBSET_AFFINITY = 0x00004000
        }

        /// <summary>
        ///     是否已释放
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     句柄
        /// </summary>
        private IntPtr _handle;

        /// <summary>
        ///     作业
        /// </summary>
        public TmphJob()
        {
            _handle = TmphKernel32.CreateJobObject(IntPtr.Zero, null);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Close();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        ///     关闭作业
        /// </summary>
        public void Close()
        {
            TmphKernel32.CloseHandle(_handle);
            _handle = IntPtr.Zero;
        }

        /// <summary>
        ///     添加进程
        /// </summary>
        /// <param name="process">进程</param>
        public void AddProcess(Process process)
        {
            if (!TmphKernel32.AssignProcessToJobObject(_handle, process.Handle))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        ///     设置扩展限制信息
        /// </summary>
        /// <param name="limit">扩展限制信息</param>
        public void SetInformation(TmphExtendedLimitInformation limit)
        {
            var length = Marshal.SizeOf(typeof(TmphExtendedLimitInformation));
            var extendedInfoPtr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(limit, extendedInfoPtr, false);
            if (
                !TmphKernel32.SetInformationJobObject(_handle, TmphInformationType.JobObjectExtendedLimitInformation,
                    extendedInfoPtr, (uint)length))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        ///     信息类型
        /// </summary>
        public enum TmphInformationType
        {
            /// <summary>
            ///     基本限制信息 JOBOBJECT_BASIC_LIMIT_INFORMATION
            /// </summary>
            JobObjectBasicLimitInformation = 2,

            /// <summary>
            ///     基本UI限制
            /// </summary>
            JobObjectBasicUiRestrictions = 4,

            /// <summary>
            ///     安全限制信息
            /// </summary>
            JobObjectSecurityLimitInformation = 5,

            /// <summary>
            ///     结束工作的时间信息
            /// </summary>
            JobObjectEndOfJobTimeInformation = 6,

            /// <summary>
            ///     完成端口信息
            /// </summary>
            JobObjectAssociateCompletionPortInformation = 7,

            /// <summary>
            ///     扩展限制信息 JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            /// </summary>
            JobObjectExtendedLimitInformation = 9

            ///// <summary>
            ///// 组信息
            ///// </summary>
            //JobObjectGroupInformation = 11,
            ///// <summary>
            ///// 通知限制信息
            ///// </summary>
            //JobObjectNotificationLimitInformation = 12,
            ///// <summary>
            ///// 扩展组信息
            ///// </summary>
            //JobObjectGroupInformationEx = 14,
            ///// <summary>
            ///// CPU速率控制信息
            ///// </summary>
            //JobObjectCpuRateControlInformation = 15,
        }

        /// <summary>
        ///     扩展限制信息JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TmphExtendedLimitInformation
        {
            /// <summary>
            ///     基本限制信息
            /// </summary>
            public TmphBasicLimitInformation BasicLimitInformation;

            /// <summary>
            ///     IO计数限制(保留)
            /// </summary>
            public TmphIoCounters IoInfo;

            /// <summary>
            ///     进程虚拟内存的限制,LimitFlags.JOB_OBJECT_LIMIT_PROCESS_MEMORY
            /// </summary>
            public uint ProcessMemoryLimit;

            /// <summary>
            ///     作业虚拟内存的限制,LimitFlags.JOB_OBJECT_LIMIT_JOB_MEMORY
            /// </summary>
            public uint JobMemoryLimit;

            /// <summary>
            ///     进程内存峰值
            /// </summary>
            public uint PeakProcessMemoryUsed;

            /// <summary>
            ///     作业内存峰值
            /// </summary>
            public uint PeakJobMemoryUsed;
        }

        /// <summary>
        ///     基本限制信息JOBOBJECT_BASIC_LIMIT_INFORMATION
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TmphBasicLimitInformation
        {
            /// <summary>
            ///     进程用时限制(单位100纳秒),LimitFlags.JOB_OBJECT_LIMIT_PROCESS_TIME
            /// </summary>
            public long PerProcessUserTimeLimit;

            /// <summary>
            ///     作业用时限制(单位100纳秒),LimitFlags.JOB_OBJECT_LIMIT_JOB_TIME
            /// </summary>
            public long PerJobUserTimeLimit;

            /// <summary>
            ///     限制状态标识
            /// </summary>
            public TmphLimitFlags LimitFlags;

            /// <summary>
            ///     最小工作集,LimitFlags.JOB_OBJECT_LIMIT_WORKINGSET
            /// </summary>
            public UIntPtr MinimumWorkingSetSize;

            /// <summary>
            ///     最大工作集,LimitFlags.JOB_OBJECT_LIMIT_WORKINGSET
            /// </summary>
            public UIntPtr MaximumWorkingSetSize;

            /// <summary>
            ///     活动进程数量限制,LimitFlags.JOB_OBJECT_LIMIT_ACTIVE_PROCESS
            /// </summary>
            public uint ActiveProcessLimit;

            /// <summary>
            ///     CUP亲和限制,必须调用GetProcessAffinityMask,LimitFlags.JOB_OBJECT_LIMIT_AFFINITY
            /// </summary>
            public UIntPtr Affinity;

            /// <summary>
            ///     优先级修改限制,LimitFlags.JOB_OBJECT_LIMIT_PRIORITY_CLASS
            /// </summary>
            public uint PriorityClass;

            /// <summary>
            ///     调度有效值为0到9,LimitFlags.JOB_OBJECT_LIMIT_SCHEDULING_CLASS
            /// </summary>
            public uint SchedulingClass;
        }

        /// <summary>
        ///     IO计数限制IO_COUNTERS
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TmphIoCounters
        {
            /// <summary>
            ///     读取操作次数
            /// </summary>
            public ulong ReadOperationCount;

            /// <summary>
            ///     写入操作次数
            /// </summary>
            public ulong WriteOperationCount;

            /// <summary>
            ///     其它操作次数
            /// </summary>
            public ulong OtherOperationCount;

            /// <summary>
            ///     读取字节数
            /// </summary>
            public ulong ReadTransferCount;

            /// <summary>
            ///     写入字节数
            /// </summary>
            public ulong WriteTransferCount;

            /// <summary>
            ///     其它字节数
            /// </summary>
            public ulong OtherTransferCount;
        }
    }
}