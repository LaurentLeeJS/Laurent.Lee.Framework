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

namespace Laurent.Lee.CLB.Config
{
    /// <summary>
    ///     内存数据库相关参数
    /// </summary>
    public sealed class TmphMemoryDatabase : TmphDatabase
    {
        /// <summary>
        ///     数据库日志文件最小刷新尺寸(单位:KB)
        /// </summary>
        internal const int DefaultMinRefreshSize = 1024;

        /// <summary>
        ///     物理层最小数据缓冲区字节数
        /// </summary>
        internal const int MinPhysicalBufferSize = 1 << 12;

        /// <summary>
        ///     默认内存数据库相关参数
        /// </summary>
        public static readonly TmphMemoryDatabase Default = new TmphMemoryDatabase();

        /// <summary>
        ///     客户端缓存字节数
        /// </summary>
        private readonly int bufferSize = 1 << 10;

        /// <summary>
        ///     缓存默认容器尺寸(单位:2^n)
        /// </summary>
        private readonly byte cacheCapacity = 16;

        /// <summary>
        ///     数据库日志文件最大刷新比例(:KB)
        /// </summary>
        private readonly int maxRefreshPerKB = 512;

        /// <summary>
        ///     数据库日志文件最小刷新尺寸(单位:KB)
        /// </summary>
        private readonly int minRefreshSize = DefaultMinRefreshSize;

        /// <summary>
        ///     物理层默认数据缓冲区字节数(单位:2^n)
        /// </summary>
        private readonly byte physicalBufferSize = 16;

        /// <summary>
        ///     数据库文件刷新超时秒数
        /// </summary>
        private readonly int refreshTimeOutSeconds = 30;

        /// <summary>
        ///     物理层服务验证
        /// </summary>
        public string PhysicalVerify;

        /// <summary>
        ///     内存数据库相关参数
        /// </summary>
        private TmphMemoryDatabase()
        {
            TmphPub.LoadConfig(this);
        }

        /// <summary>
        ///     数据库文件刷新超时周期
        /// </summary>
        public long RefreshTimeOutTicks
        {
            get { return new TimeSpan(0, 0, 0, refreshTimeOutSeconds <= 0 ? 30 : refreshTimeOutSeconds).Ticks; }
        }

        /// <summary>
        ///     数据库日志文件最小刷新字节数
        /// </summary>
        public int MinRefreshSize
        {
            get { return minRefreshSize <= DefaultMinRefreshSize ? DefaultMinRefreshSize : minRefreshSize; }
        }

        /// <summary>
        ///     数据库日志文件最大刷新比例(:KB)
        /// </summary>
        public int MaxRefreshPerKb
        {
            get { return maxRefreshPerKB > 0 ? maxRefreshPerKB : 512; }
        }

        /// <summary>
        ///     缓存默认容器尺寸
        /// </summary>
        public int CacheCapacity
        {
            get { return cacheCapacity >= 8 && cacheCapacity <= 30 ? 1 << cacheCapacity : (1 << 16); }
        }

        /// <summary>
        ///     客户端缓存字节数
        /// </summary>
        public int BufferSize
        {
            get { return bufferSize <= 0 ? 1 << 10 : bufferSize; }
        }

        /// <summary>
        ///     物理层默认数据缓冲区字节数
        /// </summary>
        public int PhysicalBufferSize
        {
            get { return physicalBufferSize >= 12 && physicalBufferSize <= 30 ? 1 << physicalBufferSize : (1 << 16); }
        }
    }
}