using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.IO;
using Laurent.Lee.CLB.Threading;
using System;
using System.IO;
using System.Threading;

namespace Laurent.Lee.CLB.Diagnostics
{
    /// <summary>
    ///     控制台日志处理
    /// </summary>
    public abstract class TmphConsoleLog : IDisposable
    {
        /// <summary>
        ///     进程复制重启文件监视
        /// </summary>
        private TmphCreateFlieTimeoutWatcher _fileWatcher;

        /// <summary>
        ///     文件监视是否超时
        /// </summary>
        private int _isFileWatcherTimeout;

        /// <summary>
        ///     控制台日志处理
        /// </summary>
        protected TmphConsoleLog()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            TmphDomainUnload.Add(Dispose);
        }

        /// <summary>
        ///     是否自动设置进程复制重启
        /// </summary>
        protected virtual bool IsSetProcessCopy
        {
            get { return true; }
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            TmphPub.Dispose(ref _fileWatcher);
            DisposeResource();
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        protected abstract void DisposeResource();

        /// <summary>
        ///     启动处理
        /// </summary>
        public void Start()
        {
            if (Config.TmphPub.Default.IsDebug) Output("警告：Debug模式");
            if (Config.TmphPub.Default.IsService) TmphThreadPool.TinyPool.Start(StartHandle);
            else
            {
                Output("非服务模式启动");
                StartHandle();
                if (IsSetProcessCopy) SetProcessCopy();
            }
        }

        /// <summary>
        ///     启动处理
        /// </summary>
        protected abstract void StartHandle();

        /// <summary>
        ///     输出信息
        /// </summary>
        /// <param name="message">输出信息</param>
        protected void Output(string message)
        {
            if (!Config.TmphPub.Default.IsService) Console.WriteLine(message);
            TmphLog.Default.Add(message);
        }

        /// <summary>
        ///     输出错误
        /// </summary>
        /// <param name="error">输出错误</param>
        protected void Output(Exception error)
        {
            if (!Config.TmphPub.Default.IsService) Console.WriteLine(error.ToString());
            TmphLog.Error.Add(error, null, false);
        }

        /// <summary>
        ///     启动进程
        /// </summary>
        /// <param name="process">进程信息</param>
        protected void StartProcess(string process)
        {
            string arguments = null;
            var index = process.IndexOf('|');
            if (index != -1)
            {
                arguments = process.Substring(index + 1);
                process = process.Substring(0, index);
            }
            var file = new FileInfo(process);
            if (file.Exists)
            {
                if (TmphProcess.Count(TmphProcess.GetProcessName(file)) == 0)
                {
                    TmphProcess.StartDirectory(process, arguments);
                    Output("启动进程 : " + file.Name);
                }
                else Output("进程已存在 : " + file.Name);
            }
            else Output("文件不存在 : " + process);
        }

        /// <summary>
        ///     输出异常
        /// </summary>
        /// <param name="errorString">异常信息</param>
        private void outputException(string errorString)
        {
            Console.WriteLine();
            Console.WriteLine(errorString);
            try
            {
                File.AppendAllText("CRASH " + TmphDate.NowSecond.ToString("yyyyMMdd HHmmss") + ".txt", errorString + @"
");
            }
            catch (Exception error)
            {
                Console.WriteLine();
                Console.WriteLine(error.ToString());
            }
        }

        /// <summary>
        ///     异常重启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        private void UnhandledException(object sender, UnhandledExceptionEventArgs exception)
        {
            var exceptionObject = exception.ExceptionObject;
            outputException(exceptionObject == null ? exception.ToString() : exceptionObject.ToString());
            try
            {
                TmphProcess.ReStart();
            }
            catch (Exception error)
            {
                outputException(error.ToString());
            }
            try
            {
                Dispose();
            }
            catch (Exception error)
            {
                outputException(error.ToString());
            }
        }

        /// <summary>
        ///     设置进程复制重启
        /// </summary>
        private void SetProcessCopy()
        {
            if (!Config.TmphPub.Default.IsService && TmphProcessCopy.Default.WatcherPath != null)
            {
                try
                {
                    _fileWatcher = new TmphCreateFlieTimeoutWatcher(TmphProcessCopy.Default.CheckTimeoutSeconds,
                        OnFileWatcherTimeout, TmphProcessCopyServer.DefaultFileWatcherFilter);
                    _fileWatcher.Add(TmphProcessCopy.Default.WatcherPath);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, TmphProcessCopy.Default.WatcherPath, false);
                }
                TmphThreadPool.TinyPool.Start(TmphProcessCopyServer.Guard);
            }
        }

        /// <summary>
        ///     删除进程复制重启
        /// </summary>
        protected virtual void RemoveProcessCopy()
        {
            if (IsSetProcessCopy) TmphProcessCopyServer.Remove();
        }

        /// <summary>
        ///     文件监视超时处理
        /// </summary>
        private void OnFileWatcherTimeout()
        {
            if (Interlocked.CompareExchange(ref _isFileWatcherTimeout, 1, 0) == 0) FileWatcherTimeout();
        }

        /// <summary>
        ///     文件监视超时处理
        /// </summary>
        private void FileWatcherTimeout()
        {
            if (TmphProcessCopyServer.CopyStart())
            {
                Dispose();
                Environment.Exit(-1);
            }
            else
            {
                TmphTimerTask.Default.Add(FileWatcherTimeout,
                    TmphDate.NowSecond.AddSeconds(TmphProcessCopy.Default.CheckTimeoutSeconds));
            }
        }

        /// <summary>
        ///     启动控制台服务
        /// </summary>
        /// <typeparam name="TServerType">服务类型</typeparam>
        /// <param name="server">服务实例</param>
        /// <param name="exitCommand">退出命令</param>
        public static void Start<TServerType>(ref TServerType server, string exitCommand)
            where TServerType : TmphConsoleLog
        {
            server.Start();
            try
            {
                var command = Console.ReadLine();
                while (command != exitCommand)
                {
                    if (command == "thread")
                    {
                        Console.WriteLine("Laurent.Lee.CLB.Threading.TmphThreadPool.CheckLog START");
                        TmphThreadPool.CheckLog();
                        Console.WriteLine("Laurent.Lee.CLB.Threading.TmphThreadPool.CheckLog FINALLY");
                    }
                    command = Console.ReadLine();
                }
                server.RemoveProcessCopy();
            }
            finally
            {
                TmphPub.Dispose(ref server);
            }
        }
    }
}