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
using System.Threading;

namespace Laurent.Lee.CLB.MemoryDatabase.Cache
{
    /// <summary>
    /// 自增数组缓存
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    /// <typeparam name="TModelType"></typeparam>
    public class TmphIdentityArray<TValueType, TModelType> : TmphLoadCache<TValueType, TModelType, int>, TmphILoadIdentityCache<TValueType, TModelType>
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 根据自增值计算数组长度
        /// </summary>
        /// <param name="identity">自增值</param>
        /// <returns>数组长度</returns>
        private static int getArrayLength(int identity)
        {
            uint value = (uint)cacheCapacity;
            while (value <= (uint)identity) value <<= 1;
            if (value == 0x80000000U)
            {
                if (identity == int.MaxValue) TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                return int.MaxValue;
            }
            return (int)value;
        }

        /// <summary>
        /// 数据集合
        /// </summary>
        private TmphArrayValue[] array;

        /// <summary>
        /// 对象数量
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 枚举数据集合
        /// </summary>
        public override IEnumerable<TValueType> Values
        {
            get
            {
                foreach (TmphArrayValue value in array)
                {
                    if (value.Value != null) yield return value.Value;
                }
            }
        }

        /// <summary>
        /// 获取数组
        /// </summary>
        /// <returns></returns>
        public override TmphSubArray<TValueType> GetSubArray()
        {
            TmphSubArray<TValueType> TmphSubArray = new TmphSubArray<TValueType>(Count);
            foreach (TmphArrayValue value in array)
            {
                if (value.Value != null) TmphSubArray.Add(value.Value);
            }
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
        /// 当前自增值
        /// </summary>
        private int currentIdentity;

        /// <summary>
        /// 获取下一个自增值
        /// </summary>
        /// <returns>自增值</returns>
        public int NextIdentity()
        {
            if (((isLoaded ^ 1) | isDisposed) == 0) return Interlocked.Increment(ref currentIdentity);
            TmphLog.Error.Throw(TmphLog.TmphExceptionType.ErrorOperation);
            return int.MinValue;
        }

        /// <summary>
        /// 是否存在关键字
        /// </summary>
        /// <param name="identity">关键字</param>
        /// <returns>是否存在对象</returns>
        public bool ContainsKey(int identity)
        {
            return (uint)identity < (uint)array.Length && array[identity].Value != null;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="identity">关键字</param>
        /// <returns>数据,失败返回null</returns>
        public override TValueType Get(int identity)
        {
            return (uint)identity < (uint)array.Length ? array[identity].Value : null;
        }

        /// <summary>
        /// 新建对象数组
        /// </summary>
        /// <param name="identity">自增值</param>
        private void newArray(int identity)
        {
            int length = array.Length << 1;
            TmphArrayValue[] newArray = new TmphArrayValue[length > identity ? length : getArrayLength(identity)];
            Array.Copy(array, 0, newArray, 0, array.Length);
            array = newArray;
        }

        /// <summary>
        /// 加载日志添加对象
        /// </summary>
        /// <param name="value">添加的对象</param>
        /// <param name="identity">自增值</param>
        /// <param name="logSize">日志字节长度</param>
        public void LoadInsert(TValueType value, int identity, int logSize)
        {
            if (array == null) array = new TmphArrayValue[getArrayLength(identity)];
            else if (identity >= array.Length) newArray(identity);
            array[identity].Set(value, logSize);
            ++Count;
            if (identity > currentIdentity) currentIdentity = identity;
        }

        /// <summary>
        /// 加载日志修改对象
        /// </summary>
        /// <param name="value">修改的对象</param>
        /// <param name="identity">自增值</param>
        /// <param name="memberMap">修改对象成员位图</param>
        public void LoadUpdate(TValueType value, int identity, TmphMemberMap memberMap)
        {
            if ((uint)identity < (uint)array.Length)
            {
                TValueType TCacheValue = array[identity].Value;
                if (TCacheValue != null) Laurent.Lee.CLB.Emit.TmphMemberCopyer<TModelType>.Copy(TCacheValue, value, memberMap);
            }
        }

        /// <summary>
        /// 加载日志删除对象
        /// </summary>
        /// <param name="identity">自增值</param>
        /// <returns>日志字节长度</returns>
        public int LoadDelete(int identity)
        {
            if ((uint)identity < (uint)array.Length)
            {
                int logSize;
                TValueType value = array[identity].Clear(out logSize);
                if (value != null)
                {
                    --Count;
                    return logSize;
                }
            }
            return 0;
        }

        /// <summary>
        /// 日志数据加载完成
        /// </summary>
        /// <param name="isLoaded">是否加载成功</param>
        public void Loaded(bool isLoaded)
        {
            try
            {
                if (isLoaded)
                {
                    if (array == null) array = new TmphArrayValue[cacheCapacity];
                }
                else Dispose();
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
                Dispose();
            }
            finally { onLoaded(); }
        }

        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="value">添加的对象</param>
        /// <param name="identity">自增值</param>
        /// <param name="logSize">日志字节长度</param>
        /// <param name="isCopy">是否浅复制缓存对象值,否则返回缓存对象</param>
        /// <returns>添加的对象</returns>
        protected override TValueType insert(TValueType value, int identity, int logSize, bool isCopy)
        {
            if (identity >= array.Length) newArray(identity);
            if (isCopy) value = Laurent.Lee.CLB.Emit.TmphMemberCopyer<TValueType>.MemberwiseClone(value);
            array[identity].Set(value, logSize);
            ++Count;
            return value;
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="identity">自增值</param>
        /// <param name="logSize">日志字节长度</param>
        /// <returns>被删除对象,失败返回null</returns>
        protected override TValueType delete(int identity, out int logSize)
        {
            if ((uint)identity < (uint)array.Length)
            {
                TValueType value = array[identity].Clear(out logSize);
                if (value != null)
                {
                    --Count;
                    return value;
                }
            }
            logSize = 0;
            return null;
        }
    }

    /// <summary>
    /// 自增数组缓存
    /// </summary>
    /// <typeparam name="TModelType"></typeparam>
    public sealed class TmphIdentityArray<TModelType> : TmphIdentityArray<TModelType, TModelType>
        where TModelType : class
    {
    }
}