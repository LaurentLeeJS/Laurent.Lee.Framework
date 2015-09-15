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

namespace Laurent.Lee.CLB.Net.Tcp.Http
{
    /// <summary>
    ///     索引位置
    /// </summary>
    public struct TmphBufferIndex
    {
        /// <summary>
        ///     长度
        /// </summary>
        public short Length;

        /// <summary>
        ///     起始位置
        /// </summary>
        public short StartIndex;

        /// <summary>
        ///     索引位置
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        public void Set(long startIndex, long length)
        {
            StartIndex = (short)startIndex;
            Length = (short)length;
        }

        /// <summary>
        ///     索引位置
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        public void Set(int startIndex, int length)
        {
            StartIndex = (short)startIndex;
            Length = (short)length;
        }

        /// <summary>
        ///     索引位置
        /// </summary>
        /// <param name="startIndex">起始位置</param>
        /// <param name="length">长度</param>
        public void Set(short startIndex, short length)
        {
            StartIndex = startIndex;
            Length = length;
        }

        /// <summary>
        ///     索引位置置空
        /// </summary>
        public void Null()
        {
            StartIndex = Length = 0;
        }
    }
}