using System.Collections.Generic;

namespace Laurent.Lee.CLB
{
    public struct TmphKeyValue<TKeyType, TValueType>
    {
        public TKeyType Key;
        public TValueType Value;

        public TmphKeyValue(TKeyType key, TValueType value)
        {
            Key = key;
            Value = value;
        }

        public void Set(TKeyType key, TValueType value)
        {
            Key = key;
            Value = value;
        }

        public void Null()
        {
            Key = default(TKeyType);
            Value = default(TValueType);
        }

        public static implicit operator TmphKeyValue<TKeyType, TValueType>(KeyValuePair<TKeyType, TValueType> value)
        {
            return new TmphKeyValue<TKeyType, TValueType>(value.Key, value.Value);
        }

        public static implicit operator KeyValuePair<TKeyType, TValueType>(TmphKeyValue<TKeyType, TValueType> value)
        {
            return new KeyValuePair<TKeyType, TValueType>(value.Key, value.Value);
        }
    }
}