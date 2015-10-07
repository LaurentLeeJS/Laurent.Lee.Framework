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
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Laurent.Lee.WLB
{
    public class TmphPageHelper
    {
        #region 控件状态设置

        /// <summary>
        /// 锁定页面上的一些组件
        /// </summary>
        /// <param name="page"></param>
        /// <param name="obj">不需锁定的控件</param>
        public static void LockPage(Page page, object[] obj)
        {
            Control htmlForm = null;
            foreach (Control ctl in page.Controls)
            {
                if (ctl is HtmlForm)
                {
                    htmlForm = ctl;
                    break;
                }
            }
            //foreach (Control ctl in page.Controls[1].Controls)
            foreach (Control ctl in htmlForm.Controls)
            {
                if (IsContains(obj, ctl) == false)
                {
                    //锁定
                    LockControl(page, ctl);
                }
                else
                {
                    //解除锁定
                    UnLockControl(page, ctl);
                }
            }
        }

        /// <summary>
        /// 解除锁定页面上的一些组件
        /// </summary>
        /// <param name="page"></param>
        /// <param name="obj">继续保持锁定的控件</param>
        public static void UnLockPage(Page page, object[] obj)
        {
            Control htmlForm = null;
            foreach (Control ctl in page.Controls)
            {
                if (ctl is HtmlForm)
                {
                    htmlForm = ctl;
                    break;
                }
            }
            //foreach (Control ctl in page.Controls[1].Controls)
            foreach (Control ctl in htmlForm.Controls)
            {
                if (IsContains(obj, ctl) == false)
                {
                    //解除锁定
                    UnLockControl(page, ctl);
                }
                else
                {
                    //锁定
                    LockControl(page, ctl);
                }
            }
        }

        /// <summary>
        /// 禁用控件
        /// </summary>
        /// <param name="page"></param>
        /// <param name="ctl"></param>
        private static void LockControl(Page page, Control ctl)
        {
            //WebControl
            if (ctl is Button || ctl is CheckBox || ctl is HyperLink || ctl is LinkButton
                || ctl is ListControl || ctl is TextBox)
            {
                ((WebControl)ctl).Enabled = false;

                #region 多行文本框不能禁用，应设为只读，不然滚动条不能使用

                if (ctl is TextBox)
                {
                    if (((TextBox)ctl).TextMode == TextBoxMode.MultiLine)
                    {
                        ((TextBox)ctl).Enabled = true;
                        ((TextBox)ctl).ReadOnly = true;
                    }
                }

                #endregion 多行文本框不能禁用，应设为只读，不然滚动条不能使用
            }

            //HtmlControl
            if (ctl is HtmlInputFile)
            {
                ((HtmlInputFile)ctl).Disabled = true;
            }
        }

        /// <summary>
        /// 开放控件
        /// </summary>
        /// <param name="page"></param>
        /// <param name="ctl"></param>
        private static void UnLockControl(Page page, Control ctl)
        {
            //WebControl
            if (ctl is Button || ctl is CheckBox || ctl is HyperLink || ctl is LinkButton
                || ctl is ListControl || ctl is TextBox)
            {
                ((WebControl)ctl).Enabled = true;

                //文本框去掉只读属性
                if (ctl is TextBox)
                {
                    ((TextBox)ctl).ReadOnly = false;
                }

                ////时间输入文本框不禁用时显示按钮
                //if (ctl is WebDateTimeEdit)
                //{
                //    ((WebDateTimeEdit)ctl).SpinButtons.Display = ButtonDisplay.OnRight;
                //}

                ////时间选择文本框不禁用时显示按钮
                //if (ctl is WebDateChooser)
                //{
                //    page.ClientScript.RegisterStartupScript(typeof(string), "Display" + ctl.ClientID + "Image", "<script language=javascript>" +
                //        "document.getElementById('" + ctl.ClientID + "_img" + "').style.display='';</script>");
                //}
            }

            //HtmlControl
            if (ctl is HtmlInputFile)
            {
                ((HtmlInputFile)ctl).Disabled = false;
            }
        }

        /// <summary>
        /// 数组中是否包含当前控件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ctl"></param>
        /// <returns></returns>
        private static bool IsContains(object[] obj, Control ctl)
        {
            foreach (Control c in obj)
            {
                if (c.ID == ctl.ID)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion 控件状态设置

        #region 页面处理其它辅助方法

        /// <summary>
        /// 得到当前页对象实例
        /// </summary>
        /// <returns></returns>
        public static Page GetCurrentPage()
        {
            return (Page)HttpContext.Current.Handler;
        }

        /// <summary>
        /// 从System.Web.HttpRequest的Url中获取所调用的页面名称
        /// </summary>
        /// <returns>页面名称</returns>
        public static string GetPageName()
        {
            int start = 0;
            int end = 0;
            string Url = HttpContext.Current.Request.RawUrl;
            start = Url.LastIndexOf("/") + 1;
            end = Url.IndexOf("?");
            if (end <= 0)
            {
                return Url.Substring(start, Url.Length - start);
            }
            else
            {
                return Url.Substring(start, end - start);
            }
        }

        /// <summary>
        /// 读取QueryString值
        /// </summary>
        /// <param name="queryStringName">QueryString名称</param>
        /// <returns>QueryString值</returns>
        public static string GetQueryString(string queryStringName)
        {
            if ((HttpContext.Current.Request.QueryString[queryStringName] != null) &&
                (HttpContext.Current.Request.QueryString[queryStringName] != "undefined"))
            {
                return HttpContext.Current.Request.QueryString[queryStringName].Trim();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 页面跳转
        /// </summary>
        /// <param name="url">URL地址</param>
        public void Redirect(string url)
        {
            Page page = GetCurrentPage();
            page.Response.Redirect(url);
        }

        /// <summary>
        /// 获取当前请求页面相对于根目录的层级
        /// </summary>
        /// <returns></returns>
        public static string GetRelativeLevel()
        {
            string ApplicationPath = HttpContext.Current.Request.ApplicationPath;
            if (ApplicationPath.Trim() == "/")
            {
                ApplicationPath = "";
            }

            int i = ApplicationPath == "" ? 1 : 2;
            return "";//Nandasoft.Helper.NDHelperString.Repeat("../", Nandasoft.Helper.NDHelperString.RepeatTime(HttpContext.Current.Request.Path, "/") - i);
        }

        /// <summary>
        /// 写javascript脚本
        /// </summary>
        /// <param name="script">脚本内容</param>
        public static void WriteScript(string script)
        {
            Page page = GetCurrentPage();

            // NDGridViewScriptFirst(page.Form.Controls, page);

            //ScriptManager.RegisterStartupScript(page, page.GetType(), System.Guid.NewGuid().ToString(), script, true);
        }

        //private void NDGridViewScriptFirst(ControlCollection ctls, Page page)
        //{
        //    foreach (Control ctl in ctls)
        //    {
        //        if (ctl is NDGridView)
        //        {
        //            NDGridView ndgv = (NDGridView)ctl;
        //            ScriptManager.RegisterStartupScript(page, page.GetType(), ndgv.ClientScriptKey, ndgv.ClientScriptName, true);
        //        }
        //        else
        //        {
        //            NDGridViewScriptFirst(ctl.Controls, page);
        //        }
        //    }
        //}

        /// <summary>
        /// 返回客户端浏览器版本
        /// 如果是IE类型，返回版本数字
        /// 如果不是IE类型，返回-1
        /// </summary>
        /// <returns>一位数字版本号</returns>
        public static int GetClientBrowserVersion()
        {
            string USER_AGENT = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];

            if (USER_AGENT.IndexOf("MSIE") < 0) return -1;

            string version = USER_AGENT.Substring(USER_AGENT.IndexOf("MSIE") + 5, 1);
            if (!TmphUtility.IsInt(version)) return -1;

            return Convert.ToInt32(version);
        }

        #endregion 页面处理其它辅助方法
    }
}