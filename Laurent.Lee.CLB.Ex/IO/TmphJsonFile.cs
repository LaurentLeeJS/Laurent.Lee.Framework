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

using Laurent.Lee.CLB.Threading;
using System;
using System.IO;
using System.Text;

namespace Laurent.Lee.CLB.IO
{
    /// <summary>
    /// JSON对象缓存文件
    /// </summary>
    /// <typeparam name="TValueType">对象类型</typeparam>
    public sealed class TmphJsonFile<TValueType> where TValueType : class
    {
        /// <summary>
        /// 缓存文件名称
        /// </summary>
        private string fileName;

        /// <summary>
        /// 数据对象
        /// </summary>
        public TValueType Value { get; private set; }

        /// <summary>
        /// 编码
        /// </summary>
        private Encoding encoding;

        /// <summary>
        /// 文件访问锁
        /// </summary>
        private int fileLock;

        /// <summary>
        /// JSON对象缓存文件
        /// </summary>
        /// <param name="fileName">缓存文件名称</param>
        /// <param name="value">数据对象</param>
        public TmphJsonFile(string fileName, TValueType value = null, Encoding encoding = null)
        {
            if (fileName == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            this.fileName = fileName;
            this.encoding = encoding ?? Laurent.Lee.CLB.Config.TmphAppSetting.Encoding;
            bool isFile = false, isJson = false;
            try
            {
                if (File.Exists(fileName))
                {
                    isFile = true;
                    if (Laurent.Lee.CLB.Emit.TmphJsonParser.Parse(File.ReadAllText(fileName, this.encoding), ref value))
                    {
                        Value = value;
                        isJson = true;
                    }
                }
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, fileName, false);
            }
            if (isFile && !isJson) TmphFile.MoveBak(fileName);
        }

        /// <summary>
        /// 修改对象
        /// </summary>
        /// <param name="value">新的对象值</param>
        /// <returns>是否修改成功</returns>
        public bool Rework(TValueType value)
        {
            if (value == null) TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
            string json = Laurent.Lee.CLB.Emit.TmphJsonSerializer.ToJson(value);
            TmphInterlocked.CompareSetSleep1(ref fileLock);
            try
            {
                if (write(json))
                {
                    Value = value;
                    return true;
                }
            }
            finally { fileLock = 0; }
            return false;
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Write()
        {
            return Rework(Value);
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>是否成功</returns>
        private bool write(string json)
        {
            try
            {
                if (File.Exists(fileName)) TmphFile.MoveBak(fileName);
                File.WriteAllText(fileName, json, this.encoding);
                return true;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, fileName, false);
            }
            return false;
        }
    }
}