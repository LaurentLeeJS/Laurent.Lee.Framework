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

using System.Net.Sockets;

namespace Laurent.Lee.CLB.Net
{
    /// <summary>
    ///     异步回调参数
    /// </summary>
    public static class TmphSocketAsyncEventArgs
    {
        /// <summary>
        ///     获取一个异步回调参数
        /// </summary>
        /// <returns>异步回调参数</returns>
        internal static SocketAsyncEventArgs Get()
        {
            var value = TmphTypePool<SocketAsyncEventArgs>.Pop();
            if (value == null)
            {
                value = new SocketAsyncEventArgs();
                value.SocketFlags = SocketFlags.None;
                value.DisconnectReuseSocket = false;
            }
            return value;
        }

        /// <summary>
        ///     添加异步回调参数
        /// </summary>
        /// <param name="value">异步回调参数</param>
        internal static void Push(ref SocketAsyncEventArgs value)
        {
            value.SetBuffer(null, 0, 0);
            value.UserToken = null;
            value.SocketError = SocketError.Success;
            TmphTypePool<SocketAsyncEventArgs>.Push(value);
        }
    }
}