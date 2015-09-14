using System;
using System.Net.Sockets;
using System.Reflection;

#if MONO
#else
namespace Laurent.Lee.CLB.Net
{
    /// <summary>
    /// 异步回调参数
    /// </summary>
    public static class TmphSocketAsyncEventArgsProxy
    {
        /// <summary>
        /// 获取一个异步回调参数
        /// </summary>
        public static Func<SocketAsyncEventArgs> Get = (Func<SocketAsyncEventArgs>)Delegate.CreateDelegate(typeof(Func<SocketAsyncEventArgs>), typeof(TmphSocketAsyncEventArgs).GetMethod("Get", BindingFlags.NonPublic | BindingFlags.Static));
        /// <summary>
        /// 获取一个异步回调参数
        /// </summary>
        public static TmphPushPool<SocketAsyncEventArgs> Push = (TmphPushPool<SocketAsyncEventArgs>)Delegate.CreateDelegate(typeof(TmphPushPool<SocketAsyncEventArgs>), typeof(TmphSocketAsyncEventArgs).GetMethod("Push", BindingFlags.NonPublic | BindingFlags.Static));
    }
}
#endif
