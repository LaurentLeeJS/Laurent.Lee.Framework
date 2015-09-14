namespace Laurent.Lee.CLB
{
    public static class TmphEnmuParser<TEnumType>
    {
        public static readonly TmphStateSearcher.TmphAscii<TEnumType> Parser;

        static TmphEnmuParser()
        {
            if (typeof(TEnumType).IsEnum)
            {
                TEnumType[] values = (TEnumType[])System.Enum.GetValues(typeof(TEnumType));
                Parser = new TmphStateSearcher.TmphAscii<TEnumType>(values.getArray(value => value.ToString()), values);
            }
        }
    }
}