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

namespace Laurent.Lee.CLB.Sql.MsSql
{
    /// <summary>
    ///     SQL数据类型相关操作
    /// </summary>
    internal static class TmphSqlDbType
    {
        /// <summary>
        ///     默认值集合
        /// </summary>
        private static readonly string[] defaultValues;

        /// <summary>
        ///     SqlServer2000 syscolumns.xtype值与SqlDbType值映射关系集合
        /// </summary>
        private static readonly SqlDbType[] SqlType;

        static TmphSqlDbType()
        {
            #region 默认值集合

            defaultValues = new string[TmphEnum.GetMaxValue<SqlDbType>(0) + 1];
            defaultValues[(int)SqlDbType.BigInt] = "0";
            defaultValues[(int)SqlDbType.Bit] = "0";
            defaultValues[(int)SqlDbType.Char] = "''";
            defaultValues[(int)SqlDbType.DateTime] = "getdate()";
            defaultValues[(int)SqlDbType.Decimal] = "0";
            defaultValues[(int)SqlDbType.Float] = "0";
            defaultValues[(int)SqlDbType.Int] = "0";
            defaultValues[(int)SqlDbType.Money] = "0";
            defaultValues[(int)SqlDbType.NChar] = "''";
            defaultValues[(int)SqlDbType.NText] = "''";
            defaultValues[(int)SqlDbType.NVarChar] = "''";
            defaultValues[(int)SqlDbType.Real] = "0";
            defaultValues[(int)SqlDbType.SmallDateTime] = "getdate()";
            defaultValues[(int)SqlDbType.SmallInt] = "0";
            defaultValues[(int)SqlDbType.SmallMoney] = "0";
            defaultValues[(int)SqlDbType.Text] = "''";
            defaultValues[(int)SqlDbType.TinyInt] = "0";
            defaultValues[(int)SqlDbType.VarChar] = "''";

            #endregion 默认值集合

            #region SqlServer syscolumns.xusertype值与sqlDbType值映射关系集合

            SqlType = new SqlDbType[256];
            SqlType[34] = SqlDbType.Image;
            SqlType[35] = SqlDbType.Text;
            SqlType[36] = SqlDbType.UniqueIdentifier;
            SqlType[40] = SqlDbType.Date;
            SqlType[41] = SqlDbType.Time;
            SqlType[42] = SqlDbType.DateTime2;
            SqlType[43] = SqlDbType.DateTimeOffset;
            SqlType[48] = SqlDbType.TinyInt;
            SqlType[52] = SqlDbType.SmallInt;
            SqlType[56] = SqlDbType.Int;
            SqlType[58] = SqlDbType.SmallDateTime;
            SqlType[59] = SqlDbType.Real;
            SqlType[60] = SqlDbType.Money;
            SqlType[61] = SqlDbType.DateTime;
            SqlType[62] = SqlDbType.Float;
            //SqlType[98] = SqlDbType.sql_variant;
            SqlType[99] = SqlDbType.NText;
            SqlType[104] = SqlDbType.Bit;
            SqlType[106] = SqlDbType.Decimal;
            //SqlType[108] = SqlDbType.numeric;
            SqlType[122] = SqlDbType.SmallMoney;
            SqlType[127] = SqlDbType.BigInt;
            //SqlType[128] = SqlDbType.hierarchyid;
            //SqlType[129] = SqlDbType.geometry;
            //SqlType[130] = SqlDbType.geography;
            SqlType[165] = SqlDbType.VarBinary;
            SqlType[167] = SqlDbType.VarChar;
            SqlType[173] = SqlDbType.Binary;
            SqlType[175] = SqlDbType.Char;
            SqlType[189] = SqlDbType.Timestamp;
            SqlType[231] = SqlDbType.NVarChar;
            SqlType[239] = SqlDbType.NChar;
            SqlType[241] = SqlDbType.Xml;
            //SqlType[256] = SqlDbType.sysname;

            #endregion SqlServer syscolumns.xusertype值与sqlDbType值映射关系集合
        }

        /// <summary>
        ///     获取默认值
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns>默认值</returns>
        public static string getDefaultValue(this SqlDbType type)
        {
            return defaultValues.get((int)type, null);
        }

        /// <summary>
        ///     根据SqlServer syscolumns.xtype值与SqlDbType值
        /// </summary>
        /// <param name="dataType">SqlServer syscolumns.xtype值</param>
        /// <returns>SqlDbType值</returns>
        public static SqlDbType GetType(short dataType)
        {
            return SqlType.get(dataType, (SqlDbType)(-1));
        }
    }
}