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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.MemoryDataBase.Cache;
using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB.MemoryDatabase.Cache
{
    /// <summary>
    /// 搜索树缓存
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    /// <typeparam name="TModelType"></typeparam>
    /// <typeparam name="TKeyType"></typeparam>
    public class TmphSearchTree<TValueType, TModelType, TKeyType> : TmphLoadCache<TValueType, TModelType, TKeyType>, TmphILoadCache<TValueType, TModelType, TKeyType>
        where TValueType : class, TModelType
        where TModelType : class
        where TKeyType : IComparable<TKeyType>
    {
        /// <summary>
        /// 数据集合
        /// </summary>
        private Laurent.Lee.CLB.TmphSearchTree<TKeyType, TmphCacheValue> tree = new Laurent.Lee.CLB.TmphSearchTree<TKeyType, TmphCacheValue>();

        /// <summary>
        /// 对象数量
        /// </summary>
        public int Count { get { return tree.Count; } }

        /// <summary>
        /// 枚举数据集合
        /// </summary>
        public override IEnumerable<TValueType> Values
        {
            get
            {
                foreach (TmphCacheValue value in tree.GetArray()) yield return value.Value;
            }
        }

        /// <summary>
        /// 获取数组
        /// </summary>
        /// <returns></returns>
        public override TmphSubArray<TValueType> GetSubArray()
        {
            return new TmphSubArray<TValueType>(GetArray());
        }

        /// <summary>
        /// 获取数组
        /// </summary>
        /// <returns></returns>
        public override TValueType[] GetArray()
        {
            return tree.GetArray(TmphCacheValue.GetValue);
        }

        /// <summary>
        /// 是否存在关键字
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>是否存在对象</returns>
        public bool ContainsKey(TKeyType key)
        {
            return tree.ContainsKey(key);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns>数据,失败返回null</returns>
        public override TValueType Get(TKeyType key)
        {
            TmphCacheValue value;
            return tree.TryGetValue(key, out value) ? value.Value : null;
        }

        /// <summary>
        /// 加载日志添加对象
        /// </summary>
        /// <param name="value">添加的对象</param>
        /// <param name="key"></param>
        /// <param name="logSize">日志字节长度</param>
        public void LoadInsert(TValueType value, TKeyType key, int logSize)
        {
            tree[key] = new TmphCacheValue { Value = value, LogSize = logSize };
        }

        /// <summary>
        /// 加载日志修改对象
        /// </summary>
        /// <param name="value">修改的对象</param>
        /// <param name="key"></param>
        /// <param name="memberMap">修改对象成员位图</param>
        public void LoadUpdate(TValueType value, TKeyType key, TmphMemberMap memberMap)
        {
            TmphCacheValue TCacheValue;
            if (tree.TryGetValue(key, out TCacheValue))
            {
                Laurent.Lee.CLB.Emit.TmphMemberCopyer<TModelType>.Copy(TCacheValue.Value, value, memberMap);
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
            return tree.Remove(key, out cacheValue) ? cacheValue.LogSize : 0;
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
            tree[key] = new TmphCacheValue { Value = value, LogSize = logSize };
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
            if (tree.Remove(key, out cacheValue))
            {
                logSize = cacheValue.LogSize;
                return cacheValue.Value;
            }
            logSize = 0;
            return null;
        }
    }

    /// <summary>
    /// 搜索树缓存
    /// </summary>
    /// <typeparam name="TModelType"></typeparam>
    /// <typeparam name="TKeyType"></typeparam>
    public sealed class TmphSearchTree<TModelType, TKeyType> : TmphSearchTree<TModelType, TModelType, TKeyType>
        where TModelType : class
        where TKeyType : IComparable<TKeyType>
    {
    }
}