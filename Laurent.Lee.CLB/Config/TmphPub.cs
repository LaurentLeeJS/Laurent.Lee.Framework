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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Emit;
using System;
using System.Collections.Generic;
using System.IO;

namespace Laurent.Lee.CLB.Config
{
    /// <summary>
    ///     基本配置
    /// </summary>
    public sealed class TmphPub
    {
        ///// <summary>
        ///// 获取配置
        ///// </summary>
        ///// <param name="type">配置类型</param>
        ///// <returns>配置</returns>
        //private json.node getConfig(Type type)
        //{
        //    if (configs.Type != json.node.nodeType.Null)
        //    {
        //        string name = type.FullName, fastCSharpName = Laurent.Lee.CLB.pub.Laurent.Lee.CLB + ".";
        //        if (name.StartsWith(fastCSharpName, StringComparison.Ordinal))
        //        {
        //            string fastCSharpConfig = fastCSharpName + "TmphConfig.";
        //            name = name.Substring(name.StartsWith(fastCSharpConfig, StringComparison.Ordinal) ? fastCSharpConfig.Length : fastCSharpName.Length);
        //        }
        //        json.node TmphConfig = configs;
        //        foreach (string tagName in name.Split('.'))
        //        {
        //            if (TmphConfig.Type != json.node.nodeType.Dictionary || (TmphConfig = TmphConfig[tagName]).Type == json.node.nodeType.Null)
        //            {
        //                return default(json.node);
        //            }
        //        }
        //        return TmphConfig;
        //    }
        //    return default(json.node);
        //}
        ///// <summary>
        ///// 配置加载
        ///// </summary>
        ///// <param name="value">配置对象</param>
        ///// <param name="name">配置名称,null表示只匹配类型</param>
        //public TValueType LoadConfig<TValueType>(string name = null) where TValueType : struct
        //{
        //    TValueType value = default(TValueType);
        //    return loadConfig(ref value, name);
        //}
        /// <summary>
        ///     Laurent.Lee.CLB命名空间
        /// </summary>
        private const string LeeFrameWorkName = CLB.TmphPub.LaurentLeeFramework + ".";

        /// <summary>
        ///     Laurent.Lee.CLB配置命名空间
        /// </summary>
        private const string LeeFrameWorkConfigName = LeeFrameWorkName + "TmphConfig.";

        /// <summary>
        ///     配置JSON解析参数
        /// </summary>
        private static readonly TmphJsonParser.TmphConfig JsonConfig = new TmphJsonParser.TmphConfig
        {
            MemberFilter = TmphMemberFilters.Instance
        };

        /// <summary>
        ///     配置集合
        /// </summary>
        private static readonly Dictionary<TmphHashString, TmphSubString> Configs;

        /// <summary>
        ///     默认基本配置
        /// </summary>
        public static readonly TmphPub Default;

        /// <summary>
        ///     死锁检测分钟数,0表示不检测
        /// </summary>
        private readonly int lockCheckMinutes = 0;

        /// <summary>
        ///     默认分页大小
        /// </summary>
        private readonly int maxEnumArraySize = 1024;

        /// <summary>
        ///     成员位图内存池支持最大成员数量
        /// </summary>
        private readonly int maxMemberMapCount = 1024;

        /// <summary>
        ///     服务器端套接字单次最大发送数据量(单位:KB)
        /// </summary>
        private readonly int maxServerSocketSendSize = 8;

        /// <summary>
        ///     成员位图内存池大小(单位:KB)
        /// </summary>
        private readonly int memberMapPoolSize = 8;

        /// <summary>
        ///     默认分页大小
        /// </summary>
        private readonly int pageSize = 10;

        /// <summary>
        ///     原始套接字监听缓冲区尺寸(单位:KB)
        /// </summary>
        private readonly int rawSocketBufferSize = 1024;

        /// <summary>
        ///     任务最大线程数
        /// </summary>
        private readonly int taskMaxThreadCount = 65536;

        /// <summary>
        ///     默认任务线程数
        /// </summary>
        private readonly int taskThreadCount = 128;

        /// <summary>
        ///     微型线程任务线程数
        /// </summary>
        private readonly int tinyThreadCount = 65536;

        static TmphPub()
        {
            Configs = TmphDictionary.CreateHashString<TmphSubString>();
            new TmphLoader().Load(TmphAppSetting.ConfigFile);
            Default = new TmphPub();
        }

