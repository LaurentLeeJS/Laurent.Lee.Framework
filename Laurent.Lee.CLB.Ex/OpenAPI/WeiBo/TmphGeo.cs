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

namespace Laurent.Lee.CLB.OpenAPI.WeiBo
{
    /// <summary>
    /// 地理信息
    /// </summary>
    public sealed class TmphGeo
    {
        /// <summary>
        /// 经度坐标
        /// </summary>
        public string longitude;

        /// <summary>
        /// 维度坐标
        /// </summary>
        public string latitude;

        /// <summary>
        /// 所在城市的城市代码
        /// </summary>
        public string city;

        /// <summary>
        /// 所在省份的省份代码
        /// </summary>
        public string province;

        /// <summary>
        /// 所在城市的城市名称
        /// </summary>
        public string city_name;

        /// <summary>
        /// 所在省份的省份名称
        /// </summary>
        public string province_name;

        /// <summary>
        /// 所在的实际地址，可以为空
        /// </summary>
        public string address;

        /// <summary>
        /// 地址的汉语拼音，不是所有情况都会返回该字段
        /// </summary>
        public string pinyin;

        /// <summary>
        /// 更多信息，不是所有情况都会返回该字段
        /// </summary>
        public string more;
    }
}