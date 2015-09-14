using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    public class TmphTimeoutDictionary<TKeyType, TValueType> : IDisposable
        where TKeyType : IEquatable<TKeyType>
    {
        private struct TmphValue
        {
            public DateTime Timeout;
            public TValueType Value;
        }

        private long timeoutTicks;
        private Dictionary<TKeyType, TmphValue> values = TmphDictionary<TKeyType>.Create<TmphValue>();

        public event Action<TKeyType, TValueType> OnRemovedLock;

        private int dictionaryLock;
        private int isRefresh;
        private Action refresh;
        private TmphList<TmphKeyValue<TKeyType, TValueType>> refreshValues = new TmphList<TmphKeyValue<TKeyType, TValueType>>();

        public TValueType this[TKeyType key]
        {
            get
            {
                TValueType value = default(TValueType);
                if (TryGetValue(key, ref value)) return value;
                TmphLog.Error.Throw(TmphLog.TmphExceptionType.IndexOutOfRange);
                return value;
            }
            set
            {
                int isRefresh = 1;
                DateTime timeout = TmphDate.NowSecond.AddTicks(timeoutTicks);
                TmphValue timeValue;
                TmphInterlocked.NoCheckCompareSetSleep0(ref dictionaryLock);
                try
                {
                    if (values.Count == 0)
                    {
                        isRefresh = this.isRefresh;
                        this.isRefresh = 1;
                        values[key] = new TmphValue { Timeout = timeout, Value = value };
                    }
                    else
                    {
                        if (values.TryGetValue(key, out timeValue)) remove(key, timeValue.Value);
                        values.Add(key, new TmphValue { Timeout = timeout, Value = value });
                    }
                }
                finally { dictionaryLock = 0; }
                if (isRefresh == 0) TmphTimerTask.Default.Add(refresh, timeout, null);
            }
        }

        public TmphTimeoutDictionary(int timeoutSeconds)
        {
            timeoutTicks = new TimeSpan(0, 0, timeoutSeconds <= 0 ? 1 : timeoutSeconds).Ticks;
            refresh = refreshTimeout;
        }

        public void Dispose()
        {
            refresh = null;
            TmphInterlocked.CompareSetSleep1(ref dictionaryLock);
            try
            {
                if (OnRemovedLock == null) this.values.Clear();
                else
                {
                    foreach (KeyValuePair<TKeyType, TmphValue> values in this.values.getArray()) remove(values.Key, values.Value.Value);
                }
            }
            finally { dictionaryLock = 0; }
        }

        private void refreshTimeout()
        {
            DateTime time = TmphDate.NowSecond;
            int count = 0, isRefresh = 1;
            TmphInterlocked.NoCheckCompareSetSleep0(ref dictionaryLock);
            try
            {
                if (refreshValues.Count < this.values.Count)
                {
                    refreshValues.Empty();
                    refreshValues.AddLength(this.values.Count);
                }
                TmphKeyValue<TKeyType, TValueType>[] refreshValueArray = refreshValues.Unsafer.Array;
                foreach (KeyValuePair<TKeyType, TmphValue> values in this.values)
                {
                    if (time >= values.Value.Timeout) refreshValueArray[count++].Set(values.Key, values.Value.Value);
                }
                if (count != 0)
                {
                    foreach (TmphKeyValue<TKeyType, TValueType> value in refreshValueArray)
                    {
                        remove(value.Key, value.Value);
                        if (--count == 0) break;
                    }
                }
                if (this.values.Count == 0) isRefresh = this.isRefresh = 0;
            }
            finally { dictionaryLock = 0; }
            if (isRefresh != 0) TmphTimerTask.Default.Add(refresh, TmphDate.NowSecond.AddTicks(timeoutTicks), null);
        }

        public bool ContainsKey(TKeyType key)
        {
            return values.ContainsKey(key);
        }

        public bool TryGetValue(TKeyType key, ref TValueType value)
        {
            DateTime now = TmphDate.NowSecond;
            TmphValue timeValue;
            TmphInterlocked.CompareSetSleep1(ref dictionaryLock);
            if (values.TryGetValue(key, out timeValue))
            {
                if (timeValue.Timeout > TmphDate.NowSecond)
                {
                    dictionaryLock = 0;
                    value = timeValue.Value;
                    return true;
                }
                else
                {
                    try
                    {
                        remove(key, timeValue.Value);
                    }
                    finally { dictionaryLock = 0; }
                }
            }
            else dictionaryLock = 0;
            return false;
        }

        public TValueType Get(TKeyType key, TValueType nullValue)
        {
            TValueType value = default(TValueType);
            return TryGetValue(key, ref value) ? value : default(TValueType);
        }

        public bool Remove(TKeyType key)
        {
            TValueType value = default(TValueType);
            return Remove(key, ref value);
        }

        public bool Remove(TKeyType key, ref TValueType value)
        {
            bool isRemove = false;
            TmphValue timeValue;
            TmphInterlocked.CompareSetSleep1(ref dictionaryLock);
            try
            {
                if (values.TryGetValue(key, out timeValue))
                {
                    isRemove = true;
                    remove(key, value = timeValue.Value);
                }
            }
            finally { dictionaryLock = 0; }
            return isRemove;
        }

        private void remove(TKeyType key, TValueType value)
        {
            values.Remove(key);
            if (OnRemovedLock != null) OnRemovedLock(key, value);
        }

        public void RefreshTimeout(TKeyType key)
        {
            DateTime timeout = TmphDate.NowSecond.AddTicks(timeoutTicks);
            TmphValue value;
            TmphInterlocked.CompareSetSleep1(ref dictionaryLock);
            try
            {
                if (values.TryGetValue(key, out value))
                {
                    values[key] = new TmphValue { Timeout = timeout, Value = value.Value };
                }
            }
            finally { dictionaryLock = 0; }
        }
    }
}