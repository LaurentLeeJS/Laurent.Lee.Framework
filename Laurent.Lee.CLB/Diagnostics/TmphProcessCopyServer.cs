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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Emit;
using Laurent.Lee.CLB.Net.Tcp;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Laurent.Lee.CLB.Diagnostics
{
    /// <summary>
    ///     进程复制重启服务
    /// </summary>
    [Code.CSharp.TmphTcpServer(Service = "TmphProcessCopyServer", IsIdentityCommand = true,
        VerifyMethodType = typeof(TmphVerifyMethod))]
    public partial class TmphProcessCopyServer
    {
        /// <summary>
        ///     进程守护缓存文件名
        /// </summary>
        private const string CacheFile = "ProcessCopyServer_Guard.cache";

        /// <summary>
        ///     守护进程集合
        /// </summary>
        private static Dictionary<int, TmphCopyer> _guards;

        /// <summary>
        ///     守护进程集合访问锁
        /// </summary>
        private static readonly object GuardLock = new object();

        /// <summary>
        ///     守护进程客户端调用
        /// </summary>
        private static Action _guardHandle;

        /// <summary>
        ///     默认文件监视器过滤
        /// </summary>
        public static readonly Func<FileSystemEventArgs, bool> DefaultFileWatcherFilter = FileWatcherFilter;

        /// <summary>
        ///     设置TCP服务端
        /// </summary>
        /// <param name="tcpServer">TCP服务端</param>
        public void SetTcpServer(TmphServer tcpServer)
        {
            var isSave = false;
            Monitor.Enter(GuardLock);
            if (_guards == null)
            {
                try
                {
                    _guards = TmphDictionary.CreateInt<TmphCopyer>();
                    if (File.Exists(CacheFile))
                    {
                        foreach (var copyer in TmphDataDeSerializer.DeSerialize<TmphCopyer[]>(File.ReadAllBytes(CacheFile)))
                        {
                            _guards.Add(copyer.ProcessId, copyer);
                            if (!copyer.Guard(this))
                            {
                                _guards.Remove(copyer.ProcessId);
                                isSave = true;
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                    try
                    {
                        File.Delete(CacheFile);
                    }
                    catch (Exception error1)
                    {
                        TmphLog.Default.Add(error1, null, false);
                    }
                }
            }
            Monitor.Exit(GuardLock);
            if (isSave) SaveCache();
        }

        /// <summary>
        ///     进程复制重启服务端注册验证函数
        /// </summary>
        /// <returns>是否验证成功</returns>
        [Code.CSharp.TmphTcpServer(IsVerifyMethod = true, IsServerAsynchronousTask = false, InputParameterMaxLength = 1024
            )]
        protected virtual bool Verify(string value)
        {
            if (TmphProcessCopy.Default.Verify == null && !Config.TmphPub.Default.IsDebug)
            {
                TmphLog.Error.Add("进程复制重启服务验证数据不能为空", false, true);
                return false;
            }
            return TmphProcessCopy.Default.Verify == value;
        }

        /// <summary>
        ///     复制重启进程
        /// </summary>
        /// <param name="copyer">文件复制器</param>
        [Code.CSharp.TmphTcpServer]
        private void copyStart(TmphCopyer copyer)
        {
            if (copyer.CheckName())
            {
                bool isGuard;
                Monitor.Enter(GuardLock);
                try
                {
                    isGuard = _guards.Remove(copyer.ProcessId);
                }
                finally
                {
                    Monitor.Exit(GuardLock);
                }
                if (isGuard) SaveCache();
                TmphLog.Default.Add("启动文件复制 " + copyer.Process);
                copyer.Copy();
            }
        }

        /// <summary>
        ///     守护进程
        /// </summary>
        /// <param name="copyer">文件信息</param>
        [Code.CSharp.TmphTcpServer]
        private void Guard(TmphCopyer copyer)
        {
            if (copyer.CheckName())
            {
                TmphCopyer cache;
                Monitor.Enter(GuardLock);
                try
                {
                    if (_guards.TryGetValue(copyer.ProcessId, out cache)) _guards[copyer.ProcessId] = copyer;
                    else _guards.Add(copyer.ProcessId, copyer);
                }
                finally
                {
                    Monitor.Exit(GuardLock);
                }
                if (cache != null) TmphPub.Dispose(ref cache);
                if (!copyer.Guard(this))
                {
                    Monitor.Enter(GuardLock);
                    try
                    {
                        if (_guards.TryGetValue(copyer.ProcessId, out cache) && cache == copyer)
                            _guards.Remove(copyer.ProcessId);
                    }
                    finally
                    {
                        Monitor.Exit(GuardLock);
                    }
                }
                SaveCache();
            }
        }

        /// <summary>
        ///     删除守护进程
        /// </summary>
        /// <param name="copyer">文件信息</param>
        [Code.CSharp.TmphTcpServer]
        private void Remove(TmphCopyer copyer)
        {
            if (copyer.CheckName()) removeNoCheck(copyer);
        }

        /// <summary>
        ///     删除守护进程
        /// </summary>
        /// <param name="copyer">文件信息</param>
        private void removeNoCheck(TmphCopyer copyer)
        {
            TmphCopyer cache;
            Monitor.Enter(GuardLock);
            try
            {
                if (_guards.TryGetValue(copyer.ProcessId, out cache)) _guards.Remove(copyer.ProcessId);
            }
            finally
            {
                Monitor.Exit(GuardLock);
            }
            if (cache != null)
            {
                cache.RemoveGuard();
                SaveCache();
            }
        }

        /// <summary>
        ///     守护进程客户端调用
        /// </summary>
        public static void Guard()
        {
            try
            {
                using (var processCopy = new TcpClient.TmphProcessCopy())
                {
                    if (processCopy.guard(TmphCopyer.Watcher).IsReturn) return;
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            TmphLog.Error.Add("守护进程客户端调用失败");
            if (_guardHandle == null) _guardHandle = GuardReCall;
            TmphTimerTask.Default.Add(_guardHandle, TmphDate.NowSecond.AddMinutes(1));
        }

        /// <summary>
        ///     守护进程删除客户端调用
        /// </summary>
        public static void Remove()
        {
            try
            {
                using (var processCopy = new TcpClient.TmphProcessCopy()) processCopy.remove(TmphCopyer.Watcher);
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
        }

        /// <summary>
        ///     守护进程客户端调用
        /// </summary>
        private static void GuardReCall()
        {
            try
            {
                using (var processCopy = new TcpClient.TmphProcessCopy())
                {
                    if (processCopy.guard(TmphCopyer.Watcher).IsReturn)
                    {
                        TmphLog.Default.Add("守护进程客户端调用成功");
                        return;
                    }
                }
            }
            catch
            {
                // ignored
            }
            TmphTimerTask.Default.Add(_guardHandle, TmphDate.NowSecond.AddMinutes(1));
        }

        /// <summary>
        ///     文件监视器过滤
        /// </summary>
        /// <param name="e"></param>
        /// <returns>是否继续检测</returns>
        private static unsafe bool FileWatcherFilter(FileSystemEventArgs e)
        {
            var name = e.FullPath;
            if (name.Length > 4)
            {
                fixed (char* nameFixed = name)
                {
                    var end = nameFixed + name.Length;
                    var code = *(end - 4) | (*(end - 3) << 8) | (*(end - 2) << 16) | (*(end - 1) << 24) | 0x202020;
                    if (code == ('.' | ('d' << 8) | ('l' << 16) | ('l' << 24))
                        || code == ('.' | ('p' << 8) | ('d' << 16) | ('b' << 24))
                        || code == ('.' | ('e' << 8) | ('x' << 16) | ('e' << 24)))
                    {
                        return true;
                    }
                    if ((code | 0x20000000) == ('n' | ('f' << 8) | ('i' << 16) | ('g' << 24)) && name.Length > 7)
                    {
                        end -= 7;
                        if ((*end | (*(end + 1) << 8) | (*(end + 2) << 16) | 0x2020) == ('.' | ('c' << 8) | ('o' << 16)))
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     进程复制重启客户端调用
        /// </summary>
        /// <returns>是否成功</returns>
        public static bool CopyStart()
        {
            try
            {
                using (var processCopy = new TcpClient.TmphProcessCopy())
                {
                    processCopy.copyStart(TmphCopyer.Watcher);
                }
                return true;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            return false;
        }

        /// <summary>
        ///     保存进程守护信息集合到缓存文件
        /// </summary>
        private static void SaveCache()
        {
            TmphCopyer[] cache;
            Monitor.Enter(GuardLock);
            try
            {
                cache = _guards.Values.GetArray();
            }
            finally
            {
                Monitor.Exit(GuardLock);
            }
            try
            {
                if (cache.Length == 0) File.Delete(CacheFile);
                else File.WriteAllBytes(CacheFile, TmphDataSerializer.Serialize(cache));
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
        }

        /// <summary>
        ///     文件复制器
        /// </summary>
        [TmphDataSerialize(IsMemberMap = false)]
        public sealed class TmphCopyer : IDisposable
        {
            /// <summary>
            ///     默认文件复制器
            /// </summary>
            private static TmphCopyer _watcher;

            /// <summary>
            ///     进程信息
            /// </summary>
            [TmphIgnore]
            private Process _process;

            /// <summary>
            ///     进程复制重启服务
            /// </summary>
            [TmphIgnore]
            private TmphProcessCopyServer _server;

            /// <summary>
            ///     超时时间
            /// </summary>
            [TmphIgnore]
            private DateTime _timeout;

            /// <summary>
            ///     进程启动参数
            /// </summary>
            public string Arguments;

            /// <summary>
            ///     复制文件源路径
            /// </summary>
            public string CopyPath;

            /// <summary>
            ///     目标路径
            /// </summary>
            public string Path;

            /// <summary>
            ///     进程文件名
            /// </summary>
            public string Process;

            /// <summary>
            ///     进程标识
            /// </summary>
            public int ProcessId;

            /// <summary>
            ///     进程名称
            /// </summary>
            public string ProcessName;

            /// <summary>
            ///     默认文件复制器
            /// </summary>
            internal static TmphCopyer Watcher
            {
                get
                {
                    if (_watcher == null)
                    {
                        var command = Environment.CommandLine;
                        var index = command.IndexOf(' ') + 1;
                        using (var process = System.Diagnostics.Process.GetCurrentProcess())
                        {
                            var file = new FileInfo(process.MainModule.FileName);
                            if (file.Directory != null)
                                _watcher = new TmphCopyer
                                {
                                    ProcessId = process.Id,
                                    ProcessName = process.ProcessName,
                                    Process = file.FullName,
                                    Path = file.Directory.FullName,
                                    CopyPath = TmphProcessCopy.Default.WatcherPath,
                                    Arguments = index == 0 || index == command.Length ? null : command.Substring(index)
                                };
                        }
                    }
                    return _watcher;
                }
            }

            /// <summary>
            /// </summary>
            public void Dispose()
            {
                if (_server != null)
                {
                    _server.removeNoCheck(this);
                    _server = null;
                }
            }

            /// <summary>
            ///     删除进程退出事件
            /// </summary>
            internal void RemoveGuard()
            {
                if (_process != null)
                {
                    try
                    {
                        _process.EnableRaisingEvents = false;
                        _process.Exited -= Guard;
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        TmphPub.Dispose(ref _process);
                    }
                }
            }

            /// <summary>
            ///     开始复制文件
            /// </summary>
            internal void Copy()
            {
                var isLog = true;
                _timeout = TmphDate.NowSecond.AddMinutes(TmphProcessCopy.Default.CopyTimeoutMinutes);
                for (var milliseconds = 1 << 4; milliseconds <= 1 << 11; milliseconds <<= 1)
                {
                    Thread.Sleep(milliseconds);
                    if (Copy(isLog))
                    {
                        CopyStart();
                        return;
                    }
                    isLog = false;
                }
                var tiks = new TimeSpan(0, 0, 4).Ticks;
                TmphTimerTask.Default.Add(Copy, 4, TmphDate.NowSecond.AddSeconds(4), null);
            }

            /// <summary>
            ///     开始复制文件
            /// </summary>
            /// <param name="seconds">休眠秒数</param>
            private void Copy(int seconds)
            {
                if (TmphDate.NowSecond >= _timeout)
                {
                    TmphLog.Error.Add("文件复制超时 " + Process);
                    return;
                }
                if (Copy(true))
                {
                    CopyStart();
                    return;
                }
                if (seconds != TmphProcessCopy.Default.CheckTimeoutSeconds)
                {
                    seconds <<= 1;
                    if (seconds <= 0 || seconds > TmphProcessCopy.Default.CheckTimeoutSeconds)
                        seconds = TmphProcessCopy.Default.CheckTimeoutSeconds;
                }
                TmphTimerTask.Default.Add(Copy, seconds, TmphDate.NowSecond.AddSeconds(seconds), null);
            }

            /// <summary>
            ///     复制文件
            /// </summary>
            /// <param name="isLog">是否输出错误日志</param>
            /// <returns>是否成功</returns>
            private bool Copy(bool isLog)
            {
                try
                {
                    var directory = new DirectoryInfo(Path);
                    if (!directory.Exists) directory.Create();
                    var copyDirectory = new DirectoryInfo(CopyPath);
                    Path = directory.fullName();
                    if (copyDirectory.Exists)
                    {
                        foreach (var file in copyDirectory.GetFiles()) file.CopyTo(Path + file.Name, true);
                        return true;
                    }
                    if (File.Exists(Process)) return true;
                }
                catch (Exception error)
                {
                    if (isLog) TmphLog.Error.Add(error, null, false);
                }
                return false;
            }

            /// <summary>
            ///     启动进程
            /// </summary>
            public void CopyStart()
            {
                TmphLog.Default.Add("文件复制成功 " + Process);
                Start();
            }

            /// <summary>
            ///     启动进程
            /// </summary>
            private void Start()
            {
                var info = new ProcessStartInfo(Process, Arguments);
                info.UseShellExecute = true;
                info.WorkingDirectory = Path;
                System.Diagnostics.Process.Start(info);
                TmphLog.Default.Add("进程启动成功 " + Process);
            }

            /// <summary>
            /// </summary>
            /// <returns></returns>
            internal bool CheckName()
            {
                using (var process = System.Diagnostics.Process.GetProcessById(ProcessId))
                {
                    return process.ProcessName == ProcessName;
                    //if (process != null)
                    //{
                    //    try
                    //    {
                    //        FileInfo file = new FileInfo(process.MainModule.FileName);
                    //        return Process == file.FullName;
                    //    }
                    //    catch(Exception error)
                    //    {
                    //        log.Default.Add(error, null, false);
                    //        return true;
                    //    }
                    //}
                }
                //return false;
            }

            /// <summary>
            ///     进程退出事件
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Guard(object sender, EventArgs e)
            {
                try
                {
                    Dispose();
                    if (File.Exists(Process)) Start();
                    else TmphLog.Error.Add("没有找到文件 " + Process);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, "进程启动失败 " + Process, false);
                }
            }

            /// <summary>
            ///     守护进程退出事件
            /// </summary>
            /// <param name="server"></param>
            /// <returns></returns>
            internal bool Guard(TmphProcessCopyServer server)
            {
                _server = server;
                try
                {
                    if ((_process = System.Diagnostics.Process.GetProcessById(ProcessId)) != null)
                    {
                        _process.EnableRaisingEvents = true;
                        _process.Exited += Guard;
                        TmphLog.Default.Add("添加守护进程 " + Process);
                        return true;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Default.Add(error, null, false);
                }
                return false;
            }
        }
    }
}