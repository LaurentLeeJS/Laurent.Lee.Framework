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
using System.Reflection;

namespace Laurent.Lee.CLB.Code.CSharp
{
    /// <summary>
    ///     数据库表格模型配置
    /// </summary>
    public abstract class TmphDataModel : TmphMemberFilter.TmphPublicInstanceField
    {
        /// <summary>
        ///     是否有序比较
        /// </summary>
        public bool IsComparable;

        ///// <summary>
        ///// 是否检查添加数据的自增值
        ///// </summary>
        //public bool IsCheckAppendIdentity = true;
        /// <summary>
        ///     获取数据库表格模型类型
        /// </summary>
        /// <param name="type">数据库表格绑定类型</param>
        /// <returns>数据库表格模型类型,失败返回null</returns>
        internal static Type GetModelType<TModelType>(Type type) where TModelType : TmphDataModel
        {
            do
            {
                var sqlModel = TmphTypeAttribute.GetAttribute<TModelType>(type, false, true);
                if (sqlModel != null) return type;
                if ((type = type.BaseType) == null) return null;
            } while (true);
        }

        /// <summary>
        ///     获取字段成员集合
        /// </summary>
        /// <param name="type"></param>
        /// <param name="model"></param>
        /// <returns>字段成员集合</returns>
        public static TmphSubArray<TmphMemberInfo> GetPrimaryKeys<TModeType>(Type type, TModeType model)
            where TModeType : TmphDataModel
        {
            var fields =
                (TmphFieldIndex[])
                    typeof(TmphMemberIndexGroup<>).MakeGenericType(type)
                        .GetMethod("GetFields", BindingFlags.Static | BindingFlags.Public)
                        .Invoke(null, new object[] { model.MemberFilter });
            var values = new TmphSubArray<TmphMemberInfo>();
            foreach (var field in fields)
            {
                type = field.Member.FieldType;
                if (!type.IsPointer && (!type.IsArray || type.GetArrayRank() == 1) && !field.IsIgnore)
                {
                    var attribute = field.GetSetupAttribute<TmphDataMember>(true, true);
                    if (attribute != null && attribute.PrimaryKeyIndex != 0)
                        values.Add(new TmphMemberInfo(type, field.Member.Name, attribute.PrimaryKeyIndex));
                }
            }
            return values.Sort((left, right) =>
            {
                var value = left.MemberIndex - right.MemberIndex;
                return value == 0 ? string.Compare(left.MemberName, right.MemberName, StringComparison.Ordinal) : value;
            });
        }
    }
}