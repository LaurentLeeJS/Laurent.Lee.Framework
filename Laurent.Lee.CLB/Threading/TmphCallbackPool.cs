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

using System;

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    ///     回调池
    /// </summary>
    /// <typeparam name="TCallbackType">回调对象类型</typeparam>
    /// <typeparam name="TValueType">回调值类型</typeparam>
    public abstract class TmphCallbackActionPool<TCallbackType, TValueType>
        where TCallbackType : class
    {
        /// <summary>
        ///     回调委托
        /// </summary>
        public Action<TValueType> Callback;

        /// <summary>
        ///     添加回调对象
        /// </summary>
        /// <param name="poolCallback">回调对象</param>
        /// <param name="value">回调值</param>
        protected void push(TCallbackType poolCallback, TValueType value)
        {
            var callback = Callback;
            Callback = null;
            try
            {
                TmphTypePool<TCallbackType>.Push(poolCallback);
            }
            finally
            {
                if (callback != null)
                {
                    try
                    {
                        callback(value);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                }
            }
        }

        /// <summary>
        ///     回调处理
        /// </summary>
        /// <param name="value">回调值</param>
        protected void onlyCallback(TValueType value)
        {
            var callback = Callback;
            if (callback != null)
            {
                try
                {
                    callback(value);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }
        }
    }
}