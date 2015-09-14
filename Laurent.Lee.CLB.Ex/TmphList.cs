using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    public static partial class TmphListExtension<TValueType>
    {
        public static readonly Func<List<TValueType>, TValueType[]> GetItems = Emit.TmphPub.GetField<List<TValueType>, TValueType[]>("_items");
    }

    public static partial class TmphListExtension
    {
        public static TmphSubArray<TValueType> toSubArray<TValueType>(this List<TValueType> list)
        {
            if (list == null) return default(TmphSubArray<TValueType>);
            return TmphSubArray<TValueType>.Unsafe(TmphListExtension<TValueType>.GetItems(list), 0, list.Count);
        }
    }
}