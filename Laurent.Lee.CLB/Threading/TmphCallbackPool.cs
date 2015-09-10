/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
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