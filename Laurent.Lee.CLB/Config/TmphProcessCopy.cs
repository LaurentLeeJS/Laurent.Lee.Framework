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
using System.IO;

namespace Laurent.Lee.CLB.Config
{
    /// <summary>
    ///     进程复制重启服务配置
    /// </summary>
    public sealed class TmphProcessCopy
    {
        /// <summary>
        ///     默认进程复制重启服务配置
        /// </summary>
        public static readonly TmphProcessCopy Default = new TmphProcessCopy();

        /// <summary>
        ///     文件更新重启检测时间(单位:秒)
        /// </summary>
        private readonly int checkTimeoutSeconds = 5;

        /// <summary>
        ///     文件更新重启复制超时时间(单位:分)
        /// </summary>
        private readonly int copyTimeoutMinutes = 10;

        /// <summary>
        ///     进程复制重启失败最大休眠秒数
        /// </summary>
        private readonly int maxThreadSeconds = 10;

        /// <summary>
        ///     进程复制重启服务验证
        /// </summary>
        public string Verify;

        /// <summary>
        ///     文件监视路径
        /// </summary>
        public string WatcherPath;

        /// <summary>
        ///     进程复制重启服务配置
        /// </summary>
        private TmphProcessCopy()
        {
            TmphPub.LoadConfig(this);
            if (WatcherPath != null)
            {
                try
                {
                    var fileWatcherDirectory = new DirectoryInfo(WatcherPath);
                    if (fileWatcherDirectory.Exists) WatcherPath = fileWatcherDirectory.fullName().ToLower();
                    else
                    {
                        WatcherPath = null;
                        TmphLog.Error.Add("没有找到文件监视路径 " + WatcherPath, false, false);
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, WatcherPath, false);
                    WatcherPath = null;
                }
            }
        }

        /// <summary>
        ///     文件更新重启检测时间(单位:秒)
        /// </summary>
        public int CheckTimeoutSeconds
        {
            get { return Math.Max(checkTimeoutSeconds, 2); }
        }

        /// <summary>
        ///     文件更新重启复制超时时间(单位:分)
        /// </summary>
        public int CopyTimeoutMinutes
        {
            get { return Math.Max(copyTimeoutMinutes, 1); }
        }

        /// <summary>
        ///     进程复制重启失败最大休眠秒数
        /// </summary>
        public int MaxThreadSeconds
        {
            get { return Math.Max(maxThreadSeconds, 2); }
        }
    }
}