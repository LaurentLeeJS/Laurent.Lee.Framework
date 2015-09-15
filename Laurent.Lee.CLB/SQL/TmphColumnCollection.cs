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

namespace Laurent.Lee.CLB.Sql
{
    /// <summary>
    ///     列集合
    /// </summary>
    internal sealed class THColumnCollection
    {
        /// <summary>
        ///     列集合类型
        /// </summary>
        public enum TmphType
        {
            /// <summary>
            ///     普通集合
            /// </summary>
            None,

            /// <summary>
            ///     主键
            /// </summary>
            PrimaryKey,

            /// <summary>
            ///     普通索引
            /// </summary>
            Index,

            /// <summary>
            ///     唯一索引
            /// </summary>
            UniqueIndex
        }

        /// <summary>
        ///     列集合
        /// </summary>
        public TmphColumn[] Columns;

        /// <summary>
        ///     列集合名称
        /// </summary>
        public string Name;

        /// <summary>
        ///     列集合类型
        /// </summary>
        public TmphType Type;
    }
}