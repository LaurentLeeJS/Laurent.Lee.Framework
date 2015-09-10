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

using System.Threading;

namespace Laurent.Lee.CLB.Threading
{
    /// <summary>
    ///     一次性等待锁
    /// </summary>
    internal struct TmphAutoWaitHandle
    {
        /// <summary>
        ///     同步等待
        /// </summary>
        private readonly EventWaitHandle waitHandle;

        /// <summary>
        ///     同步等待
        /// </summary>
        private int isWait;

        /// <summary>
        ///     一次性等待锁
        /// </summary>
        /// <param name="isSet">是否默认结束等待</param>
        public TmphAutoWaitHandle(bool isSet)
        {
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            isWait = isSet ? 1 : 0;
        }

        /// <summary>
        ///     等待结束
        /// </summary>
        public void Wait()
        {
            //Thread.Sleep(0);
            if (Interlocked.CompareExchange(ref isWait, 1, 0) == 0) waitHandle.WaitOne();
            isWait = 0;
        }

        /// <summary>
        ///     结束等待
        /// </summary>
        public void Set()
        {
            if (Interlocked.CompareExchange(ref isWait, 1, 0) != 0) waitHandle.Set();
        }
    }
}