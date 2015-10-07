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

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// UpLoadFiles 的摘要说明
    /// </summary>
    public class TmphUpLoadFiles : System.Web.UI.Page
    {
        public TmphUpLoadFiles()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public string UploadFile(string filePath, int maxSize, string[] fileType, System.Web.UI.HtmlControls.HtmlInputFile TargetFile)
        {
            string Result = "UnDefine";
            bool typeFlag = false;
            string FilePath = filePath;
            int MaxSize = maxSize;
            string strFileName, strNewName, strFilePath;
            if (TargetFile.PostedFile.FileName == "")
            {
                return "FILE_ERR";
            }
            strFileName = TargetFile.PostedFile.FileName;
            TargetFile.Accept = "*/*";
            strFilePath = FilePath;
            if (Directory.Exists(strFilePath) == false)
            {
                Directory.CreateDirectory(strFilePath);
            }
            FileInfo myInfo = new FileInfo(strFileName);
            string strOldName = myInfo.Name;
            strNewName = strOldName.Substring(strOldName.LastIndexOf("."));
            strNewName = strNewName.ToLower();
            if (TargetFile.PostedFile.ContentLength <= MaxSize)
            {
                for (int i = 0; i <= fileType.GetUpperBound(0); i++)
                {
                    if (strNewName.ToLower() == fileType[i].ToString()) { typeFlag = true; break; }
                }
                if (typeFlag)
                {
                    string strFileNameTemp = GetUploadFileName();
                    string strFilePathTemp = strFilePath;
                    float strFileSize = TargetFile.PostedFile.ContentLength;
                    strOldName = strFileNameTemp + strNewName;
                    strFilePath = strFilePath + "\\" + strOldName;
                    TargetFile.PostedFile.SaveAs(strFilePath);
                    Result = strOldName + "|" + strFileSize;
                    TargetFile.Dispose();
                }
                else
                {
                    return "TYPE_ERR";
                }
            }
            else
            {
                return "SIZE_ERR";
            }
            return (Result);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="filePath">保存文件地址</param>
        /// <param name="maxSize">文件最大大小</param>
        /// <param name="fileType">文件后缀类型</param>
        /// <param name="TargetFile">控件名</param>
        /// <param name="saveFileName">保存后的文件名和地址</param>
        /// <param name="fileSize">文件大小</param>
        /// <returns></returns>
        public string UploadFile(string filePath, int maxSize, string[] fileType, System.Web.UI.HtmlControls.HtmlInputFile TargetFile, out string saveFileName, out int fileSize)
        {
            saveFileName = "";
            fileSize = 0;

            string Result = "";
            bool typeFlag = false;
            string FilePath = filePath;
            int MaxSize = maxSize;
            string strFileName, strNewName, strFilePath;
            if (TargetFile.PostedFile.FileName == "")
            {
                return "请选择上传的文件";
            }
            strFileName = TargetFile.PostedFile.FileName;
            TargetFile.Accept = "*/*";
            strFilePath = FilePath;
            if (Directory.Exists(strFilePath) == false)
            {
                Directory.CreateDirectory(strFilePath);
            }
            FileInfo myInfo = new FileInfo(strFileName);
            string strOldName = myInfo.Name;
            strNewName = strOldName.Substring(strOldName.LastIndexOf("."));
            strNewName = strNewName.ToLower();
            if (TargetFile.PostedFile.ContentLength <= MaxSize)
            {
                string strFileNameTemp = GetUploadFileName();
                string strFilePathTemp = strFilePath;
                strOldName = strFileNameTemp + strNewName;
                strFilePath = strFilePath + "\\" + strOldName;

                fileSize = TargetFile.PostedFile.ContentLength / 1024;
                saveFileName = strFilePath.Substring(strFilePath.IndexOf("FileUpload\\"));
                TargetFile.PostedFile.SaveAs(strFilePath);
                TargetFile.Dispose();
            }
            else
            {
                return "上传文件超出指定的大小";
            }
            return (Result);
        }

        public string UploadFile(string filePath, int maxSize, string[] fileType, string filename, System.Web.UI.HtmlControls.HtmlInputFile TargetFile)
        {
            string Result = "UnDefine";
            bool typeFlag = false;
            string FilePath = filePath;
            int MaxSize = maxSize;
            string strFileName, strNewName, strFilePath;
            if (TargetFile.PostedFile.FileName == "")
            {
                return "FILE_ERR";
            }
            strFileName = TargetFile.PostedFile.FileName;
            TargetFile.Accept = "*/*";
            strFilePath = FilePath;
            if (Directory.Exists(strFilePath) == false)
            {
                Directory.CreateDirectory(strFilePath);
            }
            FileInfo myInfo = new FileInfo(strFileName);
            string strOldName = myInfo.Name;
            strNewName = strOldName.Substring(strOldName.Length - 3, 3);
            strNewName = strNewName.ToLower();
            if (TargetFile.PostedFile.ContentLength <= MaxSize)
            {
                for (int i = 0; i <= fileType.GetUpperBound(0); i++)
                {
                    if (strNewName.ToLower() == fileType[i].ToString()) { typeFlag = true; break; }
                }
                if (typeFlag)
                {
                    string strFileNameTemp = filename;
                    string strFilePathTemp = strFilePath;
                    strOldName = strFileNameTemp + "." + strNewName;
                    strFilePath = strFilePath + "\\" + strOldName;
                    TargetFile.PostedFile.SaveAs(strFilePath);
                    Result = strOldName;
                    TargetFile.Dispose();
                }
                else
                {
                    return "TYPE_ERR";
                }
            }
            else
            {
                return "SIZE_ERR";
            }
            return (Result);
        }

        public string GetUploadFileName()
        {
            string Result = "";
            DateTime time = DateTime.Now;
            Result += time.Year.ToString() + FormatNum(time.Month.ToString(), 2) + FormatNum(time.Day.ToString(), 2) + FormatNum(time.Hour.ToString(), 2) + FormatNum(time.Minute.ToString(), 2) + FormatNum(time.Second.ToString(), 2) + FormatNum(time.Millisecond.ToString(), 3);
            return (Result);
        }

        public string FormatNum(string Num, int Bit)
        {
            int L;
            L = Num.Length;
            for (int i = L; i < Bit; i++)
            {
                Num = "0" + Num;
            }
            return (Num);
        }
    }
}