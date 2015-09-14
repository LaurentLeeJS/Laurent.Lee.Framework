using Laurent.Lee.CLB.Emit;

namespace Laurent.Lee.CLB.Data
{
    /// <summary>
    ///     数据源
    /// </summary>
    [TmphDataSerialize(IsMemberMap = false)]
    public sealed class TmphDataSource
    {
        /// <summary>
        ///     字节数组集合
        /// </summary>
        public byte[][] Bytes;

        /// <summary>
        ///     数据流
        /// </summary>
        public byte[] Data;

        /// <summary>
        ///     字符串集合
        /// </summary>
        public string[] Strings;
    }
}