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
    ///     任务信息
    /// </summary>
    internal struct TmphTaskInfo
    {
        /// <summary>
        ///     调用委托
        /// </summary>
        public Action Call;

        /// <summary>
        ///     任务执行出错委托,停止任务参数null
        /// </summary>
        public Action<Exception> OnError;

        /// <summary>
        ///     执行任务
        /// </summary>
        public void Run()
        {
            try
            {
                Call();
            }
            catch (Exception error)
            {
                if (OnError == null) TmphLog.Error.Add(error, null, false);
                else
                {
                    try
                    {
                        OnError(error);
                    }
                    catch (Exception exception)
                    {
                        TmphLog.Error.Add(exception, null, false);
                    }
                }
            }
        }

        /// <summary>
        ///     执行任务
        /// </summary>
        public void RunClear()
        {
            Run();
            Call = null;
            OnError = null;
        }
    }
}