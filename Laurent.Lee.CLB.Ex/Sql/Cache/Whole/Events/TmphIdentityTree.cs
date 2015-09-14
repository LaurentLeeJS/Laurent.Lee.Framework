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

namespace Laurent.Lee.CLB.Sql.Cache.Whole.Events
{
    /// <summary>
    /// 自增ID整表排序树缓存(反射模式)
    /// </summary>
    /// <typeparam name="TValueType">表格绑定类型</typeparam>
    /// <typeparam name="TModelType">表格模型类型</typeparam>
    public sealed unsafe class TmphIdentityTree<TValueType, TModelType> : TmphIdentityCache<TValueType, TModelType>, IDisposable
        where TValueType : class, TModelType
        where TModelType : class
    {
        /// <summary>
        /// 排序树节点数量集合
        /// </summary>
        private int* counts;

        /// <summary>
        /// 排序树容器数量
        /// </summary>
        private int size;

        /// <summary>
        /// 自增ID整表数组缓存
        /// </summary>
        /// <param name="sqlTool">SQL操作工具</param>
        /// <param name="group">数据分组</param>
        /// <param name="baseIdentity">基础ID</param>
        public TmphIdentityTree(Laurent.Lee.CLB.Emit.TmphSqlTable.TmphSqlTool<TValueType, TModelType> sqlTool, int group = 0, int baseIdentity = 0)
            : base(sqlTool, group, baseIdentity, true)
        {
            sqlTool.OnInsertedLock += onInserted;
            sqlTool.OnDeletedLock += onDelete;

            resetLock();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            TmphUnmanaged.Free(counts);
            counts = null;
            size = 0;
        }

        /// <summary>
        /// 重新加载数据
        /// </summary>
        protected override void reset()
        {
            TValueType[] values = SqlTool.Where(null, memberMap).getArray();
            int maxIdentity = values.maxKey(value => getIdentity(value), 0);
            if (memberGroup == 0) SqlTool.Identity64 = maxIdentity + baseIdentity;
            int length = maxIdentity >= 1024 ? 1 << ((uint)maxIdentity).bits() : 1024;
            TValueType[] newValues = new TValueType[length];
            int* newCounts = TmphUnmanaged.Get(length * sizeof(int), true).Int;
            try
            {
                foreach (TValueType value in values)
                {
                    int identity = getIdentity(value);
                    newValues[identity] = value;
                    newCounts[identity] = 1;
                }
                for (int step = 2; step != length; step <<= 1)
                {
                    for (int index = step, countStep = step >> 1; index != length; index += step)
                    {
                        newCounts[index] += newCounts[index - countStep];
                    }
                }
                TmphUnmanaged.Free(counts);
                this.values = newValues;
                counts = newCounts;
                size = length;
                Count = values.Length;
                newCounts = null;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, true);
            }
            finally
            {
                TmphUnmanaged.Free(newCounts);
            }
        }

        /// <summary>
        /// 增加数据
        /// </summary>
        /// <param name="value">新增的数据</param>
        private void onInserted(TValueType value)
        {
            int identity = getIdentity(value);
            if (identity >= values.Length)
            {
                int newLength = int.MaxValue, oldLength = values.Length;
                if ((identity & 0x40000000) == 0 && oldLength != 0x40000000)
                {
                    for (newLength = oldLength << 1; newLength <= identity; newLength <<= 1) ;
                }
                TValueType[] newValues = new TValueType[newLength];
                values.CopyTo(newValues, 0);
                int* newCounts = TmphUnmanaged.Get(newLength * sizeof(int), true).Int;
                try
                {
                    Unsafe.TmphMemory.Copy(counts, newCounts, newLength * sizeof(int));
                    TmphUnmanaged.Free(counts);
                    values = newValues;
                    counts = newCounts;
                    size = newLength;
                    newCounts = null;

                    int index = oldLength, count = counts[--index];
                    for (int step = 1; (index -= step) != 0; step <<= 1) count += counts[index];
                    counts[oldLength] = count;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, true);
                }
                finally
                {
                    TmphUnmanaged.Free(newCounts);
                }
            }
            TValueType newValue = Laurent.Lee.CLB.Emit.TmphConstructor<TValueType>.New();
            Laurent.Lee.CLB.Emit.TmphMemberCopyer<TModelType>.Copy(newValue, value, memberMap);
            values[identity] = newValue;
            for (uint index = (uint)identity, countStep = 1, length = (uint)size; index <= length; countStep <<= 1)
            {
                ++counts[index];
                while ((index & countStep) == 0) countStep <<= 1;
                index += countStep;
            }
            ++Count;
            callOnInserted(newValue);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="value">被删除的数据</param>
        private void onDelete(TValueType value)
        {
            int identity = getIdentity(value);
            TValueType TCacheValue = values[identity];
            --Count;
            for (uint index = (uint)identity, countStep = 1, length = (uint)size; index <= length; countStep <<= 1)
            {
                --counts[index];
                while ((index & countStep) == 0) countStep <<= 1;
                index += countStep;
            }
            values[identity] = null;
            callOnDeleted(TCacheValue);
        }

        /// <summary>
        /// 获取记录起始位置
        /// </summary>
        /// <param name="skipCount">跳过记录数</param>
        /// <returns>起始位置</returns>
        private int getIndex(int skipCount)
        {
            if (skipCount == 0) return 1;
            int index = values.Length != int.MaxValue ? values.Length >> 1 : 0x40000000, step = index;
            while (counts[index] != skipCount)
            {
                step >>= 1;
                if (counts[index] < skipCount)
                {
                    skipCount -= counts[index];
                    index += step;
                }
                else index -= step;
            }
            return index + 1;
        }

        /// <summary>
        /// 获取分页记录集合
        /// </summary>
        /// <param name="pageSize">分页长度</param>
        /// <param name="currentPage">分页页号</param>
        /// <param name="count">记录总数</param>
        /// <returns>分页记录集合</returns>
        public TValueType[] GetPageDesc(int pageSize, int currentPage, out int count)
        {
            Monitor.Enter(SqlTool.Lock);
            try
            {
                TmphArray.TmphPage page = new TmphArray.TmphPage(count = Count, pageSize, currentPage);
                TValueType[] values = new TValueType[page.CurrentPageSize];
                for (int writeIndex = values.Length, index = getIndex(Count - page.SkipCount - page.CurrentPageSize); writeIndex != 0; values[--writeIndex] = this.values[index++])
                {
                    while (this.values[index] == null) ++index;
                }
                return values;
            }
            finally { Monitor.Exit(SqlTool.Lock); }
        }
    }
}