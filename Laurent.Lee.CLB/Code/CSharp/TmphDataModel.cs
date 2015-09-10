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