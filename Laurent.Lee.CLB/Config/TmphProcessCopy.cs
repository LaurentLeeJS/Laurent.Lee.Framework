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