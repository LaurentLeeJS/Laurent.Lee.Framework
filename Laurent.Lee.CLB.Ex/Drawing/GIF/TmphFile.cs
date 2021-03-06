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

using Laurent.Lee.CLB.Algorithm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Laurent.Lee.CLB.Drawing.GIF
{
    /// <summary>
    /// GIF文件
    /// </summary>
    public class TmphFile
    {
        /// <summary>
        /// LZW压缩编码查询表缓冲区
        /// </summary>
#if MONO
        private static readonly unmanagedPool lzwEncodeTableBuffer = unmanagedPool.GetPool(4096 * 256 * 2);
#else
        private static readonly TmphUnmanagedPool lzwEncodeTableBuffer = TmphUnmanagedPoolProxy.GetPool(4096 * 256 * 2);
#endif

        /// <summary>
        /// 24位色彩
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct TmphColor : IEquatable<TmphColor>
        {
            /// <summary>
            /// 整数值
            /// </summary>
            [FieldOffset(0)]
            public int Value;

            /// <summary>
            /// 红色
            /// </summary>
            [FieldOffset(0)]
            public byte Red;

            /// <summary>
            /// 绿色
            /// </summary>
            [FieldOffset(1)]
            public byte Green;

            /// <summary>
            /// 蓝色
            /// </summary>
            [FieldOffset(2)]
            public byte Blue;

            /// <summary>
            /// HASH值
            /// </summary>
            /// <returns>HASH值</returns>
            public override int GetHashCode()
            {
                return Value;
            }

            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="other"></param>
            /// <returns>是否相等</returns>
            public bool Equals(TmphColor other)
            {
                return Value == other.Value;
            }

            /// <summary>
            /// 判断是否相等
            /// </summary>
            /// <param name="obj"></param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                return Equals((TmphColor)obj);
            }
        }

        /// <summary>
        /// 图象块
        /// </summary>
        public sealed class TmphImage
        {
            /// <summary>
            /// X方向偏移量
            /// </summary>
            public short LeftOffset { get; private set; }

            /// <summary>
            /// Y方向偏移量
            /// </summary>
            public short TopOffset { get; private set; }

            /// <summary>
            /// 图象宽度
            /// </summary>
            public short Width { get; private set; }

            /// <summary>
            /// 图象高度
            /// </summary>
            public short Height { get; private set; }

            /// <summary>
            /// 颜色列表
            /// </summary>
            public TmphColor[] Colors { get; private set; }

            /// <summary>
            /// 图象数据是否连续方式排列，否则使用顺序排列
            /// </summary>
            public byte InterlaceFlag { get; private set; }

            /// <summary>
            /// 颜色列表是否分类排列
            /// </summary>
            public byte SortFlag { get; private set; }

            /// <summary>
            /// LZW编码初始码表大小的位数
            /// </summary>
            public byte LzwSize { get; internal set; }

            /// <summary>
            /// 压缩数据集合
            /// </summary>
            public TmphList<TmphSubArray<byte>> lzwDatas;

            /// <summary>
            /// 位图填充
            /// </summary>
            private unsafe struct TmphFillBitmap
            {
                /// <summary>
                /// 当前颜色索引
                /// </summary>
                public byte* CurrentIndex;

                /// <summary>
                /// 颜色列表
                /// </summary>
                public TmphColor* Colors;

                /// <summary>
                /// 图像宽度
                /// </summary>
                public int Width;

                /// <summary>
                /// 填充颜色列表
                /// </summary>
                /// <param name="height">填充行数</param>
                /// <param name="bitmap">位图当前填充位置</param>
                /// <param name="bitMapSpace">位图填充留空</param>
                public void FillColor(int height, byte* bitmap, int bitMapSpace)
                {
                    byte* row = CurrentIndex;
                    for (byte* rowEnd = CurrentIndex + Width * height; row != rowEnd; bitmap += bitMapSpace)
                    {
                        byte* col = row;
                        for (row += Width; col != row; ++col)
                        {
                            TmphColor TmphColor = Colors[*col];
                            *bitmap++ = TmphColor.Blue;
                            *bitmap++ = TmphColor.Green;
                            *bitmap++ = TmphColor.Red;
                        }
                    }
                    CurrentIndex = row;
                }

                /// <summary>
                /// 填充颜色索引
                /// </summary>
                /// <param name="height">填充行数</param>
                /// <param name="bitmap">位图当前填充位置</param>
                /// <param name="bitMapSpace">位图填充留空</param>
                public void FillIndex(int height, byte* bitmap, int bitMapSpace)
                {
                    byte* row = CurrentIndex;
                    for (byte* rowEnd = CurrentIndex + Width * height; row != rowEnd; bitmap += bitMapSpace)
                    {
                        byte* col = row;
                        for (row += Width; col != row; ++col)
                        {
                            byte TmphColor = *col;
                            *bitmap++ = TmphColor;
                            *bitmap++ = TmphColor;
                            *bitmap++ = TmphColor;
                        }
                    }
                    CurrentIndex = row;
                }
            }

            /// <summary>
            /// 创建位图
            /// </summary>
            /// <param name="globalColors">全局颜色列表</param>
            /// <returns>位图,失败返回null</returns>
            public unsafe Bitmap CreateBitmap(TmphColor[] globalColors)
            {
                if (Width == 0 || Height == 0 || LzwSize == 0 || LzwSize > 8) return null;
                int colorSize = Width * Height;
                TmphPointer colorIndexs = lzwEncodeTableBuffer.Get(colorSize);
                try
                {
                    int length = lzwDecode(TmphDecoder.BlocksToByte(lzwDatas), colorIndexs, LzwSize);
                    if (length == colorSize)
                    {
                        Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
                        try
                        {
                            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, Width, Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                            byte* bitmapFixed = (byte*)bitmapData.Scan0;
                            int bitMapSpace = bitmapData.Stride - (Width << 1) - Width;
                            if (globalColors == null) globalColors = Colors;
                            if (globalColors != null)
                            {
                                fixed (TmphColor* colorFixed = globalColors)
                                {
                                    TmphFillBitmap fillBitmap = new TmphFillBitmap { CurrentIndex = colorIndexs.Byte, Colors = colorFixed, Width = Width };
                                    if (InterlaceFlag == 0) fillBitmap.FillColor(Height, bitmapFixed, bitMapSpace);
                                    else
                                    {
                                        int bitmapStride = bitMapSpace + (bitmapData.Stride << 3) - bitmapData.Stride;
                                        fillBitmap.FillColor((Height + 7) >> 3, bitmapFixed, bitmapStride);
                                        fillBitmap.FillColor((Height + 3) >> 3, bitmapFixed + (bitmapData.Stride << 2), bitmapStride);
                                        fillBitmap.FillColor((Height + 1) >> 2, bitmapFixed + (bitmapData.Stride << 1), bitmapStride -= bitmapData.Stride << 2);
                                        fillBitmap.FillColor(Height >> 1, bitmapFixed + bitmapData.Stride, bitmapStride - (bitmapData.Stride << 1));
                                    }
                                }
                            }
                            else
                            {
                                TmphFillBitmap fillBitmap = new TmphFillBitmap { CurrentIndex = colorIndexs.Byte, Width = Width };
                                if (InterlaceFlag == 0) fillBitmap.FillIndex(Height, bitmapFixed, bitMapSpace);
                                else
                                {
                                    int bitmapStride = bitMapSpace + (bitmapData.Stride << 3) - bitmapData.Stride;
                                    fillBitmap.FillIndex((Height + 7) >> 3, bitmapFixed, bitmapStride);
                                    fillBitmap.FillIndex((Height + 3) >> 3, bitmapFixed + (bitmapData.Stride << 2), bitmapStride);
                                    fillBitmap.FillIndex((Height + 1) >> 2, bitmapFixed + (bitmapData.Stride << 1), bitmapStride -= bitmapData.Stride << 2);
                                    fillBitmap.FillIndex(Height >> 1, bitmapFixed + bitmapData.Stride, bitmapStride - (bitmapData.Stride << 1));
                                }
                            }
                            bitmap.UnlockBits(bitmapData);
                            return bitmap;
                        }
                        catch (Exception error)
                        {
                            bitmap.Dispose();
                            TmphLog.Error.Add(error, null, false);
                        }
                    }
                }
                finally { lzwEncodeTableBuffer.Push(ref colorIndexs); }
                return null;
            }

            /// <summary>
            /// 图象标识符设置
            /// </summary>
            /// <param name="data">当前解析数据</param>
            public unsafe void SetDescriptor(byte* data)
            {
                LeftOffset = *(short*)data;
                TopOffset = *(short*)(data + 2);
                Width = *(short*)(data + 4);
                Height = *(short*)(data + 6);
                byte localFlag = *(data + 8);
                InterlaceFlag = (byte)(localFlag & 0x40);
                SortFlag = (byte)(localFlag & 0x20);
                if ((localFlag & 0x80) != 0) Colors = new TmphColor[1 << ((localFlag & 7) + 1)];
            }

            /// <summary>
            /// LZW压缩解码字符串缓冲区
            /// </summary>
#if MONO
            private static readonly TmphMemoryPool stringBuffers = TmphMemoryPool.GetPool(4097 * 8);
#else
            private static readonly TmphMemoryPool stringBuffers = TmphMemoryPoolProxy.GetPool(4097 * 8);
#endif

            /// <summary>
            /// LZW压缩解码
            /// </summary>
            /// <param name="input">输入数据</param>
            /// <param name="output">输出数据缓冲</param>
            /// <param name="table">字符串查找表格</param>
            /// <param name="size">编码长度</param>
            /// <returns>解码数据长度,失败返回-1</returns>
            private unsafe static int lzwDecode(byte[] input, TmphPointer output, byte size)
            {
                int tableSize = (int)size + 1;
                short clearIndex = (short)(1 << size), nextIndex = clearIndex;
                byte[] stringBuffer = stringBuffers.Get();
                try
                {
                    fixed (byte* inputFixed = input, stringFixed = stringBuffer)
                    {
                        byte* nextStrings = null;
                        byte* currentInput = inputFixed, inputEnd = inputFixed + input.Length;
                        byte* currentOutput = output.Byte, outputEnd = output.Byte + lzwEncodeTableBuffer.Size;
                        int valueBits = 0, inputSize = 0, inputOffset = (int)inputEnd & (sizeof(ulong) - 1), startSize = tableSize;
                        ulong inputValue = 0, inputMark = ushort.MaxValue, startMark = ((ulong)1UL << startSize) - 1;
                        short endIndex = (short)(clearIndex + 1), prefixIndex, currentIndex = 0;
                        if (inputOffset == 0)
                        {
                            inputEnd -= sizeof(ulong);
                            inputOffset = sizeof(ulong);
                        }
                        else inputEnd -= inputOffset;
                        if (size == 1) ++startSize;
                        while (currentIndex != endIndex)
                        {
                            if (valueBits >= startSize)
                            {
                                prefixIndex = (short)(inputValue & startMark);
                                valueBits -= startSize;
                                inputValue >>= startSize;
                            }
                            else
                            {
                                if (currentInput > inputEnd) return -1;
                                ulong nextValue = *(ulong*)currentInput;
                                prefixIndex = (short)((inputValue | (nextValue << valueBits)) & startMark);
                                inputValue = nextValue >> -(valueBits -= startSize);
                                valueBits += sizeof(ulong) << 3;
                                if (currentInput == inputEnd && (valueBits -= (sizeof(ulong) - inputOffset) << 3) < 0) return -1;
                                currentInput += sizeof(ulong);
                            }
                            if (prefixIndex == clearIndex) continue;
                            if (prefixIndex == endIndex) break;
                            if (currentOutput == outputEnd) return -1;

                            Array.Clear(stringBuffer, 0, 4097 * 8);
                            inputSize = startSize;
                            inputMark = startMark;
                            nextIndex = (short)(endIndex + 1);
                            *(short*)(nextStrings = stringFixed + (nextIndex << 3)) = prefixIndex;
                            *(short*)(nextStrings + 2) = prefixIndex;
                            *(int*)(nextStrings + 4) = 2;
                            *currentOutput++ = (byte)prefixIndex;
                            do
                            {
                                if (valueBits >= inputSize)
                                {
                                    currentIndex = (short)(inputValue & inputMark);
                                    valueBits -= inputSize;
                                    inputValue >>= inputSize;
                                }
                                else
                                {
                                    if (currentInput > inputEnd) return -1;
                                    ulong nextValue = *(ulong*)currentInput;
                                    currentIndex = (short)((inputValue | (nextValue << valueBits)) & inputMark);
                                    inputValue = nextValue >> -(valueBits -= inputSize);
                                    valueBits += sizeof(ulong) << 3;
                                    if (currentInput == inputEnd && (valueBits -= (sizeof(ulong) - inputOffset) << 3) < 0) return -1;
                                    currentInput += sizeof(ulong);
                                }
                                *(short*)(nextStrings += 8) = currentIndex;
                                if (currentIndex < clearIndex)
                                {
                                    if (currentOutput == outputEnd) return -1;
                                    *(short*)(nextStrings + 2) = currentIndex;
                                    *(int*)(nextStrings + 4) = 2;
                                    *currentOutput++ = (byte)currentIndex;
                                }
                                else if (currentIndex > endIndex)
                                {
                                    byte* currentString = stringFixed + (currentIndex << 3);
                                    int outputCount = *(int*)(currentString + 4);
                                    if (outputCount == 0) return -1;
                                    *(short*)(nextStrings + 2) = *(short*)(currentString + 2);
                                    *(int*)(nextStrings + 4) = outputCount + 1;
                                    if ((currentOutput += outputCount) > outputEnd) return -1;
                                    do
                                    {
                                        *--currentOutput = *(currentString + 2 + 8);
                                        prefixIndex = *(short*)currentString;
                                        if (prefixIndex < clearIndex) break;
                                        currentString = stringFixed + (prefixIndex << 3);
                                    }
                                    while (true);
                                    *--currentOutput = (byte)prefixIndex;
                                    currentOutput += outputCount;
                                }
                                else break;
                                prefixIndex = currentIndex;
                                if (nextIndex++ == (short)inputMark)
                                {
                                    if (inputSize == 12) return -1;
                                    inputMark <<= 1;
                                    ++inputSize;
                                    ++inputMark;
                                }
                            }
                            while (true);
                        }
                        return (int)(currentOutput - output.Byte);
                    }
                }
                finally { stringBuffers.Push(ref stringBuffer); }
            }
        }

        /// <summary>
        /// 图形控制扩展
        /// </summary>
        public sealed class TmphGraphicControl
        {
            /// <summary>
            /// 图形处置方法
            /// </summary>
            public enum TmphMethodType
            {
                /// <summary>
                /// 不使用处置方法
                /// </summary>
                None = 0,

                /// <summary>
                /// 不处置图形，保留当前的图像，再绘制一帧图像在上面
                /// </summary>
                KeepCurrent = 1,

                /// <summary>
                /// 回复到背景色
                /// </summary>
                BackgroundColor = 2,

                /// <summary>
                /// 回复到先前状态
                /// </summary>
                PreviousState = 3,

                /// <summary>
                /// 自定义
                /// </summary>
                Custom4 = 4,

                /// <summary>
                /// 自定义
                /// </summary>
                Custom5 = 5,

                /// <summary>
                /// 自定义
                /// </summary>
                Custom6 = 6,

                /// <summary>
                /// 自定义
                /// </summary>
                Custom7 = 7
            }

            /// <summary>
            /// 延迟时间，单位1/100秒
            /// </summary>
            public short DelayTime { get; private set; }

            /// <summary>
            /// 透明色索引值
            /// </summary>
            public byte TransparentColorIndex { get; private set; }

            /// <summary>
            /// 是否使用使用透明颜色
            /// </summary>
            public byte IsTransparentColor { get; private set; }

            /// <summary>
            /// 用户输入标志，指出是否期待用户有输入之后才继续进行下去，置位表示期待，值否表示不期待。
            /// </summary>
            public byte IsUseInput { get; private set; }

            /// <summary>
            /// 图形处置方法
            /// </summary>
            public TmphMethodType MethodType { get; private set; }

            /// <summary>
            /// 图形控制扩展
            /// </summary>
            /// <param name="data">当前解析数据</param>
            public unsafe TmphGraphicControl(byte* data)
            {
                byte flag = *data;
                MethodType = (TmphGraphicControl.TmphMethodType)((flag >> 2) & 7);
                IsUseInput = (byte)(flag & 2);
                IsTransparentColor = (byte)(flag & 1);
                DelayTime = *(short*)(data + 1);
                TransparentColorIndex = *(data + 3);
            }
        }

        /// <summary>
        /// 图形文本扩展
        /// </summary>
        public sealed class TmphPlainText
        {
            /// <summary>
            /// 文本框离逻辑屏幕的左边界距离
            /// </summary>
            public short Left { get; private set; }

            /// <summary>
            /// 文本框离逻辑屏幕的上边界距离
            /// </summary>
            public short Top { get; private set; }

            /// <summary>
            /// 文本框像素宽度
            /// </summary>
            public short Width { get; private set; }

            /// <summary>
            /// 文本框像素高度
            /// </summary>
            public short Height { get; private set; }

            /// <summary>
            /// 字符宽度
            /// </summary>
            public short CharacterWidth { get; private set; }

            /// <summary>
            /// 字符高度
            /// </summary>
            public short CharacterHeight { get; private set; }

            /// <summary>
            /// 前景色在全局颜色列表中的索引
            /// </summary>
            public byte ColorIndex { get; private set; }

            /// <summary>
            /// 背景色在全局颜色列表中的索引
            /// </summary>
            public byte BlackgroundColorIndex { get; private set; }

            /// <summary>
            /// 文本数据块集合
            /// </summary>
            public TmphList<TmphSubArray<byte>> text;

            /// <summary>
            /// 文本数据
            /// </summary>
            public byte[] Text
            {
                get { return TmphDecoder.BlocksToByte(text); }
            }

            /// <summary>
            /// 图形文本扩展
            /// </summary>
            /// <param name="data">当前解析数据</param>
            public unsafe TmphPlainText(byte* data)
            {
                Left = *(short*)data;
                Top = *(short*)(data + 2);
                Width = *(short*)(data + 4);
                Height = *(short*)(data + 6);
                CharacterWidth = *(data + 8);
                CharacterHeight = *(data + 9);
                ColorIndex = *(data + 10);
                BlackgroundColorIndex = *(data + 11);
            }
        }

        /// <summary>
        /// 应用程序扩展
        /// </summary>
        public sealed class TmphApplication
        {
            /// <summary>
            /// 用来鉴别应用程序自身的标识(8个连续ASCII字符)
            /// </summary>
            public TmphSubArray<byte> Identifier { get; private set; }

            /// <summary>
            /// 应用程序定义的特殊标识码(3个连续ASCII字符)
            /// </summary>
            public TmphSubArray<byte> AuthenticationCode { get; private set; }

            /// <summary>
            /// 应用程序自定义数据块集合
            /// </summary>
            private TmphList<TmphSubArray<byte>> customDatas;

            /// <summary>
            /// 应用程序自定义数据块
            /// </summary>
            public byte[] CustomData
            {
                get { return TmphDecoder.BlocksToByte(customDatas); }
            }

            /// <summary>
            /// 应用程序扩展
            /// </summary>
            /// <param name="identifier">用来鉴别应用程序自身的标识(8个连续ASCII字符)</param>
            /// <param name="authenticationCode">应用程序定义的特殊标识码(3个连续ASCII字符)</param>
            /// <param name="customDatas">应用程序自定义数据块集合</param>
            public TmphApplication(TmphSubArray<byte> identifier, TmphSubArray<byte> authenticationCode, TmphList<TmphSubArray<byte>> customDatas)
            {
                Identifier = identifier;
                AuthenticationCode = authenticationCode;
                this.customDatas = customDatas;
            }
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        public enum TmphDataType
        {
            /// <summary>
            /// 图像块
            /// </summary>
            Image,

            /// <summary>
            /// 图形控制扩展(需要89a版本)
            /// </summary>
            GraphicControl,

            /// <summary>
            /// 图形文本扩展(需要89a版本)
            /// </summary>
            PlainText,

            /// <summary>
            /// 注释扩展(需要89a版本)
            /// </summary>
            Comment,

            /// <summary>
            /// 应用程序扩展(需要89a版本)
            /// </summary>
            Application,
        }

        /// <summary>
        /// 数据块
        /// </summary>
        public sealed class TmphDataBlock
        {
            /// <summary>
            /// 数据类型
            /// </summary>
            public TmphDataType Type { get; private set; }

            /// <summary>
            /// 图像块
            /// </summary>
            public TmphImage Image { get; private set; }

            /// <summary>
            /// 图形控制扩展
            /// </summary>
            public TmphGraphicControl GraphicControl { get; private set; }

            /// <summary>
            /// 注释扩展
            /// </summary>
            public TmphSubArray<byte> Comment { get; private set; }

            /// <summary>
            /// 图形文本扩展
            /// </summary>
            public TmphPlainText PlainText { get; private set; }

            /// <summary>
            /// 应用程序扩展
            /// </summary>
            public TmphApplication Application { get; private set; }

            /// <summary>
            /// 数据块
            /// </summary>
            /// <param name="TmphImage">图像块</param>
            public TmphDataBlock(TmphImage TmphImage)
            {
                Type = TmphDataType.Image;
                Image = TmphImage;
            }

            /// <summary>
            /// 数据块
            /// </summary>
            /// <param name="TmphImage">图形控制扩展</param>
            public TmphDataBlock(TmphGraphicControl TmphGraphicControl)
            {
                Type = TmphDataType.GraphicControl;
                GraphicControl = TmphGraphicControl;
            }

            /// <summary>
            /// 数据块
            /// </summary>
            /// <param name="TmphImage">注释扩展</param>
            public TmphDataBlock(TmphSubArray<byte> comment)
            {
                Type = TmphDataType.Comment;
                Comment = comment;
            }

            /// <summary>
            /// 数据块
            /// </summary>
            /// <param name="TmphImage">图形文本扩展</param>
            public TmphDataBlock(TmphPlainText TmphPlainText)
            {
                Type = TmphDataType.PlainText;
                PlainText = TmphPlainText;
            }

            /// <summary>
            /// 数据块
            /// </summary>
            /// <param name="TmphImage">应用程序扩展</param>
            public TmphDataBlock(TmphApplication TmphApplication)
            {
                Type = TmphDataType.Application;
                Application = TmphApplication;
            }
        }

        /// <summary>
        /// GIF文件解码器
        /// </summary>
        private sealed unsafe class TmphDecoder
        {
            /// <summary>
            /// GIF文件数据
            /// </summary>
            private byte[] data;

            /// <summary>
            /// GIF文件数据起始位置
            /// </summary>
            private byte* dataPoint;

            /// <summary>
            /// GIF文件数据当前解析位置
            /// </summary>
            private byte* currentData;

            /// <summary>
            /// GIF文件数据结束位置
            /// </summary>
            private byte* dataEnd;

            /// <summary>
            /// 是否文件结束
            /// </summary>
            public bool IsFileEnd
            {
                get { return *currentData == 0x3b; }
            }

            /// <summary>
            /// GIF文件解码器
            /// </summary>
            /// <param name="data">GIF文件数据</param>
            public TmphDecoder(byte[] data, byte* dataPoint, byte* currentData)
            {
                this.data = data;
                this.dataPoint = dataPoint;
                this.currentData = currentData;
                dataEnd = dataPoint + data.Length - 1;
            }

            /// <summary>
            /// 解码下一个数据块
            /// </summary>
            /// <returns>下一个数据块,失败返回null</returns>
            public TmphDataBlock Next()
            {
                if (*currentData == 0x2c) return decodeImage();
                else if (*currentData == 0x21)
                {
                    switch (*++currentData)
                    {
                        case 0xf9:
                            return decodeGraphicControl();

                        case 1:
                            return decodePlainText();

                        case 0xfe:
                            return decodeComment();

                        case 0xff:
                            return decodeApplication();
                    }
                }
                return null;
            }

            /// <summary>
            /// 解码图像块
            /// </summary>
            /// <returns>失败返回null</returns>
            private TmphDataBlock decodeImage()
            {
                int length = data.Length - (int)(currentData - dataPoint) - 12;
                if (length <= 0) return null;
                TmphImage TmphImage = new TmphImage();
                TmphImage.SetDescriptor(++currentData);
                currentData += 9;
                if (TmphImage.Colors != null)
                {
                    int colorCount = TmphImage.Colors.Length;
                    length -= (colorCount << 1) + colorCount;
                    if (length <= 0) return null;
                    currentData = FillColor(TmphImage.Colors, currentData);
                }
                TmphImage.LzwSize = *currentData++;
                if ((TmphImage.lzwDatas = getBlockList()) == null) return null;
                return new TmphDataBlock(TmphImage);
            }

            /// <summary>
            /// 解码图形控制扩展
            /// </summary>
            /// <returns>失败返回null</returns>
            private TmphDataBlock decodeGraphicControl()
            {
                if (data.Length - (int)(++currentData - dataPoint) <= 6) return null;
                if (((*currentData ^ 4) | *(currentData + 5)) != 0) return null;
                TmphGraphicControl TmphGraphicControl = new TmphGraphicControl(++currentData);
                currentData += 5;
                return new TmphDataBlock(TmphGraphicControl);
            }

            /// <summary>
            /// 解码图形文本扩展
            /// </summary>
            /// <returns>失败返回null</returns>
            private TmphDataBlock decodePlainText()
            {
                if (*++currentData != 12) return null;
                TmphPlainText TmphPlainText = new TmphPlainText(++currentData);
                if ((currentData += 12) >= dataEnd) return null;
                if ((TmphPlainText.text = getBlockList()) == null) return null;
                return new TmphDataBlock(TmphPlainText);
            }

            /// <summary>
            /// 解码注释扩展
            /// </summary>
            /// <returns>失败返回null</returns>
            private TmphDataBlock decodeComment()
            {
                if (++currentData >= dataEnd) return null;
                TmphSubArray<byte> comment = getBlocks();
                if (comment.Array == null) return null;
                return new TmphDataBlock(comment);
            }

            /// <summary>
            /// 解码应用程序扩展
            /// </summary>
            /// <returns>失败返回null</returns>
            private TmphDataBlock decodeApplication()
            {
                if (*++currentData != 11) return null;
                int startIndex = (int)(currentData - dataPoint);
                if ((currentData += 12) >= dataEnd) return null;
                TmphList<TmphSubArray<byte>> customDatas = getBlockList();
                if (customDatas == null) return null;
                return new TmphDataBlock(new TmphApplication(TmphSubArray<byte>.Unsafe(data, startIndex + 1, 8), TmphSubArray<byte>.Unsafe(data, startIndex + 9, 3), customDatas));
            }

            /// <summary>
            /// 填充数据块
            /// </summary>
            /// <returns>填充数据块</returns>
            private TmphSubArray<byte> getBlocks()
            {
                byte* dataStart = currentData;
                for (byte count = *currentData; count != 0; count = *currentData)
                {
                    currentData += count;
                    if (++currentData >= dataEnd) return new TmphSubArray<byte>();
                }
                TmphSubArray<byte> blocks = TmphSubArray<byte>.Unsafe(data, (int)(dataStart - dataPoint), (int)(currentData - dataStart));
                ++currentData;
                return blocks;
            }

            /// <summary>
            /// 填充数据块集合
            /// </summary>
            /// <returns>填充数据块集合</returns>
            private TmphList<TmphSubArray<byte>> getBlockList()
            {
                TmphList<TmphSubArray<byte>> datas = new TmphList<TmphSubArray<byte>>();
                int startIndex = (int)(currentData - dataPoint);
                for (byte count = *currentData; count != 0; count = *currentData)
                {
                    currentData += count;
                    if (++currentData >= dataEnd) return null;
                    datas.Add(TmphSubArray<byte>.Unsafe(data, ++startIndex, count));
                    startIndex += count;
                }
                ++currentData;
                return datas;
            }

            /// <summary>
            /// 颜色列表数据填充
            /// </summary>
            /// <param name="colors">颜色列表数组</param>
            /// <param name="data">颜色列表数据</param>
            /// <returns>数据结束位置</returns>
            public static unsafe byte* FillColor(TmphColor[] colors, byte* data)
            {
                fixed (TmphColor* globalColorsFixed = colors)
                {
                    int offset = colors.Length & (sizeof(ulong) - 1);
                    TmphColor* currentColor = globalColorsFixed;
                    for (TmphColor* endColor = currentColor + (colors.Length - offset); currentColor != endColor; ++currentColor)
                    {
                        ulong value0 = *(ulong*)data, value1 = *(ulong*)(data + sizeof(ulong));
                        (*currentColor).Value = (int)(uint)value0;
                        (*++currentColor).Value = (int)(uint)(value0 >> 24);
                        (*++currentColor).Value = (int)((uint)(value0 >> 48) | ((uint)value1 << 16));
                        (*++currentColor).Value = (int)((uint)value1 >> 8);
                        value0 = *(ulong*)(data + sizeof(ulong) * 2);
                        (*++currentColor).Value = (int)(uint)(value1 >> 32);
                        (*++currentColor).Value = (int)((uint)(value1 >> 56) | ((uint)value0 << 8));
                        (*++currentColor).Value = (int)(uint)(value0 >> 16);
                        (*++currentColor).Value = (int)(uint)(value0 >> 40);
                        //(*++currentColor).Value = (int)((value0 >> 24) | (value1 << 8));
                        //value0 = *(cpuUint*)(data + sizeof(cpuUint) * 2);
                        //(*++currentColor).Value = (int)((value1 >> 16) | (value0 << 16));
                        //(*++currentColor).Value = (int)(value0 >> 8);
                        data += sizeof(ulong) * 3;
                    }
                    for (TmphColor* endColor = currentColor + offset; currentColor != endColor; ++currentColor)
                    {
                        (*currentColor).Red = *data++;
                        (*currentColor).Green = *data++;
                        (*currentColor).Blue = *data++;
                    }
                }
                return data;
            }

            /// <summary>
            /// 合并数据块集合
            /// </summary>
            /// <param name="datas">数据块集合</param>
            /// <returns>合并后的数据块</returns>
            public unsafe static byte[] BlocksToByte(TmphList<TmphSubArray<byte>> datas)
            {
                if (datas.count() != 0)
                {
                    int length = 0, count = datas.Count;
                    TmphSubArray<byte>[] array = datas.Unsafer.Array;
                    for (int index = 0; index != count; ++index) length += array[index].Count;
                    byte[] data = new byte[length];
                    fixed (byte* dataFixed = data)
                    {
                        byte* currentData = dataFixed;
                        for (int index = 0; index != count; ++index)
                        {
                            TmphSubArray<byte> TmphSubArray = array[index];
                            fixed (byte* subArrayFixed = TmphSubArray.Array)
                            {
                                Laurent.Lee.CLB.Unsafe.TmphMemory.Copy(subArrayFixed + TmphSubArray.StartIndex, currentData, TmphSubArray.Count);
                            }
                            currentData += TmphSubArray.Count;
                        }
                    }
                    return data;
                }
                return null;
            }
        }

        /// <summary>
        /// GIF文件写入器
        /// </summary>
        public sealed class TmphWriter : IDisposable
        {
            /// <summary>
            /// GIF文件标识与版本信息
            /// </summary>
            private const ulong fileVersion = 'G' + ('I' << 8) + ('F' << 16) + ('8' << 24) + ((ulong)'9' << 32) + ((ulong)'a' << 40);

            /// <summary>
            /// 文件流
            /// </summary>
            private FileStream fileStream;

            /// <summary>
            /// 文件缓冲区
            /// </summary>
            private byte[] fileBuffer;

            /// <summary>
            /// 当前图像色彩缓存
            /// </summary>
            private TmphColor[] colors;

            /// <summary>
            /// 当前图像色彩数量缓存
            /// </summary>
            private int[] colorCounts;

            /// <summary>
            /// 当前图像色彩数量
            /// </summary>
            private Dictionary<TmphColor, int> colorIndexs;

            /// <summary>
            /// 当前文件缓存位置
            /// </summary>
            private int bufferIndex;

            /// <summary>
            /// 素数宽度
            /// </summary>
            public short Width { get; private set; }

            /// <summary>
            /// 素数高度
            /// </summary>
            public short Height { get; private set; }

            /// <summary>
            /// 全局颜色数量
            /// </summary>
            private int globalColorCount;

            /// <summary>
            /// 文件缓冲区
            /// </summary>
#if MONO
            private static readonly TmphMemoryPool fileBuffers = TmphMemoryPool.GetPool(Laurent.Lee.CLB.Config.appSetting.StreamBufferSize + (256 * 3) + 8);
#else
            private static readonly TmphMemoryPool fileBuffers = TmphMemoryPoolProxy.GetPool(Laurent.Lee.CLB.Config.TmphAppSetting.StreamBufferSize + (256 * 3) + 8);
#endif

            /// <summary>
            /// GIF文件写入器
            /// </summary>
            /// <param name="filename">文件名称</param>
            /// <param name="width">素数宽度</param>
            /// <param name="height">素数高度</param>
            /// <param name="globalColors">全局颜色列表</param>
            /// <param name="backgroundColorIndex">背景颜色在全局颜色列表中的索引，如果没有全局颜色列表，该值没有意义</param>
            public unsafe TmphWriter(string filename, short width, short height, TmphColor[] globalColors = null, byte backgroundColorIndex = 0)
            {
                if (width <= 0 || height <= 0 || filename.Length == 0) CLB.TmphLog.Error.Throw(TmphLog.TmphExceptionType.Null);
                fileStream = new FileStream(filename, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.WriteThrough);
                fileBuffer = fileBuffers.Get();
                Width = width;
                Height = height;
                globalColorCount = globalColors.length();
                int pixel = 0;
                if (globalColorCount != 0)
                {
                    if (globalColorCount < 256)
                    {
                        pixel = ((uint)globalColorCount).bits() - 1;
                        if (globalColorCount != (1 << pixel)) ++pixel;
                    }
                    else
                    {
                        globalColorCount = 256;
                        pixel = 7;
                    }
                    pixel |= 0x80;
                }
                fixed (byte* bufferFixed = fileBuffer)
                {
                    *(ulong*)bufferFixed = fileVersion | ((ulong)width << 48);
                    *(uint*)(bufferFixed + 8) = (uint)(int)height | (globalColorCount == 0 ? 0 : ((uint)pixel << 16)) | (7 << (16 + 4))
                        | (backgroundColorIndex >= globalColorCount ? 0 : ((uint)backgroundColorIndex << 24));
                    bufferIndex = 13;
                    if (globalColorCount != 0)
                    {
                        byte* currentBuffer = bufferFixed + 13;
                        fixed (TmphColor* colorFixed = globalColors)
                        {
                            for (TmphColor* currentColor = colorFixed, colorEnd = colorFixed + globalColorCount; currentColor != colorEnd; ++currentColor)
                            {
                                TmphColor TmphColor = *currentColor;
                                *currentBuffer++ = TmphColor.Red;
                                *currentBuffer++ = TmphColor.Green;
                                *currentBuffer++ = TmphColor.Blue;
                            }
                        }
                        bufferIndex += 3 << (pixel ^ 0x80);
                    }
                }
                colors = new TmphColor[(int)Width * Height];
                colorCounts = new int[colors.Length];
                colorIndexs = TmphDictionary.Create<TmphColor, int>();
            }

            /// <summary>
            /// 释放文件
            /// </summary>
            public void Dispose()
            {
                using (FileStream fileStream = this.fileStream)
                {
                    this.fileStream = null;
                    if (fileStream != null)
                    {
                        fileBuffer[bufferIndex++] = 0x3b;
                        try
                        {
                            fileStream.Write(fileBuffer, 0, bufferIndex);
                        }
                        catch (Exception error)
                        {
                            TmphLog.Error.Add(error, null, false);
                        }
                        finally
                        {
                            colors = null;
                            colorCounts = null;
                            fileBuffers.Push(ref fileBuffer);
                        }
                    }
                }
            }

            /// <summary>
            /// 检测文件缓存
            /// </summary>
            /// <param name="bufferFixed">文件缓存起始位置</param>
            /// <returns>文件是否写入成功</returns>
            private unsafe bool checkBuffer(byte* bufferFixed)
            {
                int count = bufferIndex - Laurent.Lee.CLB.Config.TmphAppSetting.StreamBufferSize;
                if (count >= 0)
                {
                    try
                    {
                        fileStream.Write(fileBuffer, 0, Laurent.Lee.CLB.Config.TmphAppSetting.StreamBufferSize);
                        Laurent.Lee.CLB.Unsafe.TmphMemory.Copy(bufferFixed + Laurent.Lee.CLB.Config.TmphAppSetting.StreamBufferSize, bufferFixed, bufferIndex = count);
                    }
                    catch (Exception error)
                    {
                        TmphLog.Error.Add(error, null, false);
                        fileBuffers.Push(ref fileBuffer);
                        TmphPub.Dispose(ref fileStream);
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// 检测文件缓存
            /// </summary>
            /// <param name="bufferFixed">文件缓存起始位置</param>
            /// <param name="length">新增长度</param>
            /// <returns>文件是否写入成功</returns>
            private unsafe bool checkBuffer(byte* bufferFixed, int length)
            {
                bufferIndex += length;
                return checkBuffer(bufferFixed);
            }

            /// <summary>
            /// 添加图片
            /// </summary>
            /// <param name="bitmap">位图</param>
            /// <param name="leftOffset">X方向偏移量</param>
            /// <param name="topOffset">Y方向偏移量</param>
            /// <param name="width">图象宽度</param>
            /// <param name="height">图象高度</param>
            /// <param name="bitmapLeftOffset">位图剪切X方向偏移量</param>
            /// <param name="bitmapTopOffset">位图剪切Y方向偏移量</param>
            /// <param name="isInterlace">图象数据是否连续方式排列，否则使用顺序排列</param>
            /// <param name="maxPixel">最大色彩深度</param>
            /// <returns>图片是否添加成功</returns>
            public unsafe bool AddImage(Bitmap bitmap, int bitmapLeftOffset = 0, int bitmapTopOffset = 0
                , int leftOffset = 0, int topOffset = 0, int width = 0, int height = 0, bool isInterlace = false, byte maxPixel = 8)
            {
                if (fileBuffer == null || bitmap == null) return false;
                if (width == 0) width = Width;
                if (height == 0) height = Height;
                if (leftOffset < 0)
                {
                    bitmapLeftOffset -= leftOffset;
                    width += leftOffset;
                    leftOffset = 0;
                }
                if (topOffset < 0)
                {
                    bitmapTopOffset -= topOffset;
                    height += topOffset;
                    topOffset = 0;
                }
                if (bitmapLeftOffset < 0)
                {
                    leftOffset -= bitmapLeftOffset;
                    width += bitmapLeftOffset;
                    bitmapLeftOffset = 0;
                }
                if (bitmapTopOffset < 0)
                {
                    topOffset -= bitmapTopOffset;
                    height += bitmapTopOffset;
                    bitmapTopOffset = 0;
                }
                int minWidth = bitmap.Width - bitmapLeftOffset, minHeight = bitmap.Height - bitmapTopOffset;
                if (minWidth < width) width = minWidth;
                if (minHeight < height) height = minHeight;
                if ((minWidth = width - leftOffset) < width) width = minWidth;
                if ((minHeight = height - topOffset) < height) height = minHeight;
                if (width <= 0 || height <= 0) return false;
                if ((byte)(maxPixel - 2) >= 8) maxPixel = 8;
                BitmapData bitmapData = null;
                try
                {
                    bitmapData = bitmap.LockBits(new Rectangle(bitmapLeftOffset, bitmapTopOffset, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                    return false;
                }
                try
                {
                    return addImage(bitmapData, 0, 0, leftOffset, topOffset, width, height, isInterlace, maxPixel);
                }
                finally { bitmap.UnlockBits(bitmapData); }
            }

            /// <summary>
            /// 添加图片
            /// </summary>
            /// <param name="bitmap">位图</param>
            /// <param name="leftOffset">X方向偏移量</param>
            /// <param name="topOffset">Y方向偏移量</param>
            /// <param name="width">图象宽度</param>
            /// <param name="height">图象高度</param>
            /// <param name="bitmapLeftOffset">位图剪切X方向偏移量</param>
            /// <param name="bitmapTopOffset">位图剪切Y方向偏移量</param>
            /// <param name="isInterlace">图象数据是否连续方式排列，否则使用顺序排列</param>
            /// <param name="maxPixel">最大色彩深度</param>
            /// <returns>图片是否添加成功</returns>
            public unsafe bool addImage(BitmapData bitmap, int bitmapLeftOffset, int bitmapTopOffset
                , int leftOffset, int topOffset, int width, int height, bool isInterlace, byte maxPixel)
            {
                if (fileBuffer == null) return false;
                fixed (TmphColor* colorFixed = colors)
                fixed (int* colorCountFixed = colorCounts)
                {
                    byte* bitmapFixed = (byte*)bitmap.Scan0, currentBitmap = bitmapFixed + bitmap.Stride * (bitmapTopOffset - 1) + (bitmapLeftOffset + width) * 3;
                    TmphColor* currentColor = colorFixed;
                    int bitMapSpace = bitmap.Stride - (width << 1) - width;
                    colorIndexs.Clear();
                    for (int colorIndex, row = height; row != 0; --row)
                    {
                        currentBitmap += bitMapSpace;
                        for (int col = width; col != 0; --col)
                        {
                            TmphColor TmphColor = new TmphColor { Green = *currentBitmap++, Blue = *currentBitmap++, Red = *currentBitmap++ };
                            if (colorIndexs.TryGetValue(TmphColor, out colorIndex)) ++colorCountFixed[colorIndex];
                            else
                            {
                                colorIndexs.Add(TmphColor, colorIndex = (int)(currentColor - colorFixed));
                                colorCountFixed[colorIndex] = 1;
                            }
                            *currentColor++ = TmphColor;
                        }
                    }
                    int pixel = ((uint)colorIndexs.Count).bits() - 1;
                    if ((1 << pixel) != colorIndexs.Count) ++pixel;
                    if (pixel > maxPixel) pixel = maxPixel;
                    else if (pixel < 2) pixel = 2;
                    int maxColorCount = 1 << pixel;
                    fixed (byte* bufferFixed = fileBuffer)
                    {
                        byte* currentBuffer = bufferFixed + bufferIndex;
                        *currentBuffer = 0x2c;
                        *(short*)(currentBuffer + 1) = (short)leftOffset;
                        *(short*)(currentBuffer + 3) = (short)topOffset;
                        *(short*)(currentBuffer + 5) = (short)width;
                        *(short*)(currentBuffer + 7) = (short)height;
                        *(currentBuffer + 9) = (byte)(0x80 + (isInterlace ? 0x40 : 0) + (pixel - 1));
                        if (!checkBuffer(bufferFixed, 10)) return false;
                    }
                    if (colorIndexs.Count <= maxColorCount)
                    {
                        fixed (byte* bufferFixed = fileBuffer)
                        {
                            int* currentColorCount = colorCountFixed;
                            foreach (TmphColor colorKey in colorIndexs.Keys) *currentColorCount++ = colorKey.Value;
                            TmphColor TmphColor = new TmphColor();
                            int currentColorIndex = 0;
                            byte* currentBuffer = bufferFixed + bufferIndex;
                            while (currentColorCount != colorCountFixed)
                            {
                                TmphColor.Value = *--currentColorCount;
                                *currentBuffer++ = TmphColor.Red;
                                *currentBuffer++ = TmphColor.Blue;
                                *currentBuffer++ = TmphColor.Green;
                                colorIndexs[TmphColor] = currentColorIndex++;
                            }
                            *(bufferFixed + bufferIndex + (maxColorCount << 1) + maxColorCount) = (byte)pixel;
                            if (!checkBuffer(bufferFixed, (maxColorCount << 1) + maxColorCount + 1)) return false;
                        }
                    }
                    else
                    {
                        int indexCount = colorIndexs.Count;
                        TmphUnmanagedPool pool = TmphUnmanagedPool.GetDefaultPool(indexCount * sizeof(TmphIntSortIndex));
                        TmphPointer buffer = pool.Get(indexCount * (sizeof(TmphIntSortIndex) + sizeof(int)));
                        try
                        {
                            TmphIntSortIndex* indexFixed = (TmphIntSortIndex*)(buffer.Int + indexCount), currentSortIndex = indexFixed;
                            foreach (KeyValuePair<TmphColor, int> colorIndex in colorIndexs)
                            {
                                int color0 = colorIndex.Key.Value;
                                int color3 = ((color0 >> 3) & 0x111111) * 0x1020400;
                                int color2 = ((color0 >> 2) & 0x111111) * 0x1020400;
                                int color1 = ((color0 >> 1) & 0x111111) * 0x1020400;
                                color0 = (color0 & 0x111111) * 0x1020400;
                                (*currentSortIndex++).Set((color3 & 0x70000000) | ((color2 >> 4) & 0x7000000)
                                    | ((color1 >> 8) & 0x700000) | ((color0 >> 12) & 0x70000) | ((color3 >> 12) & 0x7000)
                                    | ((color2 >> 16) & 0x700) | ((color1 >> 20) & 0x70) | ((color0 >> 24) & 7), colorIndex.Value);
                            }
                            TmphQuickSort.sort(indexFixed, indexFixed + indexCount - 1);
                            int* currentSortArray;
                            if (maxColorCount != 2)
                            {
                                currentSortArray = buffer.Int;
                                for (int currentColorCode, lastColorCode = (*--currentSortIndex).Value; currentSortIndex != indexFixed; lastColorCode = currentColorCode)
                                {
                                    currentColorCode = (*--currentSortIndex).Value;
                                    *currentSortArray++ = lastColorCode - currentColorCode;
                                }
                                currentSortArray = buffer.Int + (maxColorCount >> 1) - 2;
                                new TmphQuickSort.intRangeSorterDesc { SkipCount = currentSortArray, GetEndIndex = currentSortArray }.Sort(buffer.Int, buffer.Int + indexCount - 2);
                                int minColorDifference = *currentSortArray, minColorDifferenceCount = 1;
                                while (currentSortArray != buffer.Int)
                                {
                                    if (*--currentSortArray == minColorDifference) ++minColorDifferenceCount;
                                }
                                currentSortIndex = indexFixed + indexCount;
                                int maxCountIndex = (*--currentSortIndex).Index, maxCount = *(colorCountFixed + maxCountIndex);
                                for (int currentColorCode, lastColorCode = (*currentSortIndex).Value; currentSortIndex != indexFixed; lastColorCode = currentColorCode)
                                {
                                    currentColorCode = (*--currentSortIndex).Value;
                                    int colorDifference = lastColorCode - currentColorCode;
                                    if (colorDifference >= minColorDifference)
                                    {
                                        if (colorDifference == minColorDifference && --minColorDifferenceCount == 0) ++minColorDifference;
                                        *(colorCountFixed + maxCountIndex) = int.MaxValue;
                                        maxCount = *(colorCountFixed + (maxCountIndex = (*currentSortIndex).Index));
                                    }
                                    else
                                    {
                                        int countIndex = (*currentSortIndex).Index, count = *(colorCountFixed + countIndex);
                                        if (count > maxCount)
                                        {
                                            maxCountIndex = countIndex;
                                            maxCount = count;
                                        }
                                    }
                                }
                                *(colorCountFixed + maxCountIndex) = int.MaxValue;
                            }
                            for (currentSortArray = buffer.Int + indexCount, currentSortIndex = indexFixed; currentSortArray != buffer.Int; *(--currentSortArray) = *(colorCountFixed + (*currentSortIndex++).Index)) ;
                            currentSortArray = buffer.Int + maxColorCount - 1;
                            new TmphQuickSort.intRangeSorterDesc { SkipCount = currentSortArray, GetEndIndex = currentSortArray }.Sort(buffer.Int, buffer.Int + indexCount - 1);
                            int minColorCount = *currentSortArray, minColorCounts = 1;
                            while (currentSortArray != buffer.Int)
                            {
                                if (*--currentSortArray == minColorCount) ++minColorCounts;
                            }
                            fixed (byte* fileBufferFixed = fileBuffer)
                            {
                                byte* currentBuffer = fileBufferFixed + bufferIndex;
                                TmphIntSortIndex* lastSortIndex = indexFixed, endSortIndex = indexFixed + indexCount;
                                while (*(colorCountFixed + (*lastSortIndex).Index) < minColorCount) colorIndexs[*(colorFixed + (*lastSortIndex++).Index)] = 0;
                                if (*(colorCountFixed + (*lastSortIndex).Index) == minColorCount && --minColorCounts == 0) ++minColorCount;
                                TmphColor outputColor = *(colorFixed + (*lastSortIndex).Index);
                                *currentBuffer++ = outputColor.Red;
                                *currentBuffer++ = outputColor.Blue;
                                *currentBuffer++ = outputColor.Green;
                                colorIndexs[outputColor] = 0;
                                for (--maxColorCount; *(colorCountFixed + (*--endSortIndex).Index) < minColorCount; colorIndexs[*(colorFixed + (*endSortIndex).Index)] = maxColorCount) ;
                                if (*(colorCountFixed + (*endSortIndex).Index) == minColorCount && --minColorCounts == 0) ++minColorCount;
                                colorIndexs[*(colorFixed + (*endSortIndex).Index)] = maxColorCount++;
                                int currentColorIndex = 0;
                                for (int* lastColorCount = colorCountFixed + (*endSortIndex).Index; lastSortIndex != endSortIndex;)
                                {
                                    for (*lastColorCount = 0; *(colorCountFixed + (*++lastSortIndex).Index) >= minColorCount; colorIndexs[outputColor] = ++currentColorIndex)
                                    {
                                        if (*(colorCountFixed + (*lastSortIndex).Index) == minColorCount && --minColorCounts == 0) ++minColorCount;
                                        outputColor = *(colorFixed + (*lastSortIndex).Index);
                                        *currentBuffer++ = outputColor.Red;
                                        *currentBuffer++ = outputColor.Blue;
                                        *currentBuffer++ = outputColor.Green;
                                    }
                                    if (lastSortIndex == endSortIndex) break;
                                    *lastColorCount = int.MaxValue;
                                    TmphIntSortIndex* nextSortIndex = lastSortIndex;
                                    while (*(colorCountFixed + (*++nextSortIndex).Index) < minColorCount) ;
                                    for (int lastColorCode = (*(lastSortIndex - 1)).Value, nextColorCode = (*nextSortIndex).Value; lastSortIndex != nextSortIndex; ++lastSortIndex)
                                    {
                                        colorIndexs[*(colorFixed + (*lastSortIndex).Index)] = (*lastSortIndex).Value - lastColorCode <= nextColorCode - (*lastSortIndex).Value ? currentColorIndex : (currentColorIndex + 1);
                                    }
                                    if (lastSortIndex != endSortIndex)
                                    {
                                        if (*(colorCountFixed + (*lastSortIndex).Index) == minColorCount && --minColorCounts == 0) ++minColorCount;
                                        outputColor = *(colorFixed + (*lastSortIndex).Index);
                                        *currentBuffer++ = outputColor.Red;
                                        *currentBuffer++ = outputColor.Blue;
                                        *currentBuffer++ = outputColor.Green;
                                        colorIndexs[outputColor] = ++currentColorIndex;
                                    }
                                }
                                outputColor = *(colorFixed + (*lastSortIndex).Index);
                                *currentBuffer++ = outputColor.Red;
                                *currentBuffer++ = outputColor.Blue;
                                *currentBuffer++ = outputColor.Green;
                                *currentBuffer = (byte)pixel;
                                if (!checkBuffer(fileBufferFixed, (maxColorCount << 1) + maxColorCount + 1)) return false;
                            }
                        }
                        finally { pool.Push(ref buffer); }
                    }
                    byte* colorIndexFixed = (byte*)colorCountFixed;
                    if (isInterlace)
                    {
                        TmphColor* colorEnd = colorFixed + width * height;
                        int inputSpace = (width << 3) - width;
                        for (TmphColor* inputColor = colorFixed; inputColor < colorEnd; inputColor += inputSpace)
                        {
                            for (TmphColor* inputEnd = inputColor + width; inputColor != inputEnd; *colorIndexFixed++ = (byte)colorIndexs[*inputColor++]) ;
                        }
                        for (TmphColor* inputColor = colorFixed + (width << 2); inputColor < colorEnd; inputColor += inputSpace)
                        {
                            for (TmphColor* inputEnd = inputColor + width; inputColor != inputEnd; *colorIndexFixed++ = (byte)colorIndexs[*inputColor++]) ;
                        }
                        inputSpace -= width << 2;
                        for (TmphColor* inputColor = colorFixed + (width << 1); inputColor < colorEnd; inputColor += inputSpace)
                        {
                            for (TmphColor* inputEnd = inputColor + width; inputColor != inputEnd; *colorIndexFixed++ = (byte)colorIndexs[*inputColor++]) ;
                        }
                        for (TmphColor* inputColor = colorFixed + width; inputColor < colorEnd; inputColor += width)
                        {
                            for (TmphColor* inputEnd = inputColor + width; inputColor != inputEnd; *colorIndexFixed++ = (byte)colorIndexs[*inputColor++]) ;
                        }
                    }
                    else
                    {
                        for (TmphColor* inputColor = colorFixed, inputEnd = colorFixed + width * height; inputColor != inputEnd; *colorIndexFixed++ = (byte)colorIndexs[*inputColor++]) ;
                    }
                    return lzwEncode((byte*)colorCountFixed, colorIndexFixed, pixel);
                }
            }

            /// <summary>
            /// LZW压缩编码
            /// </summary>
            /// <param name="inputFixed">输入数据</param>
            /// <param name="outputFixed">输出数据缓冲</param>
            /// <param name="size">编码长度</param>
            /// <returns>LZW压缩编码输出是否成功</returns>
            private unsafe bool lzwEncode(byte* inputFixed, byte* outputFixed, int size)
            {
                TmphPointer lzwEncodeTable = lzwEncodeTableBuffer.Get();
                try
                {
                    ulong tableClearIndex = (ulong)1 << size, outputValue = tableClearIndex;
                    byte* currentOutput = outputFixed;
                    int tableSize = (int)size + 1;
                    short clearIndex = (short)tableClearIndex, nextIndex = clearIndex;
                    tableClearIndex |= tableClearIndex << 16;
                    tableClearIndex |= tableClearIndex << 32;
                    Laurent.Lee.CLB.Unsafe.TmphMemory.Fill(lzwEncodeTable.Data, tableClearIndex, ((4096 * 2) / sizeof(ulong)) << size);
                    int outputSize = tableSize;
                    if (size == 1) ++outputSize;
                    int outputStart = outputSize, nextClearIndex = 1 << outputSize;
                    nextIndex += 2;
                    short prefixIndex = *inputFixed;
                    for (byte* currentInput = inputFixed; ++currentInput != outputFixed;)
                    {
                        byte* currentTable = lzwEncodeTable.Byte + (prefixIndex << tableSize) + (*currentInput << 1);
                        if (*(short*)currentTable == clearIndex)
                        {
                            outputValue |= (ulong)(uint)(int)prefixIndex << outputStart;
                            if ((outputStart += outputSize) >= sizeof(ulong) << 3)
                            {
                                *(ulong*)currentOutput = outputValue;
                                outputStart -= sizeof(ulong) << 3;
                                currentOutput += sizeof(ulong);
                                outputValue = (uint)(int)prefixIndex >> (outputSize - outputStart);
                            }
                            if (nextIndex == nextClearIndex)
                            {
                                *(short*)currentTable = nextIndex++;
                                ++outputSize;
                                nextClearIndex <<= 1;
                            }
                            else if (nextIndex == 4095)
                            {
                                outputValue |= (ulong)(uint)(int)clearIndex << outputStart;
                                if ((outputStart += 12) >= sizeof(ulong) << 3)
                                {
                                    *(ulong*)currentOutput = outputValue;
                                    outputStart -= sizeof(ulong) << 3;
                                    currentOutput += sizeof(ulong);
                                    outputValue = (uint)(int)clearIndex >> (12 - outputStart);
                                }
                                Laurent.Lee.CLB.Unsafe.TmphMemory.Fill(lzwEncodeTable.Data, tableClearIndex, ((4096 * 2) / sizeof(ulong)) << size);
                                outputSize = tableSize;
                                if (size == 1) ++outputSize;
                                nextClearIndex = 1 << outputSize;
                                nextIndex = clearIndex;
                                nextIndex += 2;
                            }
                            else *(short*)currentTable = nextIndex++;
                            prefixIndex = *currentInput;
                        }
                        else prefixIndex = *(short*)currentTable;
                    }
                    outputValue |= (ulong)(uint)(int)prefixIndex << outputStart;
                    if ((outputStart += outputSize) >= sizeof(ulong) << 3)
                    {
                        *(ulong*)currentOutput = outputValue;
                        outputStart -= sizeof(ulong) << 3;
                        currentOutput += sizeof(ulong);
                        outputValue = (uint)(int)prefixIndex >> (outputSize - outputStart);
                    }
                    outputValue |= (ulong)(uint)(int)++clearIndex << outputStart;
                    if ((outputStart += outputSize) >= sizeof(ulong) << 3)
                    {
                        *(ulong*)currentOutput = outputValue;
                        outputStart -= sizeof(ulong) << 3;
                        currentOutput += sizeof(ulong);
                        outputValue = (uint)(int)clearIndex >> (outputSize - outputStart);
                    }
                    if (outputStart != 0)
                    {
                        *(ulong*)currentOutput = outputValue;
                        currentOutput += (outputStart + 7) >> 3;
                    }
                    fixed (byte* bufferFixed = fileBuffer) return addBlocks(bufferFixed, outputFixed, currentOutput);
                }
                finally { lzwEncodeTableBuffer.Push(ref lzwEncodeTable); }
            }

            /// <summary>
            /// 添加数据块
            /// </summary>
            /// <param name="bufferFixed">文件缓存</param>
            /// <param name="outputFixed">输出数据起始位置</param>
            /// <param name="outputEnd">输出数据结束位置</param>
            /// <returns>数据块添加是否成功</returns>
            private unsafe bool addBlocks(byte* bufferFixed, byte* outputFixed, byte* outputEnd)
            {
                for (outputEnd -= 255 * 3; outputFixed <= outputEnd; outputFixed += 255 * 3)
                {
                    byte* currentBuffer = bufferFixed + bufferIndex;
                    *currentBuffer = 255;
                    Laurent.Lee.CLB.Unsafe.TmphMemory.Copy(outputFixed, currentBuffer + 1, 255);
                    *(currentBuffer + 256) = 255;
                    Laurent.Lee.CLB.Unsafe.TmphMemory.Copy(outputFixed + 255, currentBuffer + 257, 255);
                    *(currentBuffer + 512) = 255;
                    Laurent.Lee.CLB.Unsafe.TmphMemory.Copy(outputFixed + 255 * 2, currentBuffer + 513, 255);
                    if (!checkBuffer(bufferFixed, 256 * 3)) return false;
                }
                for (outputEnd += 255 * 2; outputFixed <= outputEnd; outputFixed += 255)
                {
                    byte* currentBuffer = bufferFixed + bufferIndex;
                    *currentBuffer = 255;
                    Laurent.Lee.CLB.Unsafe.TmphMemory.Copy(outputFixed, currentBuffer + 1, 255);
                    bufferIndex += 256;
                }
                int outputLength = (int)(outputEnd + 255 - outputFixed);
                if (outputLength != 0)
                {
                    byte* currentBuffer = bufferFixed + bufferIndex;
                    *currentBuffer = (byte)outputLength;
                    Laurent.Lee.CLB.Unsafe.TmphMemory.Copy(outputFixed, currentBuffer + 1, outputLength);
                    bufferIndex += outputLength + 1;
                }
                *(bufferFixed + bufferIndex++) = 0;
                return checkBuffer(bufferFixed);
            }

            /// <summary>
            /// 添加数据块
            /// </summary>
            /// <param name="bufferFixed">文件缓存</param>
            /// <param name="text">文本数据</param>
            /// <returns>数据块添加是否成功</returns>
            private unsafe bool addBlocks(byte* bufferFixed, string text)
            {
                fixed (char* textFixed = text)
                {
                    char* outputFixed = textFixed, outputEnd = outputFixed + text.Length - 255;
                    while (outputFixed <= outputEnd)
                    {
                        byte* currentBuffer = bufferFixed + bufferIndex;
                        *currentBuffer = 255;
                        for (char* nextOutput = outputFixed + 255; outputFixed != nextOutput; ++outputFixed)
                        {
                            *++currentBuffer = *(byte*)outputFixed;
                        }
                        if (!checkBuffer(bufferFixed, 256)) return false;
                    }
                    int outputLength = (int)((outputEnd += 255) - outputFixed);
                    if (outputLength != 0)
                    {
                        byte* currentBuffer = bufferFixed + bufferIndex;
                        for (*currentBuffer = (byte)outputLength; outputFixed != outputEnd; ++outputFixed)
                        {
                            *++currentBuffer = *(byte*)outputFixed;
                        }
                        bufferIndex += outputLength + 1;
                    }
                    *(bufferFixed + bufferIndex++) = 0;
                    return checkBuffer(bufferFixed);
                }
            }

            /// <summary>
            /// 添加图形控制扩展
            /// </summary>
            /// <param name="delayTime">延迟时间，单位1/100秒</param>
            /// <param name="method">图形处置方法</param>
            /// <param name="isUseInput">用户输入标志，指出是否期待用户有输入之后才继续进行下去，置位表示期待，值否表示不期待。</param>
            /// <returns>图形控制扩展是否添加成功</returns>
            public unsafe bool AddGraphicControl(short delayTime, TmphGraphicControl.TmphMethodType method = TmphGraphicControl.TmphMethodType.None
                , bool isUseInput = false)
            {
                if (fileBuffer == null) return false;
                if (delayTime <= 0) delayTime = 1;
                fixed (byte* bufferFixed = fileBuffer)
                {
                    byte* currentBuffer = bufferFixed + bufferIndex;
                    *(int*)currentBuffer = 0x4f921 | ((int)method << 26) | (isUseInput ? (0x2000000) : 0);
                    *(int*)(currentBuffer + 4) = delayTime <= 0 ? 1 : (int)delayTime;
                    return checkBuffer(bufferFixed, 8);
                }
            }

            /// <summary>
            /// 添加图形文本扩展
            /// </summary>
            /// <param name="text">文本数据</param>
            /// <param name="left">文本框离逻辑屏幕的左边界距离</param>
            /// <param name="top">文本框离逻辑屏幕的上边界距离</param>
            /// <param name="width">文本框像素宽度</param>
            /// <param name="height">文本框像素高度</param>
            /// <param name="colorIndex">前景色在全局颜色列表中的索引</param>
            /// <param name="blackgroundColorIndex">背景色在全局颜色列表中的索引</param>
            /// <param name="characterWidth">字符宽度</param>
            /// <param name="characterHeight">字符高度</param>
            /// <returns>图形文本扩展是否添加成功</returns>
            public unsafe bool AddPlainText(string text, short left, short top, short width, short height
                , byte colorIndex, byte blackgroundColorIndex, byte characterWidth, byte characterHeight)
            {
                if (text.Length == 0) return false;
                if (left + width <= Width || left >= Width || top >= Height || top + height <= Height) return false;
                if (colorIndex >= globalColorCount || blackgroundColorIndex >= globalColorCount) return false;
                if (characterWidth == 0 || characterHeight == 0) return false;
                fixed (byte* bufferFixed = fileBuffer)
                {
                    byte* currentBuffer = bufferFixed + bufferIndex;
                    *(short*)currentBuffer = 0x121;
                    *(currentBuffer + 2) = 12;
                    *(short*)(currentBuffer + 3) = left;
                    *(short*)(currentBuffer + 5) = top;
                    *(short*)(currentBuffer + 7) = width;
                    *(short*)(currentBuffer + 9) = height;
                    *(currentBuffer + 11) = characterWidth;
                    *(currentBuffer + 12) = characterHeight;
                    *(currentBuffer + 13) = colorIndex;
                    *(currentBuffer + 14) = blackgroundColorIndex;
                    return checkBuffer(bufferFixed, 15) && addBlocks(bufferFixed, text);
                }
            }

            /// <summary>
            /// 添加注释扩展
            /// </summary>
            /// <param name="comment">注释内容</param>
            /// <returns>注释扩展是否添加成功</returns>
            public unsafe bool AddComment(byte[] comment)
            {
                if (fileBuffer == null || comment.length() == 0) return false;
                fixed (byte* bufferFixed = fileBuffer)
                {
                    *(ushort*)(bufferFixed + bufferIndex) = 0xfe21;
                    if (!checkBuffer(bufferFixed, 2)) return false;
                    fixed (byte* commentFixed = comment) return addBlocks(bufferFixed, commentFixed, commentFixed + comment.Length);
                }
            }

            /// <summary>
            /// 添加注释扩展
            /// </summary>
            /// <param name="comment">注释内容</param>
            /// <returns>注释扩展是否添加成功</returns>
            public unsafe bool AddComment(string comment)
            {
                if (fileBuffer == null || comment.Length == 0) return false;
                fixed (byte* bufferFixed = fileBuffer)
                {
                    *(ushort*)(bufferFixed + bufferIndex) = 0xfe21;
                    return checkBuffer(bufferFixed, 2) && addBlocks(bufferFixed, comment);
                }
            }

            /// <summary>
            /// 添加应用程序扩展
            /// </summary>
            /// <param name="identifier">用来鉴别应用程序自身的标识(8个连续ASCII字符)</param>
            /// <param name="authenticationCode">应用程序定义的特殊标识码(3个连续ASCII字符)</param>
            /// <param name="customData">应用程序自定义数据块集合</param>
            /// <returns>应用程序扩展是否添加成功</returns>
            public unsafe bool AddApplication(byte[] identifier, byte[] authenticationCode, byte[] customData)
            {
                if (((identifier.length() ^ 8) | (authenticationCode.length() ^ 3)) != 0) return false;
                fixed (byte* bufferFixed = fileBuffer)
                {
                    byte* currentBuffer = bufferFixed + bufferIndex;
                    *(ushort*)currentBuffer = 0xff21;
                    *(currentBuffer + 2) = 11;
                    fixed (byte* identifierFixed = identifier) *(ulong*)(currentBuffer + 3) = *(ulong*)identifierFixed;
                    fixed (byte* authenticationCodeFixed = authenticationCode) *(int*)(currentBuffer + 11) = *(int*)authenticationCodeFixed;
                    if (customData.length() == 0)
                    {
                        *(currentBuffer + 14) = 0;
                        return checkBuffer(bufferFixed, 15);
                    }
                    else
                    {
                        if (!checkBuffer(bufferFixed, 14)) return false;
                        fixed (byte* customDataFixed = customData) return addBlocks(bufferFixed, customDataFixed, customDataFixed + customData.Length);
                    }
                }
            }
        }

        /// <summary>
        /// 版本号
        /// </summary>
        public TmphSubArray<byte> Version { get; private set; }

        /// <summary>
        /// 素数宽度
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// 素数高度
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// 颜色深度
        /// </summary>
        private byte colorResoluTion;

        /// <summary>
        /// 全局颜色列表是否分类排列
        /// </summary>
        private byte sortFlag;

        /// <summary>
        /// 全局颜色列表
        /// </summary>
        public TmphColor[] GlobalColors { get; private set; }

        /// <summary>
        /// 背景颜色在全局颜色列表中的索引，如果没有全局颜色列表，该值没有意义
        /// </summary>
        private byte backgroundColorIndex;

        /// <summary>
        /// 像素宽高比
        /// </summary>
        private byte pixelAspectRadio;

        /// <summary>
        /// 数据块集合
        /// </summary>
        private TmphList<TmphDataBlock> blocks;

        /// <summary>
        /// 数据块集合
        /// </summary>
        public TmphDataBlock[] Blocks
        {
            get { return blocks.getArray(); }
        }

        /// <summary>
        /// GIF文件是否解析成功
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// GIF文件
        /// </summary>
        /// <param name="data">GIF文件数据</param>
        private unsafe TmphFile(byte[] data)
        {
            fixed (byte* dataFixed = data)
            {
                if ((*(int*)dataFixed & 0xffffff) == ('G' | ('I' << 8) | ('F' << 16)))
                {
                    Width = *(short*)(dataFixed + 6);
                    Height = *(short*)(dataFixed + 8);
                    byte globalFlag = *(dataFixed + 10);
                    backgroundColorIndex = *(dataFixed + 11);
                    pixelAspectRadio = *(dataFixed + 12);
                    colorResoluTion = (byte)(((globalFlag >> 4) & 7) + 1);
                    sortFlag = (byte)(globalFlag & 8);
                    byte* currentData = dataFixed + 6 + 7;
                    if ((globalFlag & 0x80) != 0)
                    {
                        int colorCount = 1 << ((globalFlag & 7) + 1);
                        if (data.Length < 14 + (colorCount << 1) + colorCount) return;
                        currentData = TmphFile.TmphDecoder.FillColor(GlobalColors = new TmphColor[colorCount], currentData);
                    }
                    TmphDecoder TmphDecoder = new TmphDecoder(data, dataFixed, currentData);
                    blocks = new TmphList<TmphDataBlock>();
                    while (!TmphDecoder.IsFileEnd)
                    {
                        TmphDataBlock block = TmphDecoder.Next();
                        if (block == null) return;
                        blocks.Add(block);
                    }
                    IsCompleted = true;
                }
            }
        }

        /// <summary>
        /// GIF文件
        /// </summary>
        /// <param name="data">GIF文件数据</param>
        /// <returns>GIF文件,失败返回null</returns>
        public static TmphFile Create(byte[] data)
        {
            if (data.length() > 3 + 3 + 7 + 1)
            {
                TmphFile file = new TmphFile(data);
                if (file.IsCompleted) return file;
            }
            return null;
        }

        /// <summary>
        /// GIF文件
        /// </summary>
        /// <param name="filename">GIF文件名</param>
        /// <returns>GIF文件,失败返回null</returns>
        public static TmphFile Create(string filename)
        {
            return File.Exists(filename) ? Create(File.ReadAllBytes(filename)) : null;
        }
    }
}