        /// <summary>
        ///     基本配置
        /// </summary>
        private TmphPub()
        {
            LoadConfig(this);
            if (WorkPath == null) WorkPath = CLB.TmphPub.ApplicationPath;
            else WorkPath = WorkPath.pathSuffix().ToLower();
            if (CachePath == null || !TmphDirectory.Create(CachePath = CachePath.pathSuffix().ToLower()))
                CachePath = CLB.TmphPub.ApplicationPath;
        }

        /// <summary>
        ///     程序工作主目录
        /// </summary>
        public string WorkPath { get; private set; }

        /// <summary>
        ///     缓存文件主目录
        /// </summary>
        public string CachePath { get; private set; }

        /// <summary>
        ///     是否调试模式
        /// </summary>
        public bool IsDebug { get; private set; }

        /// <summary>
        ///     是否window服务模式
        /// </summary>
        public bool IsService { get; private set; }

        /// <summary>
        ///     默认分页大小
        /// </summary>
        public int PageSize
        {
            get { return pageSize; }
        }

        /// <summary>
        ///     最大枚举数组数量
        /// </summary>
        public int MaxEnumArraySize
        {
            get { return maxEnumArraySize; }
        }

        /// <summary>
        ///     默认任务线程数
        /// </summary>
        public int TaskThreadCount
        {
            get
            {
                if (taskThreadCount > taskMaxThreadCount)
                {
                    TmphLog.Error.Add("默认任务线程数[" + taskThreadCount.toString() + "] 超出 任务最大线程数[" +
                                    taskMaxThreadCount.toString() + "]");
                    return taskMaxThreadCount;
                }
                return taskThreadCount;
            }
        }

        /// <summary>
        ///     任务最大线程数
        /// </summary>
        public int TaskMaxThreadCount
        {
            get { return taskMaxThreadCount; }
        }

        /// <summary>
        ///     死锁检测分钟数,0表示不检测
        /// </summary>
        public int LockCheckMinutes
        {
            get { return lockCheckMinutes; }
        }

        /// <summary>
        ///     微型线程任务线程数
        /// </summary>
        public int TinyThreadCount
        {
            get
            {
                if (tinyThreadCount > taskMaxThreadCount)
                {
                    TmphLog.Error.Add("微型线程任务线程数[" + tinyThreadCount.toString() + "] 超出 任务最大线程数[" +
                                    taskMaxThreadCount.toString() + "]");
                    return taskMaxThreadCount;
                }
                return tinyThreadCount;
            }
        }

        /// <summary>
        ///     原始套接字监听缓冲区尺寸(单位:B)
        /// </summary>
        public int RawSocketBufferSize
        {
            get { return Math.Max(1024 << 10, rawSocketBufferSize << 10); }
        }

        /// <summary>
        ///     服务器端套接字单次最大发送数据量(单位:B)
        /// </summary>
        public int MaxServerSocketSendSize
        {
            get { return Math.Max(4 << 10, maxServerSocketSendSize << 10); }
        }

        /// <summary>
        ///     成员位图内存池大小(单位:B)
        /// </summary>
        public int MemberMapPoolSize
        {
            get { return Math.Max(4 << 10, memberMapPoolSize << 10); }
        }

        /// <summary>
        ///     成员位图内存池支持最大成员数量
        /// </summary>
        public int MaxMemberMapCount
        {
            get { return (Math.Max(1024, maxMemberMapCount) + 63) & 0x7fffffc0; }
        }

