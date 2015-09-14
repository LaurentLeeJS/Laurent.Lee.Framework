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
using Laurent.Lee.CLB.Config;
using Laurent.Lee.CLB.Emit;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Laurent.Lee.CLB.MemoryDataBase.Cache
{
    /// <summary>
    ///     数据加载基本缓存接口
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    public interface TmphILoadCache<TValueType, TModelType> : IDisposable
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        ///     对象数量
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     枚举数据集合
        /// </summary>
        IEnumerable<TValueType> Values { get; }

        /// <summary>
        ///     获取数据集合
        /// </summary>
        /// <returns></returns>
        TmphSubArray<TValueType> GetSubArray();

        /// <summary>
        ///     获取数据集合
        /// </summary>
        /// <returns></returns>
        TValueType[] GetArray();

        /// <summary>
        ///     日志数据加载完成
        /// </summary>
        /// <param name="isLoadedOk">是否加载成功</param>
        void Loaded(bool isLoadedOk);

        /// <summary>
        ///     等待缓存加载结束
        /// </summary>
        void WaitLoad();

        /// <summary>
        ///     添加对象事件
        /// </summary>
        event Action<TValueType> OnInserted;

        /// <summary>
        ///     修改对象事件[修改后的值,对象原值,更新成员位图]
        /// </summary>
        event Action<TValueType, TValueType, TmphMemberMap> OnUpdated;

        /// <summary>
        ///     删除对象事件
        /// </summary>
        event Action<TValueType> OnDeleted;
    }

    /// <summary>
    ///     数据加载基本缓存接口
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public interface TmphILoadCache<TValueType, TModelType, TKeyType> : TmphILoadCache<TValueType, TModelType>, IDisposable
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        ///     是否存在关键字
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>是否存在对象</returns>
        bool ContainsKey(TKeyType key);

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据,失败返回null</returns>
        TValueType Get(TKeyType key);

        /// <summary>
        ///     加载日志添加对象
        /// </summary>
        /// <param name="value">添加的对象</param>
        /// <param name="key">关键字</param>
        /// <param name="logSize">日志字节长度</param>
        void LoadInsert(TValueType value, TKeyType key, int logSize);

        /// <summary>
        ///     加载日志修改对象
        /// </summary>
        /// <param name="value">修改的对象</param>
        /// <param name="key">关键字</param>
        /// <param name="memberMap">修改对象成员位图</param>
        void LoadUpdate(TValueType value, TKeyType key, TmphMemberMap memberMap);

        /// <summary>
        ///     加载日志删除对象
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>日志字节长度</returns>
        int LoadDelete(TKeyType key);

        /// <summary>
        ///     添加对象
        /// </summary>
        /// <param name="value">添加的对象</param>
        /// <param name="key">关键字</param>
        /// <param name="logSize">日志字节长度</param>
        /// <param name="isCopy">是否浅复制缓存对象值,否则返回缓存对象</param>
        /// <returns>添加的对象</returns>
        TValueType Insert(TValueType value, TKeyType key, int logSize, bool isCopy);

        /// <summary>
        ///     修改对象
        /// </summary>
        /// <param name="value">修改的对象</param>
        /// <param name="key">关键字</param>
        /// <param name="memberMap">修改对象成员位图</param>
        /// <param name="isCopy">是否浅复制缓存对象值,否则返回缓存对象</param>
        /// <returns>修改后的对象</returns>
        TValueType Update(TValueType value, TKeyType key, TmphMemberMap memberMap);

        /// <summary>
        ///     删除对象
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="logSize">日志字节长度</param>
        /// <returns>被删除的对象值,日志字节长度</returns>
        TValueType Delete(TKeyType key, out int logSize);
    }

    /// <summary>
    ///     自增数据加载基本缓存接口
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public interface TmphILoadIdentityCache<TValueType, TModelType> : TmphILoadCache<TValueType, TModelType, int>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        ///     获取下一个自增值
        /// </summary>
        /// <returns>自增值</returns>
        int NextIdentity();
    }

    /// <summary>
    ///     数据加载基本缓存
    /// </summary>
    public abstract class TmphLoadCache : IDisposable
    {
        /// <summary>
        ///     自增缓存数组默认容器尺寸
        /// </summary>
        protected static readonly int cacheCapacity = TmphMemoryDatabase.Default.CacheCapacity;

        /// <summary>
        ///     是否已经释放资源
        /// </summary>
        protected int isDisposed;

        /// <summary>
        ///     是否加载完成
        /// </summary>
        protected int isLoaded;

        /// <summary>
        ///     释放资源
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Increment(ref isDisposed) == 1) dispose();
        }

        /// <summary>
        ///     日志数据成功加载完成事件
        /// </summary>
        public event Action OnLoaded;

        /// <summary>
        ///     日志数据成功加载完成事件
        /// </summary>
        protected void onLoaded()
        {
            if (Interlocked.CompareExchange(ref isLoaded, 1, 0) == 0)
            {
                if (OnLoaded != null) OnLoaded();
            }
        }

        /// <summary>
        ///     等待缓存加载结束
        /// </summary>
        public void WaitLoad()
        {
            while (isLoaded == 0) Thread.Sleep(1);
        }

        /// <summary>
        ///     释放资源
        /// </summary>
        protected virtual void dispose()
        {
            onLoaded();
        }
    }

    /// <summary>
    ///     数据加载基本缓存
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    public abstract class TmphLoadCache<TValueType> : TmphLoadCache
        where TValueType : class
    {
        /// <summary>
        ///     枚举数据集合
        /// </summary>
        public abstract IEnumerable<TValueType> Values { get; }

        /// <summary>
        ///     获取数据集合
        /// </summary>
        /// <returns></returns>
        public abstract TmphSubArray<TValueType> GetSubArray();

        /// <summary>
        ///     获取数据集合
        /// </summary>
        /// <returns></returns>
        public abstract TValueType[] GetArray();

        /// <summary>
        ///     数据对象
        /// </summary>
        protected struct TmphArrayValue
        {
            /// <summary>
            ///     关键字操作锁
            /// </summary>
            public int Lock;

            /// <summary>
            ///     日志字节长度
            /// </summary>
            public int LogSize;

            /// <summary>
            ///     数据对象
            /// </summary>
            public TValueType Value;

            /// <summary>
            ///     设置数据对象
            /// </summary>
            /// <param name="value"></param>
            /// <param name="logSize"></param>
            public void Set(TValueType value, int logSize)
            {
                Value = value;
                LogSize = logSize;
            }

            /// <summary>
            ///     删除数据
            /// </summary>
            /// <param name="logSize"></param>
            /// <returns></returns>
            public TValueType Clear(out int logSize)
            {
                var value = Value;
                logSize = LogSize;
                Value = null;
                return value;
            }
        }

        /// <summary>
        ///     缓存数据
        /// </summary>
        protected sealed class TmphCacheValue
        {
            /// <summary>
            ///     获取数据委托
            /// </summary>
            public static readonly Func<TmphCacheValue, TValueType> GetValue = getValue;

            /// <summary>
            ///     关键字操作锁
            /// </summary>
            public int Lock;

            /// <summary>
            ///     日志字节长度
            /// </summary>
            public int LogSize;

            /// <summary>
            ///     数据对象
            /// </summary>
            public TValueType Value;

            /// <summary>
            ///     获取数据
            /// </summary>
            /// <param name="value"></param>
            private static TValueType getValue(TmphCacheValue value)
            {
                return value.Value;
            }
        }
    }

    /// <summary>
    ///     数据加载基本缓存
    /// </summary>
    /// <typeparam name="TValueType">数据类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    /// <typeparam name="TKeyType">关键字类型</typeparam>
    public abstract class TmphLoadCache<TValueType, TModelType, TKeyType> : TmphLoadCache<TValueType>
        where TValueType : class, TModelType
    {
        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据,失败返回null</returns>
        public abstract TValueType Get(TKeyType key);

        /// <summary>
        ///     添加对象事件
        /// </summary>
        public event Action<TValueType> OnInserted;

        /// <summary>
        ///     添加对象
        /// </summary>
        /// <param name="value">添加的对象</param>
        /// <param name="key">关键字</param>
        /// <param name="logSize">日志字节长度</param>
        /// <param name="isCopy">是否浅复制缓存对象值,否则返回缓存对象</param>
        /// <returns>添加的对象</returns>
        public TValueType Insert(TValueType value, TKeyType key, int logSize, bool isCopy)
        {
            if (((isLoaded ^ 1) | isDisposed) == 0 && (value = insert(value, key, logSize, isCopy)) != null)
            {
                if (OnInserted != null) OnInserted(value);
                return value;
            }
            return null;
        }

        /// <summary>
        ///     添加对象
        /// </summary>
        /// <param name="value">添加的对象</param>
        /// <param name="key">关键字</param>
        /// <param name="logSize">日志字节长度</param>
        /// <param name="isCopy">是否浅复制缓存对象值,否则返回缓存对象</param>
        /// <returns>添加的对象</returns>
        protected abstract TValueType insert(TValueType value, TKeyType key, int logSize, bool isCopy);

        /// <summary>
        ///     修改对象事件[修改后的值,对象原值,更新成员位图]
        /// </summary>
        public event Action<TValueType, TValueType, TmphMemberMap> OnUpdated;

        /// <summary>
        ///     修改对象
        /// </summary>
        /// <param name="value">修改的对象</param>
        /// <param name="key">关键字</param>
        /// <param name="memberMap">修改对象成员位图</param>
        /// <returns>修改后的对象值</returns>
        public TValueType Update(TValueType value, TKeyType key, TmphMemberMap memberMap)
        {
            if (((isLoaded ^ 1) | isDisposed) == 0)
            {
                var cacheValue = Get(key);
                if (cacheValue != null)
                {
                    var oldValue = TmphConstructor<TValueType>.New();
                    TmphMemberCopyer<TModelType>.Copy(oldValue, cacheValue);
                    TmphMemberCopyer<TModelType>.Copy(cacheValue, value, memberMap);
                    if (OnUpdated != null) OnUpdated(cacheValue, oldValue, memberMap);
                    return cacheValue;
                }
            }
            return null;
        }

        /// <summary>
        ///     删除对象事件
        /// </summary>
        public event Action<TValueType> OnDeleted;

        /// <summary>
        ///     删除对象
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="logSize">日志字节长度</param>
        /// <returns>被删除对象,失败返回null</returns>
        public TValueType Delete(TKeyType key, out int logSize)
        {
            if (((isLoaded ^ 1) | isDisposed) == 0)
            {
                var value = delete(key, out logSize);
                if (value != null)
                {
                    if (OnDeleted != null) OnDeleted(value);
                    return value;
                }
            }
            logSize = 0;
            return null;
        }

        /// <summary>
        ///     删除对象
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="logSize">日志字节长度</param>
        /// <returns>被删除对象,失败返回null</returns>
        protected abstract TValueType delete(TKeyType key, out int logSize);
    }
}