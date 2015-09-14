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

using Laurent.Lee.CLB.Emit;
using System;
using System.Collections.Generic;
using System.Data;

namespace Laurent.Lee.CLB.Data
{
    /// <summary>
    ///     数据表格
    /// </summary>
    [TmphDataSerialize(IsMemberMap = false)]
    public sealed class TmphDataTable
    {
        /// <summary>
        ///     类型集合
        /// </summary>
        private static readonly Type[] Types;

        /// <summary>
        ///     类型索引集合
        /// </summary>
        private static readonly Dictionary<Type, byte> TypeIndexs;

        /// <summary>
        ///     列名集合
        /// </summary>
        private string[] _columnNames;

        /// <summary>
        ///     列类型集合
        /// </summary>
        private byte[] _columnTypes;

        /// <summary>
        ///     数据源
        /// </summary>
        private TmphDataSource _data;

        /// <summary>
        ///     空数据位图
        /// </summary>
        private byte[] _dbNull;

        /// <summary>
        ///     表格名称
        /// </summary>
        private string _name;

        /// <summary>
        ///     数据行数
        /// </summary>
        private int _rowCount;

        static TmphDataTable()
        {
            var index = 0;
            Types = new Type[30];
            TypeIndexs = TmphDictionary.CreateOnly<Type, byte>();
            TypeIndexs.Add(Types[index] = typeof(int), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(int?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(string), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(DateTime), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(DateTime?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(double), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(double?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(float), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(float?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(decimal), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(decimal?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(Guid), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(Guid?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(bool), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(bool?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(byte), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(byte?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(byte[]), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(sbyte), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(sbyte?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(short), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(short?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(ushort), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(ushort?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(uint), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(uint?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(long), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(long?), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(ulong), (byte)index++);
            TypeIndexs.Add(Types[index] = typeof(ulong?), (byte)index);
        }

        /// <summary>
        ///     DataTable包装
        /// </summary>
        /// <param name="table"></param>
        /// <param name="TmphBuilder">数据流包装器</param>
        private unsafe void FromPackage(DataTable table, TmphDataWriter TmphBuilder)
        {
            var index = 0;
            _columnNames = new string[table.Columns.Count];
            fixed (byte* columnFixed = _columnTypes = new byte[_columnNames.Length])
            {
                var columnIndex = columnFixed;
                foreach (DataColumn column in table.Columns)
                {
                    if (!TypeIndexs.TryGetValue(column.DataType, out *columnIndex)) *columnIndex = 255;
                    ++columnIndex;
                    _columnNames[index++] = column.ColumnName;
                }
                fixed (byte* nullFixed = _dbNull = new byte[(_columnNames.Length * _rowCount + 7) >> 3])
                {
                    var nullMap = new TmphFixedMap(nullFixed);
                    index = 0;
                    foreach (DataRow row in table.Rows)
                    {
                        columnIndex = columnFixed;
                        foreach (var value in row.ItemArray)
                        {
                            if (value == DBNull.Value) nullMap.Set(index);
                            else TmphBuilder.Append(value, *columnIndex);
                            ++index;
                            ++columnIndex;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     DataTable包装
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static TmphDataTable From(DataTable table)
        {
            if (table == null) return null;
            var value = new TmphDataTable();
            if ((value._rowCount = table.Rows.Count) != 0)
            {
                using (var TmphBuilder = new TmphDataWriter())
                {
                    value.FromPackage(table, TmphBuilder);
                    value._data = TmphBuilder.Get();
                }
            }
            return value;
        }

        /// <summary>
        ///     序列化
        /// </summary>
        /// <param name="table"></param>
        /// <returns>序列化数据</returns>
        public static byte[] Serialize(DataTable table)
        {
            return TmphDataSerializer.Serialize(From(table));
        }

        /// <summary>
        ///     DataTable包装
        /// </summary>
        /// <param name="table"></param>
        /// <param name="TmphBuilder">数据流包装器</param>
        /// <returns></returns>
        public static TmphDataTable From(DataTable table, TmphDataWriter TmphBuilder)
        {
            var value = new TmphDataTable();
            if ((value._rowCount = table.Rows.Count) != 0) value.FromPackage(table, TmphBuilder);
            value._name = table.TableName;
            return value;
        }

        /// <summary>
        ///     DataTable拆包
        /// </summary>
        /// <param name="TmphBuilder">数据对象拆包器</param>
        /// <returns></returns>
        public DataTable Get(TmphDataReader TmphBuilder)
        {
            var table = new DataTable(_name);
            if (_rowCount != 0)
            {
                try
                {
                    Get(table, TmphBuilder);
                }
                catch (Exception error)
                {
                    table.Dispose();
                    table = null;
                    TmphLog.Error.Add(error, null, false);
                }
            }
            return table;
        }

        /// <summary>
        ///     DataTable拆包
        /// </summary>
        /// <param name="table"></param>
        /// <param name="TmphBuilder">数据对象拆包器</param>
        private unsafe void Get(DataTable table, TmphDataReader TmphBuilder)
        {
            var index = 0;
            var columns = new DataColumn[_columnNames.Length];
            fixed (byte* columnFixed = _columnTypes)
            {
                var columnIndex = columnFixed;
                foreach (var columnName in _columnNames)
                {
                    columns[index++] = new DataColumn(columnName,
                        *columnIndex < Types.Length ? Types[*columnIndex] : typeof(object));
                    ++columnIndex;
                }
                table.Columns.AddRange(columns);
                fixed (byte* nullFixed = _dbNull)
                {
                    var nullMap = new TmphFixedMap(nullFixed);
                    for (index = 0; _rowCount != 0; --_rowCount)
                    {
                        var values = new object[_columnNames.Length];
                        columnIndex = columnFixed;
                        for (var valueIndex = 0; valueIndex != _columnNames.Length; ++valueIndex)
                        {
                            values[valueIndex] = nullMap.Get(index++) ? DBNull.Value : TmphBuilder.Get(*columnIndex);
                            ++columnIndex;
                        }
                        var row = table.NewRow();
                        row.ItemArray = values;
                        table.Rows.Add(row);
                    }
                }
            }
        }

        /// <summary>
        ///     DataTable拆包
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static unsafe DataTable Get(TmphDataTable value)
        {
            if (value == null) return null;
            var table = new DataTable(value._name);
            try
            {
                if (value._rowCount != 0)
                {
                    fixed (byte* dataFixed = value._data.Data)
                    {
                        var TmphBuilder = new TmphDataReader(dataFixed, value._data.Strings, value._data.Bytes);
                        value.Get(table, TmphBuilder);
                    }
                }
                return table;
            }
            catch (Exception error)
            {
                table.Dispose();
                TmphLog.Error.Add(error, null, false);
            }
            return null;
        }

        /// <summary>
        ///     反序列化
        /// </summary>
        /// <param name="data">序列化数据</param>
        /// <returns></returns>
        public static DataTable DeSerialize(byte[] data)
        {
            var value = TmphDataDeSerializer.DeSerialize<TmphDataTable>(data);
            return value != null ? Get(value) : null;
        }
    }
}