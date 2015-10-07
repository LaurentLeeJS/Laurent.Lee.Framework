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

using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// 数据展示控件 绑定数据类
    /// </summary>
    public class TmphBindDataControl
    {
        #region 绑定服务器数据控件 简单绑定DataList

        /// <summary>
        /// 简单绑定DataList
        /// </summary>
        /// <param name="ctrl">控件ID</param>
        /// <param name="mydv">数据视图</param>
        public static void BindDataList(Control ctrl, DataView mydv)
        {
            ((DataList)ctrl).DataSourceID = null;
            ((DataList)ctrl).DataSource = mydv;
            ((DataList)ctrl).DataBind();
        }

        #endregion 绑定服务器数据控件 简单绑定DataList

        #region 绑定服务器数据控件 SqlDataReader简单绑定DataList

        /// <summary>
        /// SqlDataReader简单绑定DataList
        /// </summary>
        /// <param name="ctrl">控件ID</param>
        /// <param name="mydv">数据视图</param>
        public static void BindDataReaderList(Control ctrl, SqlDataReader mydv)
        {
            ((DataList)ctrl).DataSourceID = null;
            ((DataList)ctrl).DataSource = mydv;
            ((DataList)ctrl).DataBind();
        }

        #endregion 绑定服务器数据控件 SqlDataReader简单绑定DataList

        #region 绑定服务器数据控件 简单绑定GridView

        /// <summary>
        /// 简单绑定GridView
        /// </summary>
        /// <param name="ctrl">控件ID</param>
        /// <param name="mydv">数据视图</param>
        public static void BindGridView(Control ctrl, DataView mydv)
        {
            ((GridView)ctrl).DataSourceID = null;
            ((GridView)ctrl).DataSource = mydv;
            ((GridView)ctrl).DataBind();
        }

        #endregion 绑定服务器数据控件 简单绑定GridView

        /// <summary>
        /// 绑定服务器控件 简单绑定Repeater
        /// </summary>
        /// <param name="ctrl">控件ID</param>
        /// <param name="mydv">数据视图</param>
        public static void BindRepeater(Control ctrl, DataView mydv)
        {
            ((Repeater)ctrl).DataSourceID = null;
            ((Repeater)ctrl).DataSource = mydv;
            ((Repeater)ctrl).DataBind();
        }
    }
}