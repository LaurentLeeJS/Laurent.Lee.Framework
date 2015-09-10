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

namespace Laurent.Lee.CLB
{
    /// <summary>
    ///     字符串连接(适应于较长的字符串链接,一个一个字符添加的请用unmanagedStream)
    /// </summary>
    public sealed class TmphStringBuilder : TmphList<string>
    {
        /// <summary>
        ///     初始化字符串连接
        /// </summary>
        public TmphStringBuilder()
        {
        }

        /// <summary>
        ///     初始化字符串连接
        /// </summary>
        /// <param name="length">字符串集合初始大小</param>
        public TmphStringBuilder(int length) : base(length)
        {
        }

        /// <summary>
        ///     初始化字符串连接
        /// </summary>
        /// <param name="value">字符串值</param>
        public TmphStringBuilder(string value)
        {
            Add(value);
        }

        /// <summary>
        ///     初始化字符串连接
        /// </summary>
        /// <param name="value">字符串集合</param>
        public TmphStringBuilder(params string[] value) : base(value, false)
        {
        }

        #region 连接字符串

        /// <summary>
        ///     连接字符串
        /// </summary>
        /// <param name="joinValue">字符串连接</param>
        /// <returns>连接后的字符串</returns>
        public string Join(string joinValue)
        {
            return joinValue == null ? ToString() : string.Join(joinValue, ToArray());
        }

        #endregion 连接字符串

        #region 生成字符串

        /// <summary>
        ///     生成字符串
        /// </summary>
        /// <returns>字符串</returns>
        public override string ToString()
        {
            return string.Concat(ToArray());
        }

        #endregion 生成字符串

        #region 添加字符串

        /// <summary>
        ///     添加字符
        /// </summary>
        /// <param name="value">字符值</param>
        public void Append(char value)
        {
            Add(new string(value, 1));
        }

        /// <summary>
        ///     添加字符
        /// </summary>
        /// <param name="value">字符值</param>
        /// <param name="count">字符数</param>
        public void Append(char value, int count)
        {
            if (count > 0) Add(new string(value, count));
        }

        /// <summary>
        ///     添加字符串
        /// </summary>
        /// <param name="values">字符串集合</param>
        /// <returns>当前字符串连接</returns>
        public void Append(params string[] values)
        {
            Add(values);
        }

        /// <summary>
        ///     添加字符串
        /// </summary>
        /// <param name="values">字符串集合</param>
        /// <returns>当前字符串连接</returns>
        public void AppendConcat(params string[] values)
        {
            Add(string.Concat(values));
        }

        #endregion 添加字符串
    }
}