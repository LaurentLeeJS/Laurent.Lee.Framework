using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Laurent.Lee.CLB.Drawing
{
    /// <summary>
    /// 缩略图
    /// </summary>
    public static class TmphThumbnail
    {
        /// <summary>
        /// 高质量图像编码参数
        /// </summary>
        private static readonly EncoderParameters qualityEncoder;
        /// <summary>
        /// 图像编码解码器集合
        /// </summary>
        private static readonly CLB.TmphStateSearcher.TmphByteArray<ImageCodecInfo> imageCodecs;
        /// <summary>
        /// JPEG图像编码解码器
        /// </summary>
        private static readonly ImageCodecInfo jpegImageCodecInfo;
        /// <summary>
        /// 获取图像编码解码器
        /// </summary>
        /// <param name="format">图像文件格式</param>
        /// <returns>图像编码解码器</returns>
        private unsafe static ImageCodecInfo getImageCodec(ImageFormat format)
        {
            if (format != null)
            {
                TmphGuid guid = new TmphGuid { Value = format.Guid };
                return imageCodecs.Get(&guid, 16);
            }
            return null;
        }
        /// <summary>
        /// 图像缩略切剪
        /// </summary>
        /// <param name="data">图像文件数据</param>
        /// <param name="width">缩略宽度,0表示与高度同比例</param>
        /// <param name="height">缩略高度,0表示与宽度同比例</param>
        /// <param name="type">目标图像文件格式</param>
        /// <returns>图像缩略文件数据</returns>
        public static TmphSubArray<byte> Cut(byte[] data, int width, int height, ImageFormat type, TmphMemoryPool memoryPool = null, int seek = 0)
        {
            if (data == null) return default(TmphSubArray<byte>);
            return Cut(TmphSubArray<byte>.Unsafe(data, 0, data.Length), width, height, type, memoryPool, seek);
        }
        /// <summary>
        /// 图像缩略切剪
        /// </summary>
        /// <param name="data">图像文件数据</param>
        /// <param name="width">缩略宽度,0表示与高度同比例</param>
        /// <param name="height">缩略高度,0表示与宽度同比例</param>
        /// <param name="type">目标图像文件格式</param>
        /// <returns>图像缩略文件数据</returns>
        public static TmphSubArray<byte> Cut(TmphSubArray<byte> data, int width, int height, ImageFormat type, TmphMemoryPool TmphMemoryPool = null, int seek = 0)
        {
            if (data.Count != 0 && width > 0 && height > 0 && (width | height) != 0 && seek >= 0)
            {
                try
                {
                    using (MemoryStream memory = new MemoryStream(data.Array, data.StartIndex, data.Count))
                    {
                        TmphBuilder TmphBuilder = new TmphBuilder();
                        using (Image TmphImage = TmphBuilder.CreateImage(memory)) return TmphBuilder.Get(ref width, ref height, type, TmphMemoryPool, seek);
                    }
                }
                catch (Exception error)
                {
                    TmphLog.Error.Add(error, null, false);
                }
            }
            return default(TmphSubArray<byte>);
        }
        /// <summary>
        /// 缩略图创建器
        /// </summary>
        private struct TmphBuilder
        {
            /// <summary>
            /// 原始图片
            /// </summary>
            private Image TmphImage;
            /// <summary>
            /// 原始图片宽度
            /// </summary>
            private int width;
            /// <summary>
            /// 原始图片高度
            /// </summary>
            private int height;
            /// <summary>
            /// 原始图片裁剪横坐标起始位置
            /// </summary>
            private int left;
            /// <summary>
            /// 原始图片裁剪纵坐标起始位置
            /// </summary>
            private int top;
            /// <summary>
            /// 原始图片裁剪横坐标结束位置
            /// </summary>
            private int right;
            /// <summary>
            /// 原始图片裁剪纵坐标结束位置
            /// </summary>
            private int bottom;
            /// <summary>
            /// 根据数据流创建原始图片
            /// </summary>
            /// <param name="stream">数据流</param>
            /// <returns>原始图片</returns>
            public Image CreateImage(Stream stream)
            {
                TmphImage = Image.FromStream(stream);
                width = TmphImage.Width;
                height = TmphImage.Height;
                return TmphImage;
            }
            /// <summary>
            /// 计算缩略图尺寸位置
            /// </summary>
            /// <param name="width">缩略宽度,0表示与高度同比例</param>
            /// <param name="height">缩略高度,0表示与宽度同比例</param>
            /// <returns>是否需要生成缩略图</returns>
            private bool check(ref int width, ref int height)
            {
                if (width > 0)
                {
                    if (height > 0)
                    {
                        if ((long)width * this.height >= (long)height * this.width)
                        {
                            int value = (int)((long)height * this.width / width);
                            if (width > this.width)
                            {
                                if (value == 0) value = 1;
                                width = this.width;
                            }
                            left = 0;
                            top = (this.height - value) >> 1;
                            right = this.width;
                            bottom = top + value;
                        }
                        else
                        {
                            int value = (int)((long)width * this.height / height);
                            if (height > this.height)
                            {
                                if (value == 0) value = 1;
                                height = this.height;
                            }
                            left = (this.width - value) >> 1;
                            top = 0;
                            right = left + value;
                            bottom = this.height;
                        }
                        return true;
                    }
                    if (width < this.width)
                    {
                        left = top = 0;
                        right = this.width;
                        bottom = this.height;
                        if ((height = (int)((long)this.height * width / this.width)) == 0) height = 1;
                        return true;
                    }
                }
                else if (height < this.height)
                {
                    left = top = 0;
                    right = this.width;
                    bottom = this.height;
                    if ((width = (int)((long)this.width * height / this.height)) == 0) width = 1;
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 获取缩略图
            /// </summary>
            /// <param name="width">缩略宽度</param>
            /// <param name="height">缩略高度</param>
            /// <param name="type">目标图像文件格式</param>
            /// <returns>缩略图数据流</returns>
            public TmphSubArray<byte> Get(ref int width, ref int height, ImageFormat type, TmphMemoryPool TmphMemoryPool, int seek)
            {
                if (check(ref width, ref height))
                {
                    if (TmphMemoryPool == null)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            if (seek != 0) stream.Seek(seek, SeekOrigin.Begin);
                            get(stream, width, height, type);
                            return TmphSubArray<byte>.Unsafe(stream.GetBuffer(), seek, (int)stream.Position - seek);
                        }
                    }
                    byte[] buffer = TmphMemoryPool.Get();
                    try
                    {
#if MONO
                            using (MemoryStream stream = memoryStream.Get(buffer))
#else
                        using (MemoryStream stream = TmphMemoryStreamProxy.Get(buffer))
#endif
                        {
                            if (seek != 0) stream.Seek(seek, SeekOrigin.Begin);
                            get(stream, width, height, type);
                            byte[] data = stream.GetBuffer();
                            if (buffer == data)
                            {
                                buffer = null;
                                //showjim
                                if ((int)stream.Position > data.Length)
                                {
                                    TmphLog.Error.Add("Position " + ((int)stream.Position).toString() + " > " + data.Length.toString(), true, false);
                                }
                                return TmphSubArray<byte>.Unsafe(data, seek, (int)stream.Position - seek);
                            }
                            return TmphSubArray<byte>.Unsafe(data, seek, (int)stream.Position - seek);
                        }
                    }
                    finally { TmphMemoryPool.Push(ref buffer); }
                }
                return default(TmphSubArray<byte>);
            }
            /// <summary>
            /// 获取缩略图
            /// </summary>
            /// <param name="stream">输出数据流</param>
            /// <param name="width">缩略宽度</param>
            /// <param name="height">缩略高度</param>
            /// <param name="type">目标图像文件格式</param>
            private void get(Stream stream, int width, int height, ImageFormat type)
            {
                using (Bitmap bitmap = new Bitmap(width, height))
                using (Graphics graphic = Graphics.FromImage(bitmap))
                {
                    graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphic.SmoothingMode = SmoothingMode.HighQuality;
                    graphic.CompositingQuality = CompositingQuality.HighQuality;
                    graphic.DrawImage(TmphImage, new Rectangle(0, 0, width, height), new Rectangle(left, top, right - left, bottom - top), GraphicsUnit.Pixel);
                    bitmap.Save(stream, getImageCodec(type ?? ImageFormat.Jpeg) ?? jpegImageCodecInfo, qualityEncoder);
                }
            }
        }
        unsafe static TmphThumbnail()
        {
            (qualityEncoder = new EncoderParameters(1)).Param[0] = new EncoderParameter(Encoder.Quality, 100L);
            ImageCodecInfo[] infos = ImageCodecInfo.GetImageDecoders();
            imageCodecs = new CLB.TmphStateSearcher.TmphByteArray<ImageCodecInfo>(infos.getArray(value => CLB.TmphGuid.ToByteArray(value.FormatID)), infos);
            TmphGuid guid = new TmphGuid { Value = ImageFormat.Jpeg.Guid };
            jpegImageCodecInfo = imageCodecs.Get(&guid, 16);
        }
    }
}
