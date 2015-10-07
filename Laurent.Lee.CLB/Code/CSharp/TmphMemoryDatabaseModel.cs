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
using System.Reflection;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     内存数据库表格模型配置
    /// </summary>
    public class TmphMemoryDatabaseModel : TmphDataModel
    {
        /// <summary>
        ///     默认空属性
        /// </summary>
        internal static readonly TmphMemoryDatabaseModel Default = new TmphMemoryDatabaseModel();

        /// <summary>
        ///     字段信息
        /// </summary>
        internal struct TmphFieldInfo
        {
            /// <summary>
            ///     数据库成员信息
            /// </summary>
            public TmphDataMember DataMember;

            /// <summary>
            ///     字段信息
            /// </summary>
            public FieldInfo Field;

            /// <summary>
            ///     成员位图索引
            /// </summary>
            public int MemberMapIndex;

            /// <summary>
            ///     字段信息
            /// </summary>
            /// <param name="field"></param>
            /// <param name="attribute"></param>
            public TmphFieldInfo(TmphFieldIndex field, TmphDataMember attribute)
            {
                Field = field.Member;
                DataMember = attribute;
                MemberMapIndex = field.MemberIndex;
            }
        }
    }
}