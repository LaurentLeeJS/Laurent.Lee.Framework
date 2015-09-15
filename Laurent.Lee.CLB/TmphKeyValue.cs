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