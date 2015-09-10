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

using System;

namespace Laurent.Lee.CLB.Sql.Excel
{
    /// <summary>
    ///     Excel连接信息
    /// </summary>
    public sealed class TmphConnection
    {
        /// <summary>
        ///     混合数据处理方式
        /// </summary>
        public enum EIntermixed : byte
        {
            /// <summary>
            ///     输出模式，此情况下只能用作写入Excel
            /// </summary>
            Write = 0,

            /// <summary>
            ///     输入模式，此情况下只能用作读取Excel，并且始终将Excel数据作为文本类型读取
            /// </summary>
            Read = 1,

            /// <summary>
            ///     连接模式，此情况下既可用作写入、也可用作读取
            /// </summary>
            WriteAndRead = 2
        }

        /// <summary>
        ///     Excel接口类型
        /// </summary>
        public enum EProviderType
        {
            None,

            /// <summary>
            ///     只能操作Excel2007之前的.xls文件
            /// </summary>
            [THProvider(Name = "Microsoft.Jet.OleDb.4.0", Excel = "Excel 8.0")]
            Jet4,

            /// <summary>
            /// </summary>
            [THProvider(Name = "Microsoft.ACE.OLEDB.12.0", Excel = "Excel 12.0")]
            Ace12
        }

        /// <summary>
        ///     数据源
        /// </summary>
        public string DataSource;

        /// <summary>
        ///     混合数据处理方式
        /// </summary>
        public EIntermixed Intermixed = EIntermixed.WriteAndRead;

        /// <summary>
        ///     第一行是否列名
        /// </summary>
        public bool IsTitleColumn = true;

        /// <summary>
        ///     密码
        /// </summary>
        public string Password;

        /// <summary>
        ///     数据接口属性
        /// </summary>
        public EProviderType Provider = EProviderType.Ace12;

        /// <summary>
        ///     获取Excel客户端
        /// </summary>
        /// <returns>Excel客户端</returns>
        public unsafe TmphClient GetClient()
        {
            var provider = TmphEnum<EProviderType, THProvider>.Array(Provider);
            var TmphBuffer = Sql.TmphClient.SqlBuffers.Get();
            try
            {
                using (var connectionStream = new TmphCharStream(TmphBuffer.Char, Sql.TmphClient.SqlBufferSize))
                {
                    connectionStream.WriteNotNull("Provider=");
                    connectionStream.Write(provider.Name);
                    connectionStream.WriteNotNull(";Data Source=");
                    connectionStream.Write(DataSource);
                    if (Password != null)
                    {
                        connectionStream.WriteNotNull(";Database Password=");
                        connectionStream.WriteNotNull(Password);
                    }
                    connectionStream.WriteNotNull(";Extended Properties='");
                    connectionStream.Write(provider.Excel);
                    connectionStream.WriteNotNull(IsTitleColumn ? ";HDR=YES;IMEX=" : ";HDR=NO;IMEX=");
                    connectionStream.WriteNotNull(((byte)Intermixed).toString());
                    connectionStream.Write('\'');
                    return
                        (TmphClient)
                            new Sql.TmphConnection { Type = TmphType.Excel, Connection = connectionStream.ToString() }.Client;
                }
            }
            finally
            {
                Sql.TmphClient.SqlBuffers.Push(ref TmphBuffer);
            }
        }

        /// <summary>
        ///     数据接口属性
        /// </summary>
        public sealed class THProvider : Attribute
        {
            /// <summary>
            ///     Excel版本号
            /// </summary>
            public string Excel;

            /// <summary>
            ///     连接名称
            /// </summary>
            public string Name;
        }
    }
}