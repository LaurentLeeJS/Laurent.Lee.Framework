using System;
using System.Collections.Generic;
using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.MemoryDataBase.Cache;

namespace Laurent.Lee.CLB.MemoryDatabase.Cache
{
    /// <summary>
    /// 字典缓存
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    /// <typeparam name="TModelType"></typeparam>
    /// <typeparam name="TKeyType"></typeparam>
    public class TmphDictionary<TValueType, TModelType, TKeyType> : TmphLoadCache<TValueType, TModelType, TKeyType>, TmphILoadCache<TValueType, TModelType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
        /// <summary>
        /// 数据集合
        /// </summary>
        private Dictionary<TKeyType, TmphCacheValue> values = TmphDictionary<TKeyType>.Create<TmphCacheValue>(cacheCapacity);
        /// <summary>
        /// 对象数量
        /// </summary>
        public int Count { get { return values.Count; } }
        /// <summary>
        /// 枚举数据集合
        /// </summary>
        public override IEnumerable<TValueType> Values
        {
            get
            {
                foreach (TmphCacheValue value in values.Values) yield return value.Value;
            }
        }
        /// <summary>
        /// 获取数组
        /// </summary>
        /// <returns></returns>
        public override TmphSubArray<TValueType> GetSubArray()
        {
            TmphSubArray<TValueType> TmphSubArray = new TmphSubArray<TValueType>(Count);
            foreach (TmphCacheValue value in values.Values) TmphSubArray.Add(value.Value);
            return TmphSubArray;
        }
        /// <summary>
        /// 获取数组
        /// </summary>
        /// <returns></returns>
        public override TValueType[] GetArray()
        {
            return GetSubArray().ToArray();
        }
        /// <summary>
        /// 是否存在关键字
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>是否存在对象</returns>
        public bool ContainsKey(TKeyType key)
        {
            return values.ContainsKey(key);
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据,失败返回null</returns>
        public override TValueType Get(TKeyType key)
        {
            TmphCacheValue value;
            return values.TryGetValue(key, out value) ? value.Value : null;
        }
        /// <summary>
        /// 加载日志添加对象
        /// </summary>
        /// <param name="value">添加的对象</param>
        /// <param name="key"></param>
        /// <param name="logSize">日志字节长度</param>
        public void LoadInsert(TValueType value, TKeyType key, int logSize)
        {
            values[key] = new TmphCacheValue { Value = value, LogSize = logSize };
        }
        /// <summary>
        /// 加载日志修改对象
        /// </summary>
        /// <param name="value">修改的对象</param>
        /// <param name="key"></param>
        /// <param name="memberMap">修改对象成员位图</param>
        public void LoadUpdate(TValueType value, TKeyType key, TmphMemberMap memberMap)
        {
            TmphCacheValue cacheValue;
            if (values.TryGetValue(key, out cacheValue))
            {
                Laurent.Lee.CLB.Emit.TmphMemberCopyer<TModelType>.Copy(cacheValue.Value, value, memberMap);
            }
        }
        /// <summary>
        /// 加载日志删除对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns>日志字节长度</returns>
        public int LoadDelete(TKeyType key)
        {
            TmphCacheValue cacheValue;
            if (values.TryGetValue(key, out cacheValue))
            {
                values.Remove(key);
                return cacheValue.LogSize;
            }
            return 0;
        }
        /// <summary>
        /// 日志数据加载完成
        /// </summary>
        /// <param name="isLoaded">是否加载成功</param>
        public void Loaded(bool isLoaded)
        {
            if (!isLoaded) Dispose();
            onLoaded();
        }
        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="value">添加的对象</param>
        /// <param name="key"></param>
        /// <param name="logSize">日志字节长度</param>
        /// <param name="isCopy">是否浅复制缓存对象值,否则返回缓存对象</param>
        /// <returns>添加的对象</returns>
        protected override TValueType insert(TValueType value, TKeyType key, int logSize, bool isCopy)
        {
            if (isCopy) value = Laurent.Lee.CLB.Emit.TmphMemberCopyer<TValueType>.MemberwiseClone(value);
            values[key] = new TmphCacheValue { Value = value, LogSize = logSize };
            return value;
        }
        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="logSize">日志字节长度</param>
        /// <returns>被删除对象,失败返回null</returns>
        protected override TValueType delete(TKeyType key, out int logSize)
        {
            TmphCacheValue cacheValue;
            if (values.TryGetValue(key, out cacheValue))
            {
                values.Remove(key);
                logSize = cacheValue.LogSize;
                return cacheValue.Value;
            }
            logSize = 0;
            return null;
        }
    }
    /// <summary>
    /// 字典缓存
    /// </summary>
    /// <typeparam name="TModelType"></typeparam>
    /// <typeparam name="TKeyType"></typeparam>
    public sealed class TmphDictionary<TModelType, TKeyType> : TmphDictionary<TModelType, TModelType, TKeyType>
        where TModelType : class
        where TKeyType : IEquatable<TKeyType>
    {
    }
}
