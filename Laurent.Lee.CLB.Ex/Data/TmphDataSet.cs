using Laurent.Lee.CLB.Emit;
using System;
using System.Data;

namespace Laurent.Lee.CLB.Data
{
    /// <summary>
    ///     DataSet包装
    /// </summary>
    [TmphDataSerialize(IsMemberMap = false)]
    public sealed class TmphDataSet
    {
        /// <summary>
        ///     数据源
        /// </summary>
        private TmphDataSource _data;

        /// <summary>
        ///     DataSet名称
        /// </summary>
        private string _name;

        /// <summary>
        ///     数据表格集合
        /// </summary>
        private TmphDataTable[] _tables;

        /// <summary>
        ///     DataSet包装
        /// </summary>
        /// <param name="set"></param>
        private void FromPackage(DataSet set)
        {
            if (set.Tables.Count != 0)
            {
                using (var TmphBuilder = new TmphDataWriter())
                {
                    _tables = set.Tables.toGeneric<DataTable>().getArray(table => TmphDataTable.From(table, TmphBuilder));
                    _data = TmphBuilder.Get();
                }
            }
            _name = set.DataSetName;
        }

        /// <summary>
        ///     DataSet包装
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public static TmphDataSet From(DataSet set)
        {
            if (set == null) return null;
            var value = new TmphDataSet();
            value.FromPackage(set);
            return value;
        }

        /// <summary>
        ///     序列化
        /// </summary>
        /// <param name="set"></param>
        /// <returns>序列化数据</returns>
        public static byte[] Serialize(DataSet set)
        {
            return TmphDataSerializer.Serialize(From(set));
        }

        /// <summary>
        ///     DataSet拆包
        /// </summary>
        /// <param name="set"></param>
        private unsafe void set(DataSet set)
        {
            if (_tables.length() != 0)
            {
                fixed (byte* dataFixed = _data.Data)
                {
                    var TmphBuilder = new TmphDataReader(dataFixed, _data.Strings, _data.Bytes);
                    foreach (var table in _tables) set.Tables.Add(table.Get(TmphBuilder));
                }
            }
        }

        /// <summary>
        ///     DataSet拆包
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DataSet Get(TmphDataSet value)
        {
            if (value == null) return null;
            var set = new DataSet(value._name);
            try
            {
                value.set(set);
                return set;
            }
            catch (Exception error)
            {
                set.Dispose();
                TmphLog.Error.Add(error, null, false);
            }
            return null;
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <param name="data">序列化数据</param>
        /// <returns></returns>
        public static DataSet DeSerialize(byte[] data)
        {
            var value = TmphDataDeSerializer.DeSerialize<TmphDataSet>(data);
            return value != null ? Get(value) : null;
        }
    }
}