        /// <summary>
        ///     配置加载
        /// </summary>
        /// <param name="value">配置对象</param>
        /// <param name="name">配置名称,null表示只匹配类型</param>
        public static TValueType LoadConfig<TValueType>(TValueType value, string name = null) where TValueType : class
        {
            if (value == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            LoadConfigBase(ref value, name);
            return value;
        }

        /// <summary>
        ///     配置加载
        /// </summary>
        /// <param name="value">配置对象</param>
        /// <param name="name">配置名称,null表示只匹配类型</param>
        public static void LoadConfig<TValueType>(ref TValueType value, string name = null) where TValueType : struct
        {
            LoadConfigBase(ref value, name);
        }

        /// <summary>
        ///     配置加载
        /// </summary>
        /// <param name="value">配置对象</param>
        /// <param name="name">配置名称,null表示只匹配类型</param>
        private static void LoadConfigBase<TValueType>(ref TValueType value, string name)
        {
            if (name == null) name = value.GetType().FullName.ReplaceNotNull('+', '.');
            else name = value.GetType().FullName.ReplaceNotNull('+', '.') + "." + name;
            if (name.StartsWith(LeeFrameWorkName, StringComparison.Ordinal))
            {
                name =
                    name.Substring(name.StartsWith(LeeFrameWorkConfigName, StringComparison.Ordinal)
                        ? LeeFrameWorkConfigName.Length
                        : LeeFrameWorkName.Length);
            }
            TmphSubString json;
            if (Configs.TryGetValue(name, out json)) TmphJsonParser.Parse(json, ref value, JsonConfig);
        }

        /// <summary>
        ///     判断配置名称是否存在
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns>配置是否加载存在</returns>
        public static bool IsConfigName(string name)
        {
            return Configs.ContainsKey(name);
        }

        /// <summary>
        ///     配置文件加载
        /// </summary>
        private struct TmphLoader
        {
            /// <summary>
            ///     错误信息集合
            /// </summary>
            private TmphList<string> _errors;

            /// <summary>
            ///     历史配置文件
            /// </summary>
            private TmphList<string> _files;

            /// <summary>
            ///     加载配置文件
            /// </summary>
            /// <param name="configFile">配置文件名称</param>
            public void Load(string configFile)
            {
                if (_files == null)
                {
                    _files = new TmphList<string>();
                    _errors = new TmphList<string>();
                }
                try
                {
                    Load(new FileInfo(configFile));
                }
                catch (Exception error)
                {
                    TmphLog.Error.Real(error, "配置文件加载失败 : " + configFile, false);
                }
            }

            /// <summary>
            ///     加载配置文件
            /// </summary>
            /// <param name="file">配置文件</param>
            private unsafe void Load(FileInfo file)
            {
                if (file.Exists)
                {
                    var fileName = file.FullName.ToLower();
                    var count = _files.Count;
                    if (count != 0)
                    {
                        foreach (var name in _files.array)
                        {
                            if (_errors.Count == 0)
                            {
                                if (name == fileName)
                                {
                                    _errors.Add("配置文件循环嵌套");
                                    _errors.Add(name);
                                }
                            }
                            else _errors.Add(name);
                            if (--count == 0) break;
                        }
                        if (_errors.Count != 0)
                        {
                            TmphLog.Error.Real(_errors.JoinString(@"
"));
                            _errors.Empty();
                        }
                    }
                    var TmphConfig = File.ReadAllText(fileName, TmphAppSetting.Encoding);
                    fixed (char* configFixed = TmphConfig)
                    {
                        for (char* current = configFixed, end = configFixed + TmphConfig.Length; current != end;)
                        {
                            var start = current;
                            while (*current != '=' && ++current != end)
                            {
                            }
                            if (current == end) break;
                            var name = TmphSubString.Unsafe(TmphConfig, (int)(start - configFixed), (int)(current - start));
                            if (name.Equals(TmphAppSetting.ConfigIncludeName))
                            {
                                for (start = ++current; current != end && *current != '\n'; ++current)
                                {
                                }
                                Load(Path.Combine(file.DirectoryName,
                                    TmphConfig.Substring((int)(start - configFixed), (int)(current - start)).Trim()));
                                if (current == end) break;
                                ++current;
                            }
                            else
                            {
                                for (start = ++current; current != end; ++current)
                                {
                                    if (*current == '\n')
                                    {
                                        while (++current != end && *current == '\n')
                                        {
                                        }
                                        if (current == end) break;
                                        if (*current != '\t' && *current != ' ') break;
                                    }
                                }
                                TmphHashString nameKey = name;
                                if (Configs.ContainsKey(nameKey))
                                {
                                    TmphLog.Error.Real("重复的配置名称 : " + name);
                                }
                                else
                                    Configs.Add(nameKey,
                                        TmphSubString.Unsafe(TmphConfig, (int)(start - configFixed), (int)(current - start)));
                            }
                        }
                    }
                }
                else TmphLog.Default.Real("找不到配置文件 : " + file.FullName);
            }
        }
    }
}