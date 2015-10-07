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
using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Laurent.Lee.WLB
{
    public class TmphBasePage : System.Web.UI.Page
    {
        public TmphBasePage()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }

        public static string Title = "标题";
        public static string keywords = "关键字";
        public static string description = "网站描述";

        protected override void OnInit(EventArgs e)
        {
            if (Session["admin"] == null || Session["admin"].ToString().Trim() == "")
            {
                Response.Redirect("login.aspx");
            }
            base.OnInit(e);
        }

        protected void ExportData(string strContent, string FileName)
        {
            FileName = FileName + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();

            Response.Clear();
            Response.Charset = "gb2312";
            Response.ContentType = "application/ms-excel";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            //this.Page.EnableViewState = false;
            // 添加头信息，为"文件下载/另存为"对话框指定默认文件名
            Response.AddHeader("Content-Disposition", "attachment; filename=" + FileName + ".xls");
            // 把文件流发送到客户端
            Response.Write("<html><head><meta http-equiv=Content-Type content=\"text/html; charset=utf-8\">");
            Response.Write(strContent);
            Response.Write("</body></html>");
            // 停止页面的执行
            //Response.End();
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="obj"></param>
        public void ExportData(GridView obj)
        {
            try
            {
                string style = "";
                if (obj.Rows.Count > 0)
                {
                    style = @"<style> .text { mso-number-format:\@; } </script> ";
                }
                else
                {
                    style = "no data.";
                }

                Response.ClearContent();
                DateTime dt = DateTime.Now;
                string filename = dt.Year.ToString() + dt.Month.ToString() + dt.Day.ToString() + dt.Hour.ToString() + dt.Minute.ToString() + dt.Second.ToString();
                Response.AddHeader("content-disposition", "attachment; filename=ExportData" + filename + ".xls");
                Response.ContentType = "application/ms-excel";
                Response.Charset = "GB2312";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);
                obj.RenderControl(htw);
                Response.Write(style);
                Response.Write(sw.ToString());
                Response.End();
            }
            catch
            {
            }
        }
    }
}