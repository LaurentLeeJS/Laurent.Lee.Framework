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

using Laurent.Lee.CLB.Config;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     HTTP请求表单
    /// </summary>
    public sealed class TmphRequestForm
    {
        /// <summary>
        ///     文件集合
        /// </summary>
        public readonly TmphList<TmphValue> Files = new TmphList<TmphValue>(sizeof(int));

        /// <summary>
        ///     表单数据缓冲区
        /// </summary>
        private byte[] TmphBuffer;

        /// <summary>
        ///     表单数据集合
        /// </summary>
        internal TmphList<TmphValue> FormValues = new TmphList<TmphValue>(sizeof(int));

        /// <summary>
        ///     HTTP操作标识
        /// </summary>
        internal long Identity;

        /// <summary>
        ///     JSON字符串
        /// </summary>
        internal string Json;

        /// <summary>
        ///     清除表单数据
        /// </summary>
        internal void Clear()
        {
            clear(FormValues);
            clear(Files);
            Json = null;
        }

        /// <summary>
        ///     解析表单数据
        /// </summary>
        /// <param name="TmphBuffer">表单数据缓冲区</param>
        /// <param name="length">表单数据长度</param>
        /// <returns>是否成功</returns>
        internal unsafe bool Parse(byte[] TmphBuffer, int length)
        {
            fixed (byte* bufferFixed = TmphBuffer)
            {
                byte* current = bufferFixed - 1, end = bufferFixed + length;
                *end = (byte)'&';
                try
                {
                    do
                    {
                        var nameIndex = (int)(++current - bufferFixed);
                        while (*current != '&' && *current != '=') ++current;
                        var nameLength = (int)(current - bufferFixed) - nameIndex;
                        if (*current == '=')
                        {
                            var valueIndex = (int)(++current - bufferFixed);
                            while (*current != '&') ++current;
                            if (nameLength == 1 && TmphBuffer[nameIndex] == TmphWeb.Default.QueryJsonName)
                            {
                                Parse(TmphBuffer, valueIndex, (int)(current - bufferFixed) - valueIndex, Encoding.UTF8);
                                //showjim编码问题
                            }
                            else
                                FormValues.Add(new TmphValue
                                {
                                    Name = TmphSubArray<byte>.Unsafe(TmphBuffer, nameIndex, nameLength),
                                    Value =
                                        TmphSubArray<byte>.Unsafe(TmphBuffer, valueIndex,
                                            (int)(current - bufferFixed) - valueIndex)
                                });
                        }
                        else if (nameLength != 0)
                        {
                            FormValues.Add(new TmphValue
                            {
                                Name = TmphSubArray<byte>.Unsafe(TmphBuffer, nameIndex, nameLength),
                                Value = TmphSubArray<byte>.Unsafe(TmphBuffer, 0, 0)
                            });
                        }
                    } while (current != end);
                    this.TmphBuffer = TmphBuffer;
                    return true;
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }
            return false;
        }

        /// <summary>
        ///     JSON数据转换成JSON字符串
        /// </summary>
        /// <param name="TmphBuffer">数据缓冲区</param>
        /// <param name="startIndex">数据其实位置</param>
        /// <param name="length">JSON数据长度</param>
        /// <param name="encoding">编码</param>
        /// <returns>是否成功</returns>
        internal unsafe bool Parse(byte[] TmphBuffer, int startIndex, int length, Encoding encoding)
        {
            if (length == 0)
            {
                Json = string.Empty;
                return true;
            }
            try
            {
                if (encoding == Encoding.Unicode)
                {
                    Json = TmphString.FastAllocateString(length >> 1);
                    fixed (char* jsonFixed = Json)
                    fixed (byte* bufferFixed = TmphBuffer)
                    {
                        Unsafe.TmphMemory.Copy(bufferFixed + startIndex, jsonFixed, length);
                    }
                }
                else if (encoding == Encoding.ASCII)
                {
                    fixed (byte* bufferFixed = TmphBuffer) Json = TmphString.DeSerialize(bufferFixed + startIndex, -length);
                }
                else Json = encoding.GetString(TmphBuffer, startIndex, length);
                this.TmphBuffer = TmphBuffer;
                return true;
            }
            catch (Exception error)
            {
                TmphLog.Error.Add(error, null, false);
            }
            return false;
        }

        /// <summary>
        ///     设置文件表单数据
        /// </summary>
        internal void SetFileValue()
        {
            var formArray = FormValues.array;
            for (int index = 0, count = FormValues.Count; index != count; ++index) formArray[index].SetFileValue();
        }

        /// <summary>
        ///     清除表单数据
        /// </summary>
        /// <param name="values">表单数据集合</param>
        private static void clear(TmphList<TmphValue> values)
        {
            var count = values.Count;
            if (count != 0)
            {
                var formArray = values.array;
                for (var index = 0; index != count; ++index) formArray[index].Clear();
                values.Empty();
            }
        }

        /// <summary>
        ///     HTTP请求表单加载接口
        /// </summary>
        public interface TmphILoadForm
        {
            /// <summary>
            ///     表单回调处理
            /// </summary>
            /// <param name="form">HTTP请求表单</param>
            void OnGetForm(TmphRequestForm form);

            /// <summary>
            ///     根据HTTP请求表单值获取内存流最大字节数
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>内存流最大字节数</returns>
            int MaxMemoryStreamSize(TmphValue value);

            /// <summary>
            ///     根据HTTP请求表单值获取保存文件全称
            /// </summary>
            /// <param name="value">HTTP请求表单值</param>
            /// <returns>文件全称</returns>
            string GetSaveFileName(TmphValue value);
        }

        /// <summary>
        ///     HTTP请求表单值
        /// </summary>
        public struct TmphValue
        {
            /// <summary>
            ///     默认允许上传的图片扩展名集合
            /// </summary>
            private static readonly string[] defaultImageExtensions = { "jpeg", "gif", "bmp", "png" };

            /// <summary>
            ///     默认允许上传的图片扩展名集合
            /// </summary>
            public static readonly TmphStateSearcher.TmphAscii<string> DefaultImageExtensions =
                new TmphStateSearcher.TmphAscii<string>(defaultImageExtensions, defaultImageExtensions);

            /// <summary>
            ///     默认允许上传的图片类型集合
            /// </summary>
            private static readonly Dictionary<ImageFormat, string> defaultImageTypes;

            /// <summary>
            ///     客户端文件名称
            /// </summary>
            public TmphSubArray<byte> FileName;

            /// <summary>
            ///     名称
            /// </summary>
            public TmphSubArray<byte> Name;

            /// <summary>
            ///     服务器端文件名称
            /// </summary>
            public string SaveFileName;

            /// <summary>
            ///     表单值
            /// </summary>
            public TmphSubArray<byte> Value;

            static TmphValue()
            {
                defaultImageTypes = TmphDictionary.CreateOnly<ImageFormat, string>();
                defaultImageTypes.Add(ImageFormat.Jpeg, ImageFormat.Jpeg.ToString().ToLower());
                defaultImageTypes.Add(ImageFormat.Gif, ImageFormat.Gif.ToString().ToLower());
                defaultImageTypes.Add(ImageFormat.Bmp, ImageFormat.Bmp.ToString().ToLower());
                defaultImageTypes.Add(ImageFormat.MemoryBmp, ImageFormat.Bmp.ToString().ToLower());
                defaultImageTypes.Add(ImageFormat.Png, ImageFormat.Png.ToString().ToLower());
            }

            /// <summary>
            ///     设置文件表单数据
            /// </summary>
            internal void SetFileValue()
            {
                if (SaveFileName != null)
                {
                    try
                    {
                        if (File.Exists(SaveFileName))
                        {
                            var data = File.ReadAllBytes(SaveFileName);
                            Value.UnsafeSet(data, 0, data.Length);
                            File.Delete(SaveFileName);
                        }
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                }
                SaveFileName = null;
            }

            /// <summary>
            ///     保存到目标文件
            /// </summary>
            /// <param name="fileName">目标文件名称</param>
            public void SaveFile(string fileName)
            {
                if (SaveFileName == null)
                {
                    using (
                        var fileStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.Read,
                            1, FileOptions.WriteThrough))
                    {
                        fileStream.Write(Value.Array, Value.StartIndex, Value.Count);
                    }
                }
                else
                {
                    File.Move(SaveFileName, fileName);
                    SaveFileName = null;
                }
            }

            /// <summary>
            ///     保存图片
            /// </summary>
            /// <param name="fileName">不包含扩展名的图片文件名称</param>
            /// <param name="imageTypes">默认允许上传的图片类型集合</param>
            /// <returns>包含扩展名的图片文件名称,失败返回null</returns>
            public string SaveImage(string fileName, Dictionary<ImageFormat, string> imageTypes = null)
            {
                try
                {
                    string type = null;
                    if (SaveFileName == null)
                    {
                        if (Value.Count != 0)
                        {
                            using (var stream = new MemoryStream(Value.Array, Value.StartIndex, Value.Count))
                            using (var image = Image.FromStream(stream))
                            {
                                (imageTypes ?? defaultImageTypes).TryGetValue(image.RawFormat, out type);
                            }
                        }
                    }
                    else
                    {
                        using (var image = Image.FromFile(SaveFileName))
                            (imageTypes ?? defaultImageTypes).TryGetValue(image.RawFormat, out type);
                    }
                    if (type != null)
                    {
                        fileName += "." + type;
                        if (Value.Count == 0)
                        {
                            File.Move(SaveFileName, fileName);
                            SaveFileName = null;
                        }
                        else
                        {
                            using (
                                var fileStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write,
                                    FileShare.None))
                            {
                                fileStream.Write(Value.Array, Value.StartIndex, Value.Count);
                            }
                        }
                        return fileName;
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Default.Add(error, null, false);
                }
                return null;
            }

            /// <summary>
            ///     清表单数据
            /// </summary>
            internal void Clear()
            {
                Name.Null();
                Value.Null();
                FileName.Null();
                if (SaveFileName != null)
                {
                    try
                    {
                        if (File.Exists(SaveFileName)) File.Delete(SaveFileName);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                    }
                    SaveFileName = null;
                }
            }

            /// <summary>
            ///     清除数据
            /// </summary>
            internal void Null()
            {
                Name.Null();
                Value.Null();
                FileName.Null();
                SaveFileName = null;
            }
        }
    }
}