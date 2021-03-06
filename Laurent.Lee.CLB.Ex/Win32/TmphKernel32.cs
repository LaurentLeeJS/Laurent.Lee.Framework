﻿/*
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

using Laurent.Lee.CLB.Win32.SafeHandles;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace Laurent.Lee.CLB.Win32
{
    /// <summary>
    /// kernel32.dll API
    /// </summary>
    public static class TmphKernel32
    {
        /// <summary>
        /// 内存复制
        /// </summary>
        /// <param name="dest">目标位置</param>
        /// <param name="src">源位置</param>
        /// <param name="length">字节长度</param>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static unsafe extern void RtlMoveMemory(void* dest, void* src, int length);

        /// <summary>
        /// 错误句柄值
        /// </summary>
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        /// <summary>
        /// 释放句柄
        /// </summary>
        /// <param name="handle">句柄</param>
        /// <returns>是否成功</returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// 释放内存
        /// </summary>
        /// <param name="hMem">内存句柄</param>
        /// <returns>IntPtr.Zero表示成功</returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll")]
        public static extern IntPtr LocalFree(IntPtr hMem);

        /// <summary>
        /// 进程附加到作业
        /// </summary>
        /// <param name="job">作业句柄</param>
        /// <param name="process">进程句柄</param>
        /// <returns>是否成功</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        /// <summary>
        /// 设置作业信息
        /// </summary>
        /// <param name="hJob">作业句柄</param>
        /// <param name="infoType">信息类型</param>
        /// <param name="lpJobObjectInfo">信息句柄</param>
        /// <param name="cbJobObjectInfoLength">信息字节长度</param>
        /// <returns>是否成功</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetInformationJobObject(IntPtr hJob, Diagnostics.TmphJob.TmphInformationType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        /// <summary>
        /// 创建作业
        /// </summary>
        /// <param name="lpJobAttributes">安全属性SECURITY_ATTRIBUTES</param>
        /// <param name="lpName">作业名称</param>
        /// <returns>作业句柄</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

        /// <summary>
        /// 创建进程
        /// </summary>
        /// <param name="lpApplicationName">应用名称,可以null</param>
        /// <param name="lpCommandLine">命令行</param>
        /// <param name="lpProcessAttributes">进程安全属性</param>
        /// <param name="lpThreadAttributes">线程安全属性</param>
        /// <param name="bInheritHandles">是否继承句柄</param>
        /// <param name="dwCreationFlags">创建状态标识</param>
        /// <param name="lpEnvironment">环境变量</param>
        /// <param name="lpCurrentDirectory">工作目录</param>
        /// <param name="lpStartupInfo">启动信息</param>
        /// <param name="lpProcessInformation">进程信息</param>
        /// <returns>是否成功</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreateProcess([MarshalAs(UnmanagedType.LPTStr)] string lpApplicationName, string lpCommandLine, TmphSecurityAttributes lpProcessAttributes, TmphSecurityAttributes lpThreadAttributes, bool bInheritHandles, Diagnostics.TmphProcess.TmphCreateFlags dwCreationFlags, IntPtr lpEnvironment, [MarshalAs(UnmanagedType.LPTStr)] string lpCurrentDirectory, Diagnostics.TmphProcess.TmphStartupInfo lpStartupInfo, Diagnostics.TmphProcess.TmphProcessInformation lpProcessInformation);

        /// <summary>
        /// 唤醒线程
        /// </summary>
        /// <param name="hThread">线程句柄</param>
        /// <returns>负数表示失败</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ResumeThread(IntPtr hThread);

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="hReadPipe">读取通道</param>
        /// <param name="hWritePipe">写入通道</param>
        /// <param name="lpPipeAttributes">安全属性</param>
        /// <param name="nSize">缓存区字节数,0表示默认</param>
        /// <returns>是否成功</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, TmphSecurityAttributes lpPipeAttributes, int nSize);

        /// <summary>
        /// 复制句柄
        /// </summary>
        /// <param name="hSourceProcessHandle">被复制进程句柄,必须有PROCESS_DUP_HANDLE权限</param>
        /// <param name="hSourceHandle">被复制句柄</param>
        /// <param name="hTargetProcess">目标进程句柄，必须有PROCESS_DUP_HANDLE权限</param>
        /// <param name="targetHandle">目标句柄</param>
        /// <param name="dwDesiredAccess">访问权限标识</param>
        /// <param name="bInheritHandle">是否继承</param>
        /// <param name="dwOptions">可选参数</param>
        /// <returns>是否成功</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool DuplicateHandle(HandleRef hSourceProcessHandle, SafeHandle hSourceHandle, HandleRef hTargetProcess, out SafeFileHandle targetHandle, int dwDesiredAccess, bool bInheritHandle, TmphDuplicateHandleOptions dwOptions);

        /// <summary>
        /// 获取当前进程句柄
        /// </summary>
        /// <returns>当前进程句柄</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        /// <summary>
        /// 获取控制台输入编码
        /// </summary>
        /// <returns>Encoding编码</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetConsoleCP();

        /// <summary>
        /// 获取控制台输出编码
        /// </summary>
        /// <returns>Encoding编码</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetConsoleOutputCP();

        /// <summary>
        /// 获取标准输入输出句柄
        /// </summary>
        /// <param name="whichHandle"></param>
        /// <returns>句柄</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetStdHandle(TmphStandardHandle whichHandle);

        /// <summary>
        /// 创建内存映射文件
        /// </summary>
        /// <param name="hFile">文件句柄</param>
        /// <param name="lpAttributes">安全属性</param>
        /// <param name="fProtect"></param>
        /// <param name="dwMaximumSizeHigh">文件大小高32位</param>
        /// <param name="dwMaximumSizeLow">文件大小低32位</param>
        /// <param name="lpName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern TmphSafeMemoryMappedFileHandle CreateFileMapping(SafeFileHandle hFile, TmphSecurityAttributes lpAttributes, int fProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, string lpName);

        /// <summary>
        /// 打开内存映射文件
        /// </summary>
        /// <param name="dwDesiredAccess">访问类型</param>
        /// <param name="bInheritHandle">是否继承句柄</param>
        /// <param name="lpName">标识名称</param>
        /// <returns>内存映射文件句柄</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern TmphSafeMemoryMappedFileHandle OpenFileMapping(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);

        /// <summary>
        /// 获取内存映射文件视图
        /// </summary>
        /// <param name="handle">内存映射文件句柄</param>
        /// <param name="dwDesiredAccess">访问类型</param>
        /// <param name="dwFileOffsetHigh">文件大小高32位</param>
        /// <param name="dwFileOffsetLow">文件大小低32位</param>
        /// <param name="dwNumberOfBytesToMap">视图大小</param>
        /// <returns>内存映射文件视图</returns>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern TmphSafeMemoryMappedViewHandle MapViewOfFile(TmphSafeMemoryMappedFileHandle handle, int dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, UIntPtr dwNumberOfBytesToMap);

        /// <summary>
        /// 释放内存映射文件视图
        /// </summary>
        /// <param name="lpBaseAddress">内存映射文件视图句柄</param>
        /// <returns>是否成功</returns>
        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        /// <summary>
        /// 申请内存
        /// </summary>
        /// <param name="address">内存映射文件视图句柄</param>
        /// <param name="buffer">内存基本信息</param>
        /// <param name="sizeOfBuffer">内存基本信息结构大小</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualQuery(TmphSafeMemoryMappedViewHandle address, ref TmphMemoryBasicInformation buffer, IntPtr sizeOfBuffer);

        /// <summary>
        /// 申请内存
        /// </summary>
        /// <param name="address">内存映射文件视图句柄</param>
        /// <param name="numBytes">区域大小</param>
        /// <param name="commitOrReserve"></param>
        /// <param name="pageProtectionMode">访问类型</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc(TmphSafeMemoryMappedViewHandle address, UIntPtr numBytes, int commitOrReserve, int pageProtectionMode);

        /// <summary>
        /// 获取系统信息
        /// </summary>
        /// <param name="lpSystemInfo">系统信息</param>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void GetSystemInfo(ref TmphSystemInfo lpSystemInfo);

        /// <summary>
        /// 获取扩展内存状态信息
        /// </summary>
        /// <param name="lpBuffer">扩展内存状态信息</param>
        /// <returns>是否成功</returns>
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx([In, Out] TmphMemoryStatuExpand lpBuffer);

        /// <summary>
        /// 系统信息SYSTEM_INFO
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TmphSystemInfo
        {
            /// <summary>
            /// 系统OEM标识
            /// </summary>
            public int OemId;

            /// <summary>
            /// 内存分页大小
            /// </summary>
            public int PageSize;

            /// <summary>
            /// 应用程序最小地址
            /// </summary>
            public IntPtr MinimumApplicationAddress;

            /// <summary>
            /// 应用程序最大地址
            /// </summary>
            public IntPtr MaximumApplicationAddress;

            /// <summary>
            /// 活动处理器
            /// </summary>
            public IntPtr ActiveProcessorMask;

            /// <summary>
            /// 处理器数量
            /// </summary>
            public int NumberOfProcessors;

            /// <summary>
            /// 处理器类型
            /// </summary>
            public int ProcessorType;

            /// <summary>
            /// 内存分配粒度
            /// </summary>
            public int AllocationGranularity;

            /// <summary>
            /// 处理器分级数量
            /// </summary>
            public short ProcessorLevel;

            /// <summary>
            /// 处理器
            /// </summary>
            public short ProcessorRevision;
        }

        /// <summary>
        /// 扩展内存状态信息MEMORYSTATUEX
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class TmphMemoryStatuExpand
        {
            /// <summary>
            /// 扩展内存状态信息结构体长度
            /// </summary>
            public uint Length;

            /// <summary>
            ///
            /// </summary>
            public uint MemoryLoad;

            /// <summary>
            /// 物理内存大小
            /// </summary>
            public ulong TotalPhys;

            /// <summary>
            ///
            /// </summary>
            public ulong AvailPhys;

            /// <summary>
            ///
            /// </summary>
            public ulong TotalPageFile;

            /// <summary>
            ///
            /// </summary>
            public ulong AvailPageFile;

            /// <summary>
            ///
            /// </summary>
            public ulong TotalVirtual;

            /// <summary>
            ///
            /// </summary>
            public ulong AvailVirtual;

            /// <summary>
            ///
            /// </summary>
            public ulong AvailExtendedVirtual;

            /// <summary>
            /// 扩展内存状态信息
            /// </summary>
            public TmphMemoryStatuExpand()
            {
                Length = (uint)Marshal.SizeOf(typeof(TmphMemoryStatuExpand));
            }
        }

        /// <summary>
        /// 基本内存信息MEMORYBASICINFORMATION
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TmphMemoryBasicInformation
        {
            public unsafe void* BaseAddress;
            public unsafe void* AllocationBase;
            public uint AllocationProtect;

            /// <summary>
            /// 区域大小
            /// </summary>
            public UIntPtr RegionSize;

            /// <summary>
            /// 内存状态
            /// </summary>
            public uint State;

            public uint Protect;
            public uint Type;
        }

        /// <summary>
        /// 安全属性SECURITY_ATTRIBUTES
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class TmphSecurityAttributes
        {
            /// <summary>
            /// securityAttributes所占内存大小
            /// </summary>
            public int Size;

            /// <summary>
            /// 安全描述
            /// </summary>
            public TmphSafeLocalMemHandle SecurityDescriptor;

            /// <summary>
            /// 是否继承句柄
            /// </summary>
            public bool IsInheritHandle;

            /// <summary>
            /// 安全属性
            /// </summary>
            public TmphSecurityAttributes()
            {
                Size = Marshal.SizeOf(typeof(TmphSecurityAttributes));
                SecurityDescriptor = new TmphSafeLocalMemHandle(IntPtr.Zero, false);
            }
        }

        /// <summary>
        /// 复制句柄可选参数
        /// </summary>
        [Flags]
        public enum TmphDuplicateHandleOptions : int
        {
            NONE = 0,

            /// <summary>
            /// 关闭源句柄
            /// </summary>
            DUPLICATE_CLOSE_SOURCE = 1,

            /// <summary>
            /// 忽略dwDesiredAccess，设置同样的访问权限
            /// </summary>
            DUPLICATE_SAME_ACCESS = 2
        }

        /// <summary>
        /// 获取标准输入输出句柄类型
        /// </summary>
        public enum TmphStandardHandle : int
        {
            /// <summary>
            /// 标准输入
            /// </summary>
            StandardInput = -10,

            /// <summary>
            /// 标准输出
            /// </summary>
            StandardOutput = -11,

            /// <summary>
            /// 标准错误
            /// </summary>
            StandardError = -12,
        }
    }
}