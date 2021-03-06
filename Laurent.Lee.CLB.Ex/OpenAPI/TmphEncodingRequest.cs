﻿/*
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

using System;
using System.Collections.Specialized;
using System.Text;

namespace Laurent.Lee.CLB.OpenAPI
{
    /// <summary>
    /// 编码绑定请求
    /// </summary>
    public class TmphEncodingRequest
    {
        /// <summary>
        /// web请求
        /// </summary>
        private readonly TmphRequest TmphRequest;

        /// <summary>
        /// 请求编码
        /// </summary>
        private readonly Encoding encoding;

        /// <summary>
        /// 编码绑定请求
        /// </summary>
        /// <param name="TmphRequest">web请求</param>
        /// <param name="encoding">请求编码</param>
        public TmphEncodingRequest(TmphRequest TmphRequest, Encoding encoding)
        {
            this.TmphRequest = TmphRequest;
            this.encoding = encoding;
        }

        /// <summary>
        /// API请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="form">POST表单内容</param>
        /// <returns>返回内容,失败为null</returns>
        public string Request(string url, NameValueCollection form = null)
        {
            return TmphRequest.Request(url, encoding, form);
        }

        /// <summary>
        /// API请求json数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <typeparam name="TFormType">表单数据类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="formValue">POST表单</param>
        /// <returns>数据对象,失败放回null</returns>
        public TValueType RequestJson<TValueType, TFormType>(string url, TFormType formValue)
            where TValueType : class, TmphIValue
        {
            string json;
            NameValueCollection form = TmphTypePool<NameValueCollection>.Pop() ?? new NameValueCollection();
            try
            {
                Laurent.Lee.CLB.Emit.TmphFormGetter<TFormType>.Get(formValue, form);
                json = Request(url, form);
            }
            finally
            {
                form.Clear();
                TmphTypePool<NameValueCollection>.Push(ref form);
            }
            return parseJson<TValueType>(json);
        }

        /// <summary>
        /// API请求json数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="form">POST表单</param>
        /// <returns>数据对象,失败放回null</returns>
        public TValueType RequestJson<TValueType>(string url, NameValueCollection form)
            where TValueType : class, TmphIValue
        {
            return parseJson<TValueType>(Request(url, form));
        }

        /// <summary>
        /// API请求json数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <returns>数据对象,失败放回null</returns>
        public TValueType RequestJson<TValueType>(string url)
            where TValueType : class, TmphIValue
        {
            return parseJson<TValueType>(Request(url));
        }

        /// <summary>
        /// API请求json数据
        /// </summary>
        /// <typeparam name="TValueType">数据类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns>数据对象,失败放回null</returns>
        private static TValueType parseJson<TValueType>(string json)
            where TValueType : class, TmphIValue
        {
            if (json != null)
            {
                TValueType value = null;
                bool isError = false, isJson = false;
                try
                {
                    if (Laurent.Lee.CLB.Emit.TmphJsonParser.Parse(json, ref value)) isJson = true;
                }
                catch (Exception error)
                {
                    isError = true;
                    TmphLog.Error.Add(error, json, false);
                }
                if (isJson && value.IsValue) return value;
                if (!isError) TmphLog.Default.Add(value.Message + @"
" + json, false, false);
            }
            return default(TValueType);
        }
    }
}