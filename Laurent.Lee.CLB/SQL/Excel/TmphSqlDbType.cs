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