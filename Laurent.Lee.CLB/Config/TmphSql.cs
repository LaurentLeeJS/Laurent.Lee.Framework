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

namespace Laurent.Lee.CLB.Config
{
    /// <summary>
    ///     SQL相关参数
    /// </summary>
    public sealed class TmphSql : TmphDatabase
    {
        /// <summary>
        ///     默认SQL相关参数
        /// </summary>
        public static readonly TmphSql Default = new TmphSql();

        /// <summary>
        ///     检测链接类型集合
        /// </summary>
        private readonly string[] _checkConnection = null;

        /// <summary>
        ///     数据更新客户端链接类型集合
        /// </summary>
        private readonly string[] _checkQueueClient = null;

        /// <summary>
        ///     缓存默认最大容器大小
        /// </summary>
        private readonly int cacheMaxCount = 1 << 10;

        /// <summary>
        ///     默认更新队列容器大小
        /// </summary>
        private readonly int defaultUpdateQueueSize = 100000;

        /// <summary>
        ///     每批导入记录数量
        /// </summary>
        private readonly int importBatchSize = 10000;

        /// <summary>
        ///     SQL表格名称缺省前缀深度
        /// </summary>
        private readonly int tableNameDepth = 2;

        /// <summary>
        ///     更新队列超时
        /// </summary>
        private readonly int updateQueueTimeoutSeconds = 60;

        /// <summary>
        ///     SQL表格名称前缀集合
        /// </summary>
        private string[] _tableNamePrefixs;

        /// <summary>
        ///     SQL相关参数
        /// </summary>
        private TmphSql()
        {
            TmphPub.LoadConfig(this);
            if (_checkConnection.length() != 0)
                TmphLog.Default.Add("数据库链接处理类型: " + _checkConnection.joinString(',', value => value.ToString()));
        }

        /// <summary>
        ///     检测链接类型集合
        /// </summary>
        public string[] CheckConnection
        {
            get { return _checkConnection ?? TmphNullValue<string>.Array; }
        }

        /// <summary>
        ///     数据更新客户端链接类型集合
        /// </summary>
        public string[] CheckQueueClient
        {
            get { return _checkQueueClient ?? TmphNullValue<string>.Array; }
        }

        /// <summary>
        ///     缓存默认最大容器大小
        /// </summary>
        public int CacheMaxCount
        {
            get { return cacheMaxCount; }
        }

        /// <summary>
        ///     默认更新队列容器大小
        /// </summary>
        public int DefaultUpdateQueueSize
        {
            get { return defaultUpdateQueueSize <= 4 ? 100000 : defaultUpdateQueueSize; }
        }

        /// <summary>
        ///     更新队列超时
        /// </summary>
        public int UpdateQueueTimeoutSeconds
        {
            get { return updateQueueTimeoutSeconds; }
        }

        /// <summary>
        ///     SQL表格名称前缀集合
        /// </summary>
        public string[] TableNamePrefixs
        {
            get
            {
                if (_tableNamePrefixs == null) _tableNamePrefixs = TmphNullValue<string>.Array;
                return _tableNamePrefixs;
            }
        }

        /// <summary>
        ///     SQL表格名称缺省前缀深度
        /// </summary>
        public int TableNameDepth
        {
            get { return tableNameDepth; }
        }

        /// <summary>
        ///     每批导入记录数量
        /// </summary>
        public int ImportBatchSize
        {
            get { return importBatchSize > 0 ? importBatchSize : 10000; }
        }
    }
}