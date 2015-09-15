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

using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.IO;
using Laurent.Lee.CLB.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Laurent.Lee.CLB
{
    public sealed class TmphLog : IDisposable
    {
        public enum TmphCacheType
        {
            None,
            Queue,
            Last
        }

        public enum TmphExceptionType
        {
            None,
            Null,
            IndexOutOfRange,
            ErrorOperation
        }

        public const string ExceptionPrefix = TmphPub.LaurentLeeFramework + " Exception:";
        public const string DefaultFilePrefix = "TmphLog_";
        private static readonly TmphCharStream toStringStream = new TmphCharStream();
        private static int toStringStreamLock;
        public static readonly TmphLog Default;
        public static readonly TmphLog Error;
        private readonly TmphFifoPriorityQueue<TmphHashString, bool> tmphFifoPriorityQueue = new TmphFifoPriorityQueue<TmphHashString, bool>();
        private readonly string fileName;
        private readonly int maxCacheCount;
        private int cacheLock;
        private int fileLock;
        private TmphFileStreamWriter fileStream;
        private int isDisposed;
        private TmphHashString lastCache;
        public int MaxSize = TmphAppSetting.MaxLogSize;

        static TmphLog()
        {
            Default = new TmphLog(TmphAppSetting.IsLogConsole ? null : TmphAppSetting.LogPath + DefaultFilePrefix + "default.txt", TmphAppSetting.MaxLogCacheCount);
            Error = TmphAppSetting.IsErrorLog ? new TmphLog(TmphAppSetting.IsLogConsole ? null : TmphAppSetting.LogPath + DefaultFilePrefix + "error.txt", TmphAppSetting.MaxLogCacheCount) : Default;
            if (TmphAppSetting.IsPoolDebug)
                Default.Add("对象池采用纠错模式", false, TmphCacheType.None);
            if (TmphAppSetting.IsCheckMemory)
                TmphCheckMemory.Add(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public TmphLog(string fileName, int maxCacheCount)
        {
            if ((this.fileName = fileName) != null)
            {
                this.maxCacheCount = maxCacheCount <= 0 ? 1 : maxCacheCount;
                open();
                if (fileStream != null) TmphDomainUnload.AddLast(Dispose);
            }
        }

        public string FileName
        {
            get { return fileStream != null ? fileStream.FileName : null; }
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1)
            {
                TmphDomainUnload.RemoveLast(Dispose, false);
                if (fileStream != null)
                {
                    try
                    {
                        string fileName = fileStream.FileName;
                        TmphPub.Dispose(ref fileStream);
                        TmphFile.MoveBak(fileName);
                    }
                    catch
                    {
                        TmphPub.Dispose(ref fileStream);
                    }
                }
            }
        }

        /// <summary>
        ///     打开日志文件
        /// </summary>
        private void open()
        {
            try
            {
                fileStream = new TmphFileStreamWriter(fileName, FileMode.OpenOrCreate, FileShare.Read, FileOptions.None, false, TmphAppSetting.Encoding);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.ToString());
            }
            if (fileStream == null)
            {
                try
                {
                    if (File.Exists(fileName))
                        fileStream = new TmphFileStreamWriter(TmphFile.MoveBakFileName(fileName), FileMode.OpenOrCreate, FileShare.Read, FileOptions.None, false, TmphAppSetting.Encoding);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.ToString());
                }
            }
        }

        /// <summary>
        ///     日志信息写文件
        /// </summary>
        /// <param name="value">日志信息</param>
        private void output(TmphDebug value)
        {
            if (isDisposed == 0)
            {
                if (fileStream == null)
                    Console.WriteLine(@" " + TmphDate.NowSecond.toString() + " : " + value);
                else
                {
                    var data = fileStream.GetBytes(@" " + TmphDate.NowSecond.toString() + " : " + value + @" ");
                    if (Interlocked.CompareExchange(ref fileLock, 1, 0) != 0)
                    {
                        Thread.Sleep(0);
                        while (Interlocked.CompareExchange(ref fileLock, 1, 0) != 0)
                            Thread.Sleep(1);
                    }
                    try
                    {
                        if (fileStream.UnsafeWrite(data) >= MaxSize && MaxSize > 0)
                            MoveBakPri();
                        else
                            fileStream.WaitWriteBuffer();
                    }
                    finally
                    {
                        fileLock = 0;
                    }
                }
            }
        }

        private void realOutput(TmphDebug value)
        {
            if (isDisposed == 0)
            {
                if (fileStream == null)
                    Console.WriteLine(@" " + TmphDate.NowSecond.toString() + " : " + value);
                else
                {
                    var data = fileStream.GetBytes(@" " + TmphDate.NowSecond.toString() + " : " + value + @" ");
                    if (Interlocked.CompareExchange(ref fileLock, 1, 0) != 0)
                    {
                        Thread.Sleep(0);
                        while (Interlocked.CompareExchange(ref fileLock, 1, 0) != 0)
                            Thread.Sleep(1);
                    }
                    try
                    {
                        if (fileStream.UnsafeWrite(data) >= MaxSize && MaxSize > 0)
                            MoveBakPri();
                        else
                            fileStream.Flush(true);
                    }
                    finally
                    {
                        fileLock = 0;
                    }
                }
            }
        }

        private bool CheckCache(TmphDebug value, bool isQueue)
        {
            TmphHashString key = value.ToString();
            if (isQueue)
            {
                TmphInterlocked.NoCheckCompareSetSleep0(ref cacheLock);
                try
                {
                    if (tmphFifoPriorityQueue.Get(key, false))
                        return false;
                    tmphFifoPriorityQueue.Set(key, true);
                    if (tmphFifoPriorityQueue.Count > maxCacheCount)
                        tmphFifoPriorityQueue.Pop();
                }
                finally
                {
                    cacheLock = 0;
                }
                return true;
            }
            if (key.Equals(lastCache))
                return false;
            lastCache = key;
            return true;
        }

        public string MoveBak()
        {
            if (fileStream != null)
            {
                if (Interlocked.CompareExchange(ref fileLock, 1, 0) != 0)
                {
                    Thread.Sleep(0);
                    while (Interlocked.CompareExchange(ref fileLock, 1, 0) != 0)
                        Thread.Sleep(1);
                }
                try
                {
                    return MoveBakPri();
                }
                finally
                {
                    fileLock = 0;
                }
            }
            return null;
        }

        private string MoveBakPri()
        {
            string fileName = fileStream.FileName, bakFileName = null;
            TmphPub.Dispose(ref fileStream);
            bakFileName = TmphFile.MoveBak(fileName);
            open();
            return bakFileName;
        }

        public void Add(Exception exception, string message = null, bool isCache = true)
        {
            Add(exception, message, isCache ? TmphCacheType.Queue : TmphCacheType.None);
        }

        public void Add(Exception error, string message, TmphCacheType cacheType)
        {
            if (error != null && error.Message.StartsWith(ExceptionPrefix, StringComparison.Ordinal))
                error = null;
            if (error == null)
            {
                if (message != null)
                    Add(message, true, cacheType);
            }
            else
            {
                var value = new TmphDebug
                {
                    Exception = error,
                    Message = message
                };
                if (cacheType == TmphCacheType.None || CheckCache(value, cacheType == TmphCacheType.Queue)) output(value);
            }
        }

        public void Add(string message, bool isStackTrace = false, bool isCache = false)
        {
            Add(message, isStackTrace, isCache ? TmphCacheType.Queue : TmphCacheType.None);
        }

        public void Add(string message, bool isStackTrace, TmphCacheType cache)
        {
            var value = new TmphDebug
            {
                StackTrace = isStackTrace ? new StackTrace() : null,
                StackFrame = isStackTrace ? null : new StackFrame(1),
                Message = message
            };
            if (cache == TmphCacheType.None || CheckCache(value, cache == TmphCacheType.Queue))
                output(value);
        }

        public void Real(Exception exception, string message = null, bool isCache = true)
        {
            Real(exception, message, isCache ? TmphCacheType.Queue : TmphCacheType.None);
        }

        public void Real(Exception exception, string message, TmphCacheType cache)
        {
            if (exception != null && exception.Message.StartsWith(ExceptionPrefix, StringComparison.Ordinal))
                exception = null;
            if (exception == null)
            {
                if (message != null)
                    Real(message, true, cache);
            }
            else
            {
                var value = new TmphDebug
                {
                    Exception = exception,
                    Message = message
                };
                if (cache == TmphCacheType.None || CheckCache(value, cache == TmphCacheType.Queue)) realOutput(value);
            }
        }

        public void Real(string message, bool isStackTrace = false, bool isCache = false)
        {
            Real(message, isStackTrace, isCache ? TmphCacheType.Queue : TmphCacheType.None);
        }

        public void Real(string message, bool isStackTrace, TmphCacheType cache)
        {
            var value = new TmphDebug
            {
                StackTrace = isStackTrace ? new StackTrace() : null,
                StackFrame = isStackTrace ? null : new StackFrame(1),
                Message = message
            };
            if (cache == TmphCacheType.None || CheckCache(value, cache == TmphCacheType.Queue)) realOutput(value);
        }

        public void Throw(TmphExceptionType exceptionType)
        {
            var value = new TmphDebug
            {
                StackTrace = new StackTrace(),
                exceptionType = exceptionType
            };
            if (CheckCache(value, true)) output(value);
            throw new Exception(ExceptionPrefix + value);
        }

        public void Throw(Exception exception, string message = null, bool isCache = true)
        {
            Throw(exception, message, isCache ? TmphCacheType.Queue : TmphCacheType.None);
        }

        public void Throw(Exception exception, string message, TmphCacheType cache)
        {
            if (exception != null && exception.Message.StartsWith(ExceptionPrefix, StringComparison.Ordinal))
                exception = null;
            if (exception == null)
            {
                if (message != null)
                    Throw(message, true, cache);
            }
            else
            {
                var value = new TmphDebug
                {
                    Exception = exception,
                    Message = message
                };
                if (cache == TmphCacheType.None || CheckCache(value, cache == TmphCacheType.Queue))
                    output(value);
                throw exception != null ? new Exception(ExceptionPrefix + message, exception) : new Exception(ExceptionPrefix + message);
            }
        }

        public void Throw(string message, bool isStackTrace = false, bool isCache = false)
        {
            Throw(message, isStackTrace, isCache ? TmphCacheType.Queue : TmphCacheType.None);
        }

        public void Throw(string message, bool isStackTrace, TmphCacheType cache)
        {
            var value = new TmphDebug
            {
                StackTrace = isStackTrace ? new StackTrace() : null,
                StackFrame = isStackTrace ? null : new StackFrame(1),
                Message = message
            };
            if (cache == TmphCacheType.None || CheckCache(value, cache == TmphCacheType.Queue))
                output(value);
            throw new Exception(ExceptionPrefix + message);
        }

        public void ThrowReal(TmphExceptionType exceptionType)
        {
            var value = new TmphDebug
            {
                StackTrace = new StackTrace(),
                exceptionType = exceptionType
            };
            if (CheckCache(value, true)) realOutput(value);
            throw new Exception(ExceptionPrefix + value);
        }

        public void ThrowReal(Exception exception, string message = null, bool isCache = true)
        {
            ThrowReal(exception, message, isCache ? TmphCacheType.Queue : TmphCacheType.None);
        }

        public void ThrowReal(Exception exception, string message, TmphCacheType cache)
        {
            if (exception != null && exception.Message.StartsWith(ExceptionPrefix, StringComparison.Ordinal)) exception = null;
            if (exception == null)
            {
                if (message != null) ThrowReal(message, true, cache);
            }
            else
            {
                var value = new TmphDebug
                {
                    Exception = exception,
                    Message = message
                };
                if (cache == TmphCacheType.None || CheckCache(value, cache == TmphCacheType.Queue)) realOutput(value);
                throw exception != null
                    ? new Exception(ExceptionPrefix + message, exception)
                    : new Exception(ExceptionPrefix + message);
            }
        }

        public void ThrowReal(string message, bool isStackTrace = false, bool isCache = false)
        {
            ThrowReal(message, isStackTrace, isCache ? TmphCacheType.Queue : TmphCacheType.None);
        }

        public void ThrowReal(string message, bool isStackTrace, TmphCacheType cache)
        {
            var value = new TmphDebug
            {
                StackTrace = isStackTrace ? new StackTrace() : null,
                StackFrame = isStackTrace ? null : new StackFrame(1),
                Message = message
            };
            if (cache == TmphCacheType.None || CheckCache(value, cache == TmphCacheType.Queue)) realOutput(value);
            throw new Exception(ExceptionPrefix + message);
        }

        /// <summary>
        ///     日志信息
        /// </summary>
        private sealed class TmphDebug
        {
            public Exception Exception;
            public string Message;
            public StackFrame StackFrame;
            public StackTrace StackTrace;
            public string toString;
            public TmphExceptionType exceptionType;

            public override string ToString()
            {
                if (toString == null)
                {
                    string stackFrameMethodTypeName = null,
                        stackFrameMethodString = null,
                        stackFrameFile = null,
                        stackFrameLine = null,
                        stackFrameColumn = null;
                    if (StackFrame != null)
                    {
                        var stackFrameMethod = StackFrame.GetMethod();
                        stackFrameMethodTypeName = stackFrameMethod.ReflectedType.FullName;
                        stackFrameMethodString = stackFrameMethod.ToString();
                        stackFrameFile = StackFrame.GetFileName();
                        if (stackFrameFile != null)
                        {
                            stackFrameLine = StackFrame.GetFileLineNumber().ToString();
                            stackFrameColumn = StackFrame.GetFileColumnNumber().ToString();
                        }
                    }
                    var stackTrace = StackTrace == null ? null : StackTrace.ToString();
                    var exception = Exception == null ? null : Exception.ToString();
                    TmphInterlocked.NoCheckCompareSetSleep0(ref toStringStreamLock);
                    try
                    {
                        if (Message != null)
                        {
                            toStringStream.WriteNotNull(@"附加信息:");
                            toStringStream.WriteNotNull(Message);
                        }
                        if (StackFrame != null)
                        {
                            toStringStream.Write(@"堆栈帧信息:");
                            toStringStream.WriteNotNull(stackFrameMethodTypeName);
                            toStringStream.WriteNotNull(" + ");
                            toStringStream.WriteNotNull(stackFrameMethodString);
                            if (stackFrameFile != null)
                            {
                                toStringStream.WriteNotNull(" in ");
                                toStringStream.WriteNotNull(stackFrameFile);
                                toStringStream.WriteNotNull(" line ");
                                toStringStream.WriteNotNull(stackFrameLine);
                                toStringStream.WriteNotNull(" col ");
                                toStringStream.Write(stackFrameColumn);
                            }
                        }
                        if (stackTrace != null)
                        {
                            toStringStream.WriteNotNull(@"堆栈信息 : ");
                            toStringStream.WriteNotNull(stackTrace);
                        }
                        if (exception != null)
                        {
                            toStringStream.WriteNotNull(@"异常信息 : ");
                            toStringStream.WriteNotNull(exception);
                        }
                        if (exceptionType != TmphExceptionType.None)
                        {
                            toStringStream.WriteNotNull("异常类型 : ");
                            toStringStream.WriteNotNull(exceptionType.ToString());
                        }
                        toString = toStringStream.ToString();
                    }
                    finally
                    {
                        toStringStream.Clear();
                        toStringStreamLock = 0;
                    }
                }
                return toString;
            }
        }
    }
}