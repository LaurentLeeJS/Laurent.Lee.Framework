/*
 *  Copyright 2015 Tony Lee

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using System.Data;

namespace Laurent.Lee.CLB.Sql.Excel
{
    /// <summary>
    ///     SQL数据类型相关操作
    /// </summary>
    internal static class TmphSqlDbType
    {
        /// <summary>
        ///     数据类型集合
        /// </summary>
        private static readonly string[] sqlTypeNames;

        static TmphSqlDbType()
        {
            #region 数据类型集合

            sqlTypeNames = new string[TmphEnum.GetMaxValue<SqlDbType>(-1) + 1];
            sqlTypeNames[(int)SqlDbType.BigInt] = "INTEGER";
            //SqlTypeNames[(int)SqlDbType.Binary] = typeof(byte[]);
            sqlTypeNames[(int)SqlDbType.Bit] = "BOOLEAN";
            sqlTypeNames[(int)SqlDbType.Char] = "VARCHAR";
            sqlTypeNames[(int)SqlDbType.DateTime] = "DATETIME";
            sqlTypeNames[(int)SqlDbType.Decimal] = "DECIMAL";
            sqlTypeNames[(int)SqlDbType.Float] = "DECIMAL";
            //SqlTypeNames[(int)SqlDbType.Image] = typeof(byte[]);
            sqlTypeNames[(int)SqlDbType.Int] = "INTEGER";
            sqlTypeNames[(int)SqlDbType.Money] = "DECIMAL";
            sqlTypeNames[(int)SqlDbType.NChar] = "VARCHAR";
            sqlTypeNames[(int)SqlDbType.NText] = "VARCHAR";
            sqlTypeNames[(int)SqlDbType.NVarChar] = "VARCHAR";
            sqlTypeNames[(int)SqlDbType.Real] = "DECIMAL";
            //SqlTypeNames[(int)SqlDbType.UniqueIdentifier] = typeof(Guid);
            sqlTypeNames[(int)SqlDbType.SmallDateTime] = "DATETIME";
            sqlTypeNames[(int)SqlDbType.SmallInt] = "INTEGER";
            sqlTypeNames[(int)SqlDbType.SmallMoney] = "DECIMAL";
            sqlTypeNames[(int)SqlDbType.Text] = "VARCHAR";
            //SqlTypeNames[(int)SqlDbType.Timestamp] = typeof(byte[]);
            sqlTypeNames[(int)SqlDbType.TinyInt] = "INTEGER";
            //SqlTypeNames[(int)SqlDbType.VarBinary] = typeof(byte[]);
            sqlTypeNames[(int)SqlDbType.VarChar] = "VARCHAR";
            //SqlTypeNames[(int)SqlDbType.Variant] = typeof(object);

            #endregion 数据类型集合
        }

        /// <summary>
        ///     获取数据类型名称
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns>数据类型名称</returns>
        public static string getSqlTypeName(this SqlDbType type)
        {
            return sqlTypeNames.get((int)type, null);
        }
    }
}