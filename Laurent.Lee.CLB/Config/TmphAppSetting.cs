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
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text;

#if MONO
#else

#endif

namespace Laurent.Lee.CLB.Config
{
    /// <summary>
    ///     .NET配置文件
    /// </summary>
    public static class TmphAppSetting
    {
        /// <summary>
        ///     默认配置文件
        /// </summary>
        private const string DefaultConfigFile = CLB.TmphPub.LaurentLeeFramework + ".TmphConfig";

        /// <summary>
        ///     Laurent.Lee.CLB配置文件嵌套名称
        /// </summary>
        private const string DefaultConfigIncludeName = "@";

        /// <summary>
        ///     .NET配置
        /// </summary>
        private static readonly NameValueCollection Settings;

        /// <summary>
        ///     Laurent.Lee.CLB配置文件
        /// </summary>
        public static readonly string ConfigFile;

        /// <summary>
        ///     Laurent.Lee.CLB配置文件嵌套名称
        /// </summary>
        public static readonly string ConfigIncludeName;

        /// <summary>
        ///     配置文件路径
        /// </summary>
        public static readonly string ConfigPath;

        /// <summary>
        ///     日志文件主目录
        /// </summary>
        public static readonly string LogPath;

        /// <summary>
        ///     日志文件默认最大字节数
        /// </summary>
        public static readonly int MaxLogSize;

        /// <summary>
        ///     最大缓存日志数量
        /// </summary>
        public static readonly int MaxLogCacheCount;

        /// <summary>
        ///     默认日志是否重定向到控制台
        /// </summary>
        public static readonly bool IsLogConsole;

        /// <summary>
        ///     是否创建单独的错误日志文件
        /// </summary>
        public static readonly bool IsErrorLog;

        /// <summary>
        ///     JSON解析/循环检测最大深度
        /// </summary>
        public static readonly int JsonDepth;

        /// <summary>
        ///     Json转换时间差
        /// </summary>
        public static readonly DateTime JavascriptMinTime;

        /// <summary>
        ///     全局默认编码
        /// </summary>
        public static readonly Encoding Encoding;

        /// <summary>
        ///     默认微型线程池线程堆栈大小
        /// </summary>
        public static readonly int TinyThreadStackSize;

        /// <summary>
        ///     默认线程池线程堆栈大小
        /// </summary>
        public static readonly int ThreadStackSize;

        /// <summary>
        ///     对象池初始大小
        /// </summary>
        public static readonly int PoolSize;

        /// <summary>
        ///     对象池是否采用纠错模式
        /// </summary>
        public static readonly bool IsPoolDebug;

        /// <summary>
        ///     流缓冲区字节尺寸
        /// </summary>
        public static readonly int StreamBufferSize;

        /// <summary>
        ///     是否默认添加内存检测类型
        /// </summary>
        public static readonly bool IsCheckMemory;

        static TmphAppSetting()
        {
#if MONO
            settings = new NameValueCollection();
            ConfigPath = LogPath = Laurent.Lee.CLB.pub.ApplicationPath;
            ConfigIncludeName = defaultConfigIncludeName;
            MaxLogSize = 1 << 20;
            MaxLogCacheCount = 1 << 10;
            JsonDepth = 64;
            JavascriptMinTime = new DateTime(1970, 1, 1, 8, 0, 0);
            Encoding = Encoding.UTF8;
            TinyThreadStackSize = 128 << 10;
            ThreadStackSize = 1 << 20;
            PoolSize = 4;
            StreamBufferSize = 4 << 10;
#else
            try
            {
                Settings = ConfigurationManager.AppSettings;

                ConfigFile = Settings["configFile"];
                if (ConfigFile == null)
                {
                    ConfigFile = (ConfigPath = CLB.TmphPub.ApplicationPath) + DefaultConfigFile;
                }
                else if (ConfigFile.IndexOf(':') == -1)
                {
                    ConfigFile = (ConfigPath = CLB.TmphPub.ApplicationPath) + ConfigFile;
                }
                else ConfigPath = new FileInfo(ConfigFile).Directory.fullName().ToLower();

                ConfigIncludeName = Settings["configIncludeName"];
                if (ConfigIncludeName == null) ConfigIncludeName = DefaultConfigIncludeName;

                LogPath = Settings["logPath"];
                if (LogPath == null || !TmphDirectory.Create(LogPath = LogPath.pathSuffix().ToLower()))
                    LogPath = CLB.TmphPub.ApplicationPath;

                var maxLogSize = Settings["maxLogSize"];
                if (!int.TryParse(maxLogSize, out MaxLogSize)) MaxLogSize = 1 << 20;

                var maxLogCacheCount = Settings["maxLogCacheCount"];
                if (!int.TryParse(maxLogCacheCount, out MaxLogCacheCount)) MaxLogCacheCount = 1 << 10;

                var jsonParseDepth = Settings["jsonDepth"];
                if (!int.TryParse(jsonParseDepth, out JsonDepth)) JsonDepth = 64;

                var javascriptMinTimeString = Settings["javascriptMinTime"];
                if (!DateTime.TryParse(javascriptMinTimeString, out JavascriptMinTime))
                {
                    JavascriptMinTime = new DateTime(1970, 1, 1, 8, 0, 0);
                }

                if (Settings["isLogConsole"] != null) IsLogConsole = true;
                if (Settings["isErrorLog"] != null) IsErrorLog = true;

                var encoding = Settings["encoding"];
                if (encoding != null)
                {
                    try
                    {
                        Encoding = Encoding.GetEncoding(encoding);
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error.ToString());
                        Encoding = Encoding.UTF8;
                    }
                }
                if (Encoding == null) Encoding = Encoding.UTF8;

                var tinyThreadStackSize = Settings["tinyThreadStackSize"];
                if (!int.TryParse(tinyThreadStackSize, out TinyThreadStackSize)) TinyThreadStackSize = 128 << 10;

                var threadStackSize = Settings["threadStackSize"];
                if (!int.TryParse(threadStackSize, out ThreadStackSize)) ThreadStackSize = 1 << 20;

                var poolSize = Settings["poolSize"];
                if (!int.TryParse(poolSize, out PoolSize)) PoolSize = 4;

                if (Settings["isPoolDebug"] != null) IsPoolDebug = true;

                var streamBufferSize = Settings["streamBufferSize"];
                if (!int.TryParse(streamBufferSize, out StreamBufferSize)) StreamBufferSize = 4 << 10;

                if (Settings["isCheckMemory"] != null) IsCheckMemory = true;
            }
            catch (Exception error)
            {
                Settings = new NameValueCollection();
                ConfigPath = LogPath = CLB.TmphPub.ApplicationPath;
                ConfigIncludeName = DefaultConfigIncludeName;
                MaxLogSize = 1 << 20;
                MaxLogCacheCount = 1 << 10;
                JsonDepth = 64;
                JavascriptMinTime = new DateTime(1970, 1, 1, 8, 0, 0);
                Encoding = Encoding.UTF8;
                TinyThreadStackSize = 128 << 10;
                ThreadStackSize = 1 << 20;
                PoolSize = 4;
                StreamBufferSize = 4 << 10;
                Console.WriteLine(error.ToString());
            }
#endif
        }
    }
}