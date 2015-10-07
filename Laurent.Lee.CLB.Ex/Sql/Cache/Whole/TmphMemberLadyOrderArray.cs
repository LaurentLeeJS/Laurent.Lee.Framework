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
using System.Threading;

namespace Laurent.Lee.CLB.Sql.Cache.Whole
{
    /// <summary>
    /// 延时排序缓存数组
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    public sealed class TmphMemberLadyOrderArray<TValueType>
        where TValueType : class
    {
        /// <summary>
        /// 分组数据
        /// </summary>
        public TmphSubArray<TValueType> Array;

        /// <summary>
        /// 当前数据集合(不排序)
        /// </summary>
        public TmphSubArray<TValueType> CurrentArray
        {
            get
            {
                int count = Array.Count;
                return TmphSubArray<TValueType>.Unsafe(Array.Array, 0, count == 0 ? Array.StartIndex : count);
            }
        }

        /// <summary>
        /// 数据数量
        /// </summary>
        public int Count
        {
            get { return Array.StartIndex + Array.Count; }
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="value"></param>
        public void Insert(TValueType value)
        {
            TmphSubArray<TValueType> array = TmphSubArray<TValueType>.Unsafe(Array.Array, 0, Array.StartIndex + Array.Count);
            array.Add(value);
            Array.UnsafeSet(array.Array, array.Count, 0);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="value"></param>
        public void Update(TValueType value)
        {
            int count = Array.StartIndex + Array.Count, index = System.Array.IndexOf(Array.Array, value, 0, count);
            if (index == -1) TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", true, true);
            else Array.UnsafeSet(count, 0);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value"></param>
        public void Delete(TValueType value)
        {
            int index = System.Array.IndexOf(Array.Array, value, 0, Array.StartIndex + Array.Count);
            if (index == -1) TmphLog.Error.Add(typeof(TValueType).FullName + " 缓存同步错误", true, true);
            else if (Array.Count == 0)
            {
                TValueType[] array = Array.Array;
                Array.UnsafeSet(Array.StartIndex - 1, 0);
                array[index] = array[Array.StartIndex];
                array[Array.StartIndex] = null;
            }
            else Array.RemoveAt(index);
        }

        /// <summary>
        /// 获取排序数组
        /// </summary>
        /// <param name="sqlLock"></param>
        /// <param name="sorter"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public TValueType At(object sqlLock, Func<TmphSubArray<TValueType>, TmphSubArray<TValueType>> sorter, int index)
        {
            Monitor.Enter(sqlLock);
            try
            {
                if (Array.StartIndex != 0)
                {
                    if ((uint)index < Array.StartIndex)
                    {
                        Array = sorter(TmphSubArray<TValueType>.Unsafe(Array.Array, 0, Array.StartIndex));
                        return Array.Array[index];
                    }
                }
                else if ((uint)index < Array.Count) return Array.Array[index];
            }
            finally { Monitor.Exit(sqlLock); }
            return null;
        }

        /// <summary>
        /// 获取排序数组
        /// </summary>
        /// <param name="sqlLock"></param>
        /// <param name="sorter"></param>
        /// <returns></returns>
        public TValueType[] GetArray(object sqlLock, Func<TmphSubArray<TValueType>, TmphSubArray<TValueType>> sorter)
        {
            Monitor.Enter(sqlLock);
            try
            {
                if (Array.StartIndex != 0) Array = sorter(TmphSubArray<TValueType>.Unsafe(Array.Array, 0, Array.StartIndex));
                return Array.GetArray();
            }
            finally { Monitor.Exit(sqlLock); }
        }

        /// <summary>
        /// 获取分页数据集合
        /// </summary>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentPage">分页页号</param>
        /// <param name="count">数据总数</param>
        /// <returns>分页数据集合</returns>
        public TValueType[] GetPage(object sqlLock, Func<TmphSubArray<TValueType>, TmphSubArray<TValueType>> sorter, int pageSize, int currentPage, out int count)
        {
            Monitor.Enter(sqlLock);
            try
            {
                if (Array.StartIndex != 0) Array = sorter(TmphSubArray<TValueType>.Unsafe(Array.Array, 0, Array.StartIndex));
                count = Array.Count;
                return Array.Page(pageSize, currentPage).GetArray();
            }
            finally { Monitor.Exit(sqlLock); }
        }

        /// <summary>
        /// 获取分页数据集合
        /// </summary>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentPage">分页页号</param>
        /// <param name="count">数据总数</param>
        /// <returns>分页数据集合</returns>
        public TValueType[] GetPageDesc(object sqlLock, Func<TmphSubArray<TValueType>, TmphSubArray<TValueType>> sorter, int pageSize, int currentPage, out int count)
        {
            Monitor.Enter(sqlLock);
            try
            {
                if (Array.StartIndex != 0) Array = sorter(TmphSubArray<TValueType>.Unsafe(Array.Array, 0, Array.StartIndex));
                count = Array.Count;
                return Array.GetPageDesc(pageSize, currentPage);
            }
            finally { Monitor.Exit(sqlLock); }
        }
    }
}