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
using System.CodeDom.Compiler;
using System.Reflection;

namespace Laurent.Lee.WLB.JS
{
    /// <summary>
    /// 用于执行JS的类
    /// </summary>
    public class TmphExeJsHelper
    {
        public object GetMainResult(string js, string mainname)
        {
            CodeDomProvider _provider = new Microsoft.JScript.JScriptCodeProvider();
            Type _evaluateType;
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            CompilerResults results = _provider.CompileAssemblyFromSource(parameters, js);
            Assembly assembly = results.CompiledAssembly;
            string time = "";

            _evaluateType = assembly.GetType("aa.JScript");
            object[] w = new object[] { "123", time };

            object ww = _evaluateType.InvokeMember("getm32str", BindingFlags.InvokeMethod,
            null, null, w);
            return js;
        }

        ///// <summary>
        ///// 密码加密
        ///// </summary>
        ///// <param name="pass"></param>
        ///// <returns></returns>
        //public string EncodePass(string pass)
        //{
        //    ScriptControlClass sc = new ScriptControlClass();
        //    sc.UseSafeSubset = true;
        //    sc.Language = "JScript";
        //    sc.AddCode(Properties.Resources.QQRsa);  //从资源中读取js内容,也可以写成Js文件神马的.
        //    string str = sc.Run("rsaEncrypt", new object[] { pass }).ToString();
        //    return str;
        //}
    }
}