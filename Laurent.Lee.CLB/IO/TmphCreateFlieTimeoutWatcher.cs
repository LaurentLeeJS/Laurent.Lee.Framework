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

using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Laurent.Lee.CLB.IO
{
    /// <summary>
    ///     新建文件监视
    /// </summary>
    public sealed class TmphCreateFlieTimeoutWatcher : IDisposable
    {
        /// <summary>
        ///     超时检测处理
        /// </summary>
        private readonly Action checkTimeoutHandle;

        /// <summary>
        ///     超时检测文件集合访问锁
        /// </summary>
        private readonly object fileLock = new object();

        /// <summary>
        ///     新建文件前置过滤
        /// </summary>
        private readonly Func<FileSystemEventArgs, bool> filter;

        /// <summary>
        ///     新建文件处理
        /// </summary>
        private readonly FileSystemEventHandler onCreatedHandle;

        /// <summary>
        ///     超时处理
        /// </summary>
        private readonly Action onTimeout;

        /// <summary>
        ///     超时检测时钟周期
        /// </summary>
        private readonly long timeoutTicks;

        /// <summary>
        ///     文件监视器集合
        /// </summary>
        private readonly Dictionary<TmphHashString, TmphCounter> watchers;

        /// <summary>
        ///     超时检测访问锁
        /// </summary>
        private int checkLock;

        /// <summary>
        ///     超时检测文件集合
        /// </summary>
        private TmphSubArray<TmphKeyValue<FileInfo, DateTime>> files;

        /// <summary>
        ///     是否释放资源
        /// </summary>
        private int isDisposed;

        /// <summary>
        ///     文件监视器集合访问锁
        /// </summary>
        private int watcherLock;

        /// <summary>
        ///     新建文件监视
        /// </summary>
        /// <param name="seconds">超时检测秒数</param>
        /// <param name="onTimeout">超时处理</param>
        /// <param name="filter">新建文件前置过滤</param>
        public TmphCreateFlieTimeoutWatcher(int seconds, Action onTimeout, Func<FileSystemEventArgs, bool> filter = null)
        {
            if (onTimeout == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            timeoutTicks = new TimeSpan(0, 0, seconds > 0 ? (seconds + 1) : 2).Ticks;
            this.onTimeout = onTimeout;
            this.filter = filter;
            onCreatedHandle = filter == null ? (FileSystemEventHandler)onCreated : onCreatedFilter;
            checkTimeoutHandle = checkTimeout;
            watchers = TmphDictionary.CreateHashString<TmphCounter>();
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            isDisposed = 1;
            var counters = TmphNullValue<TmphCounter>.Array;
            TmphInterlocked.NoCheckCompareSetSleep0(ref watcherLock);
            try
            {
                if (watchers.Count != 0)
                {
                    counters = watchers.Values.GetArray();
                    watchers.Clear();
                }
            }
            finally
            {
                watcherLock = 0;
            }
            foreach (var counter in counters) dispose(counter.Watcher);
        }

        /// <summary>
        ///     添加监视路径
        /// </summary>
        /// <param name="path">监视路径</param>
        public void Add(string path)
        {
            if (isDisposed == 0)
            {
                path = path.ToLower();
                TmphCounter counter;
                TmphHashString pathKey = path;
                TmphInterlocked.NoCheckCompareSetSleep0(ref watcherLock);
                try
                {
                    if (watchers.TryGetValue(pathKey, out counter))
                    {
                        ++counter.Count;
                        watchers[pathKey] = counter;
                    }
                    else
                    {
                        counter.Create(path, onCreatedHandle);
                        watchers.Add(pathKey, counter);
                    }
                }
                finally
                {
                    watcherLock = 0;
                }
            }
        }

        /// <summary>
        ///     删除监视路径
        /// </summary>
        /// <param name="path">监视路径</param>
        public void Remove(string path)
        {
            path = path.ToLower();
            TmphCounter counter;
            TmphHashString pathKey = path;
            TmphInterlocked.NoCheckCompareSetSleep0(ref watcherLock);
            try
            {
                if (watchers.TryGetValue(pathKey, out counter))
                {
                    if (--counter.Count == 0) watchers.Remove(pathKey);
                    else watchers[pathKey] = counter;
                }
            }
            finally
            {
                watcherLock = 0;
            }
            if (counter.Count == 0 && counter.Watcher != null) dispose(counter.Watcher);
        }

        /// <summary>
        ///     新建文件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCreated(object sender, FileSystemEventArgs e)
        {
            onCreated(e);
        }

        /// <summary>
        ///     新建文件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onCreatedFilter(object sender, FileSystemEventArgs e)
        {
            if (filter(e)) onCreated(e);
        }

        /// <summary>
        ///     新建文件处理
        /// </summary>
        /// <param name="e"></param>
        private void onCreated(FileSystemEventArgs e)
        {
            var file = new FileInfo(e.FullPath);
            if (file.Exists)
            {
                var timeout = TmphDate.NowSecond.AddTicks(timeoutTicks);
                Monitor.Enter(fileLock);
                try
                {
                    files.Add(new TmphKeyValue<FileInfo, DateTime>(file, timeout));
                }
                finally
                {
                    Monitor.Exit(fileLock);
                }
                if (Interlocked.CompareExchange(ref checkLock, 1, 0) == 0)
                    TmphTimerTask.Default.Add(checkTimeoutHandle, TmphDate.NowSecond.AddTicks(timeoutTicks), null);
            }
        }

        /// <summary>
        ///     超时检测处理
        /// </summary>
        private void checkTimeout()
        {
            var now = TmphDate.NowSecond;
            var index = 0;
            Monitor.Enter(fileLock);
            var count = files.Count;
            var fileArray = files.array;
            try
            {
                while (index != count)
                {
                    var fileTime = fileArray[index];
                    if (fileTime.Value <= now)
                    {
                        var file = fileTime.Key;
                        var length = file.Length;
                        file.Refresh();
                        if (file.Exists)
                        {
                            if (length == file.Length)
                            {
                                try
                                {
                                    using (
                                        var fileStream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                                    {
                                        fileArray[index--] = fileArray[--count];
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                        else fileArray[index--] = fileArray[--count];
                    }
                    ++index;
                }
                files.UnsafeSetLength(count);
            }
            finally
            {
                Monitor.Exit(fileLock);
                if (count == 0)
                {
                    try
                    {
                        onTimeout();
                    }
                    finally
                    {
                        checkLock = 0;
                    }
                }
                else TmphTimerTask.Default.Add(checkTimeoutHandle, TmphDate.NowSecond.AddTicks(timeoutTicks), null);
            }
        }

        /// <summary>
        ///     关闭文件监视器
        /// </summary>
        /// <param name="watcher">文件监视器</param>
        private void dispose(FileSystemWatcher watcher)
        {
            using (watcher)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Created -= onCreatedHandle;
            }
        }

        /// <summary>
        ///     监视计数
        /// </summary>
        private struct TmphCounter
        {
            /// <summary>
            ///     监视计数
            /// </summary>
            public int Count;

            /// <summary>
            ///     文件监视器
            /// </summary>
            public FileSystemWatcher Watcher;

            /// <summary>
            ///     文件监视器初始化
            /// </summary>
            /// <param name="path">监视路径</param>
            /// <param name="onCreated">新建文件处理</param>
            public void Create(string path, FileSystemEventHandler onCreated)
            {
                Watcher = new FileSystemWatcher(path);
                Watcher.IncludeSubdirectories = false;
                Watcher.EnableRaisingEvents = true;
                Watcher.Created += onCreated;
                Count = 1;
            }
        }
    }
}