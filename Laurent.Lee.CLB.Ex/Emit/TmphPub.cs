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

using Laurent.Lee.CLB.Code;
using Laurent.Lee.CLB.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using System.Web;

namespace Laurent.Lee.CLB.Emit
{
    /// <summary>
    /// 公共类型
    /// </summary>
    public static partial class TmphPubExtension
    {
        /// <summary>
        /// 添加表单函数信息
        /// </summary>
        public static readonly MethodInfo NameValueCollectionAddMethod = typeof(NameValueCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(string), typeof(string) }, null);

        /// <summary>
        /// URL编码函数信息
        /// </summary>
        public static readonly MethodInfo UrlEncodeMethod = typeof(HttpUtility).GetMethod("UrlEncode", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string), typeof(Encoding) }, null);

        /// <summary>
        /// 引用比较函数信息
        /// </summary>
        public static readonly MethodInfo ReferenceEqualsMethod = typeof(Object).GetMethod("ReferenceEquals", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(object), typeof(object) }, null);

        /// <summary>
        /// 字符串转换调用函数信息集合
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo> toStringMethods = TmphDictionary.CreateOnly<Type, MethodInfo>();

        /// <summary>
        /// 字符串转换调用函数信息访问锁
        /// </summary>
        private static int toStringMethodLock;

        /// <summary>
        /// 获取字符串转换委托调用函数信息
        /// </summary>
        /// <param name="type">数值类型</param>
        /// <returns>字符串转换委托调用函数信息</returns>
        public static MethodInfo GetToStringMethod(Type type)
        {
            MethodInfo method;
            TmphInterlocked.NoCheckCompareSetSleep0(ref toStringMethodLock);
            if (toStringMethods.TryGetValue(type, out method))
            {
                toStringMethodLock = 0;
                return method;
            }
            try
            {
                method = type.GetMethod("ToString", BindingFlags.Instance | BindingFlags.Public, null, TmphNullValue<Type>.Array, null);
                toStringMethods.Add(type, method);
            }
            finally { toStringMethodLock = 0; }
            return method;
        }

        /// <summary>
        /// 数值转换调用函数信息集合
        /// </summary>
        private static TmphInterlocked.TmphDictionary<Type, MethodInfo> numberToStringMethods = new TmphInterlocked.TmphDictionary<Type, MethodInfo>(TmphDictionary.CreateOnly<Type, MethodInfo>());

        /// <summary>
        /// 获取数值转换委托调用函数信息
        /// </summary>
        /// <param name="type">数值类型</param>
        /// <returns>数值转换委托调用函数信息</returns>
        public static MethodInfo GetNumberToStringMethod(Type type)
        {
            MethodInfo method;
            if (numberToStringMethods.TryGetValue(type, out method)) return method;
            method = typeof(CLB.TmphNumber).GetMethod("toString", BindingFlags.Static | BindingFlags.Public, null, new Type[] { type }, null);
            numberToStringMethods.Set(type, method);
            return method;
        }

        /// <summary>
        /// 内存字符流写入字符串方法信息
        /// </summary>
        public static readonly MethodInfo CharStreamWriteNotNullMethod = typeof(TmphCharStream).GetMethod("WriteNotNull", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);

        /// <summary>
        /// 判断成员位图是否匹配成员索引
        /// </summary>
        public static readonly MethodInfo MemberMapIsMemberMethod = (MethodInfo)typeof(TmphPub).GetField("MemberMapIsMemberMethod", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

        /// <summary>
        /// 获取可空类型是否为空判断函数信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns>可空类型是否为空判断函数信息</returns>
        public static readonly Func<Type, MethodInfo> GetNullableHasValue = (Func<Type, MethodInfo>)Delegate.CreateDelegate(typeof(Func<Type, MethodInfo>), typeof(TmphPub).GetMethod("GetNullableHasValue", BindingFlags.Static | BindingFlags.NonPublic));

        /// <summary>
        /// 获取可空类型获取数据函数信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns>可空类型获取数据函数信息</returns>
        public static readonly Func<Type, MethodInfo> GetNullableValue = (Func<Type, MethodInfo>)Delegate.CreateDelegate(typeof(Func<Type, MethodInfo>), typeof(TmphPub).GetMethod("GetNullableValue", BindingFlags.Static | BindingFlags.NonPublic));

        /// <summary>
        /// 获取数值转换委托调用函数信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns>数值转换委托调用函数信息</returns>
        public static readonly Func<Type, MethodInfo> GetNumberToCharStreamMethod = (Func<Type, MethodInfo>)Delegate.CreateDelegate(typeof(Func<Type, MethodInfo>), typeof(TmphPub).GetMethod("GetNumberToCharStreamMethod", BindingFlags.Static | BindingFlags.NonPublic));

        /// <summary>
        /// 获取字段成员集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="memberAttribute"></typeparam>
        /// <param name="memberFilter"></param>
        /// <param name="isAllMember"></param>
        /// <returns>字段成员集合</returns>
        public static TmphSubArray<FieldInfo> GetFields<TValueType, memberAttribute>(TmphMemberFilters memberFilter, bool isAllMember)
            where memberAttribute : TmphIgnoreMember
        {
            return (TmphSubArray<FieldInfo>)getFieldsMethod.MakeGenericMethod(typeof(TValueType), typeof(memberAttribute)).Invoke(null, new object[] { memberFilter, isAllMember });
        }

        /// <summary>
        /// 获取字段成员集合函数信息
        /// </summary>
        private static readonly MethodInfo getFieldsMethod = typeof(TmphPub).GetMethod("GetFields", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// 获取字段成员集合
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <typeparam name="memberAttribute"></typeparam>
        /// <param name="memberFilter"></param>
        /// <param name="isAllMember"></param>
        /// <returns>字段成员集合</returns>
        public static TmphKeyValue<FieldInfo, int>[] GetFieldIndexs<TValueType>(TmphMemberFilters memberFilter)
        {
            return (TmphKeyValue<FieldInfo, int>[])getFieldIndexsMethod.MakeGenericMethod(typeof(TValueType)).Invoke(null, new object[] { memberFilter });
        }

        /// <summary>
        /// 获取字段成员集合函数信息
        /// </summary>
        private static readonly MethodInfo getFieldIndexsMethod = typeof(TmphPub).GetMethod("GetFieldIndexs", BindingFlags.Static | BindingFlags.NonPublic);
    }
}