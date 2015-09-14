using System;

namespace Laurent.Lee.CLB.Data
{
    /// <summary>
    ///     数据流包装器
    /// </summary>
    public class TmphDataWriter : IDisposable
    {
        /// <summary>
        ///     字节数组集合
        /// </summary>
        private readonly TmphList<byte[]> _bytes = new TmphList<byte[]>();

        /// <summary>
        ///     字符串集合
        /// </summary>
        private readonly TmphList<string> _strings = new TmphList<string>();

        /// <summary>
        ///     数据流
        /// </summary>
        private TmphUnmanagedStream _stream = new TmphUnmanagedStream();

        /// <summary>
        ///     释放数据流
        /// </summary>
        public void Dispose()
        {
            TmphPub.Dispose(ref _stream);
        }

        /// <summary>
        ///     添加数据
        /// </summary>
        /// <param name="value">数据对象</param>
        /// <param name="typeIndex">数据类型</param>
        public void Append(object value, byte typeIndex)
        {
            switch (typeIndex)
            {
                case 0:
                    _stream.Write((int)value);
                    break;

                case 1:
                    _stream.Write((int)(int?)value);
                    break;

                case 2:
                    _strings.Add((string)value);
                    break;

                case 3:
                    _stream.Write(((DateTime)value).Ticks);
                    break;

                case 4:
                    _stream.Write(((DateTime)(DateTime?)value).Ticks);
                    break;

                case 5:
                    _stream.Write((double)value);
                    break;

                case 6:
                    _stream.Write((double)(double?)value);
                    break;

                case 7:
                    _stream.Write((float)value);
                    break;

                case 8:
                    _stream.Write((float)(float?)value);
                    break;

                case 9:
                    _stream.Write((decimal)value);
                    break;

                case 10:
                    _stream.Write((decimal)(decimal?)value);
                    break;

                case 11:
                    _stream.Write((Guid)value);
                    break;

                case 12:
                    _stream.Write((Guid)(Guid?)value);
                    break;

                case 13:
                    _stream.Write((bool)value ? (byte)1 : (byte)0);
                    break;

                case 14:
                    _stream.Write((bool)(bool?)value ? (byte)1 : (byte)0);
                    break;

                case 15:
                    _stream.Write((byte)value);
                    break;

                case 16:
                    _stream.Write((byte)(byte?)value);
                    break;

                case 17:
                    _bytes.Add((byte[])value);
                    break;

                case 18:
                    _stream.Write((sbyte)value);
                    break;

                case 19:
                    _stream.Write((sbyte)(sbyte?)value);
                    break;

                case 20:
                    _stream.Write((short)value);
                    break;

                case 21:
                    _stream.Write((short)(short?)value);
                    break;

                case 22:
                    _stream.Write((ushort)value);
                    break;

                case 23:
                    _stream.Write((ushort)(ushort?)value);
                    break;

                case 24:
                    _stream.Write((uint)value);
                    break;

                case 25:
                    _stream.Write((uint)(uint?)value);
                    break;

                case 26:
                    _stream.Write((long)value);
                    break;

                case 27:
                    _stream.Write((long)(long?)value);
                    break;

                case 28:
                    _stream.Write((ulong)value);
                    break;

                case 29:
                    _stream.Write((ulong)(ulong?)value);
                    break;
            }
        }

        /// <summary>
        ///     获取数据源
        /// </summary>
        /// <returns></returns>
        public TmphDataSource Get()
        {
            var value = new TmphDataSource();
            value.Data = _stream.GetArray();
            value.Strings = _strings.ToArray();
            value.Bytes = _bytes.ToArray();
            return value;
        }
    }
}