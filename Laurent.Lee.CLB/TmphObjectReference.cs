using System;

namespace Laurent.Lee.CLB
{
    public struct TmphObjectReference : IEquatable<TmphObjectReference>
    {
        public object Value;

        public bool Equals(TmphObjectReference other)
        {
            var type = Value.GetType();
            if (Value.GetType() == other.Value.GetType())
            {
                return type == typeof(string)
                    ? (string)Value == (string)other.Value
                    : ReferenceEquals(Value, other.Value);
            }
            return false;
        }

        public override int GetHashCode()
        {
            try
            {
                return Value.GetHashCode();
            }
            catch
            {
                return Value.GetType().GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            return Equals((TmphObjectReference)obj);
        }
    }
}