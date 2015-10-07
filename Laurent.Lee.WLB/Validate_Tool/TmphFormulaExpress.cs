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

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// EnumFormula
    /// </summary>
    public enum TmphEnumFormula
    {
        Add,//加号
        Dec,//减号
        Mul,//乘号
        Div,//除号
        Sin,//正玄
        Cos,//余玄
        Tan,//正切
        ATan,//余切
        Sqrt,//平方根
        Pow,//求幂
        None,//无
    }

    /// <summary>
    /// FormulaDeal
    /// </summary>
    public class TmphFormulaDeal
    {
        static TmphFormulaDeal()
        {

        }
        private double CalculateExpress(string strExpression)
        {
            string strTemp = "";
            string strTempB = "";
            string strOne = "";
            string strTwo = "";
            double ReplaceValue = 0;
            while (strExpression.IndexOf("+") != -1 || strExpression.IndexOf("-") != -1
            || strExpression.IndexOf("*") != -1 || strExpression.IndexOf("/") != -1)
            {
                if (strExpression.IndexOf("*") != -1)
                {
                    strTemp = strExpression.Substring(strExpression.IndexOf("*") + 1, strExpression.Length - strExpression.IndexOf("*") - 1);
                    strTempB = strExpression.Substring(0, strExpression.IndexOf("*"));
                    strOne = strTempB.Substring(GetPrivorPos(strTempB) + 1, strTempB.Length - GetPrivorPos(strTempB) - 1);

                    strTwo = strTemp.Substring(0, GetNextPos(strTemp));
                    ReplaceValue = Convert.ToDouble(GetExpType(strOne)) * Convert.ToDouble(GetExpType(strTwo));
                    strExpression = strExpression.Replace(strOne + "*" + strTwo, ReplaceValue.ToString());
                }
                else if (strExpression.IndexOf("/") != -1)
                {
                    strTemp = strExpression.Substring(strExpression.IndexOf("/") + 1, strExpression.Length - strExpression.IndexOf("/") - 1);
                    strTempB = strExpression.Substring(0, strExpression.IndexOf("/"));
                    strOne = strTempB.Substring(GetPrivorPos(strTempB) + 1, strTempB.Length - GetPrivorPos(strTempB) - 1);


                    strTwo = strTemp.Substring(0, GetNextPos(strTemp));
                    ReplaceValue = Convert.ToDouble(GetExpType(strOne)) / Convert.ToDouble(GetExpType(strTwo));
                    strExpression = strExpression.Replace(strOne + "/" + strTwo, ReplaceValue.ToString());
                }
                else if (strExpression.IndexOf("+") != -1)
                {
                    strTemp = strExpression.Substring(strExpression.IndexOf("+") + 1, strExpression.Length - strExpression.IndexOf("+") - 1);
                    strTempB = strExpression.Substring(0, strExpression.IndexOf("+"));
                    strOne = strTempB.Substring(GetPrivorPos(strTempB) + 1, strTempB.Length - GetPrivorPos(strTempB) - 1);

                    strTwo = strTemp.Substring(0, GetNextPos(strTemp));
                    ReplaceValue = Convert.ToDouble(GetExpType(strOne)) + Convert.ToDouble(GetExpType(strTwo));
                    strExpression = strExpression.Replace(strOne + "+" + strTwo, ReplaceValue.ToString());
                }
                else if (strExpression.IndexOf("-") != -1)
                {
                    strTemp = strExpression.Substring(strExpression.IndexOf("-") + 1, strExpression.Length - strExpression.IndexOf("-") - 1);
                    strTempB = strExpression.Substring(0, strExpression.IndexOf("-"));
                    strOne = strTempB.Substring(GetPrivorPos(strTempB) + 1, strTempB.Length - GetPrivorPos(strTempB) - 1);


                    strTwo = strTemp.Substring(0, GetNextPos(strTemp));
                    ReplaceValue = Convert.ToDouble(GetExpType(strOne)) - Convert.ToDouble(GetExpType(strTwo));
                    strExpression = strExpression.Replace(strOne + "-" + strTwo, ReplaceValue.ToString());
                }
            }
            return Convert.ToDouble(strExpression);
        }

        private double CalculateExExpress(string strExpression, TmphEnumFormula ExpressType)
        {
            double retValue = 0;
            switch (ExpressType)
            {
                case TmphEnumFormula.Sin:
                    retValue = Math.Sin(Convert.ToDouble(strExpression));
                    break;
                case TmphEnumFormula.Cos:
                    retValue = Math.Cos(Convert.ToDouble(strExpression));
                    break;
                case TmphEnumFormula.Tan:
                    retValue = Math.Tan(Convert.ToDouble(strExpression));
                    break;
                case TmphEnumFormula.ATan:
                    retValue = Math.Atan(Convert.ToDouble(strExpression));
                    break;
                case TmphEnumFormula.Sqrt:
                    retValue = Math.Sqrt(Convert.ToDouble(strExpression));
                    break;
                case TmphEnumFormula.Pow:
                    retValue = Math.Pow(Convert.ToDouble(strExpression), 2);
                    break;
            }
            if (retValue == 0) return Convert.ToDouble(strExpression);
            return retValue;
        }


        private int GetNextPos(string strExpression)
        {
            int[] ExpPos = new int[4];
            ExpPos[0] = strExpression.IndexOf("+");
            ExpPos[1] = strExpression.IndexOf("-");
            ExpPos[2] = strExpression.IndexOf("*");
            ExpPos[3] = strExpression.IndexOf("/");
            int tmpMin = strExpression.Length;
            for (int count = 1; count <= ExpPos.Length; count++)
            {
                if (tmpMin > ExpPos[count - 1] && ExpPos[count - 1] != -1)
                {
                    tmpMin = ExpPos[count - 1];
                }
            }
            return tmpMin;
        }


        private int GetPrivorPos(string strExpression)
        {
            int[] ExpPos = new int[4];
            ExpPos[0] = strExpression.LastIndexOf("+");
            ExpPos[1] = strExpression.LastIndexOf("-");
            ExpPos[2] = strExpression.LastIndexOf("*");
            ExpPos[3] = strExpression.LastIndexOf("/");
            int tmpMax = -1;
            for (int count = 1; count <= ExpPos.Length; count++)
            {
                if (tmpMax < ExpPos[count - 1] && ExpPos[count - 1] != -1)
                {
                    tmpMax = ExpPos[count - 1];
                }
            }
            return tmpMax;

        }
        public string SpiltExpression(string strExpression)
        {
            string strTemp = "";
            string strExp = "";
            while (strExpression.IndexOf("(") != -1)
            {
                strTemp = strExpression.Substring(strExpression.LastIndexOf("(") + 1, strExpression.Length - strExpression.LastIndexOf("(") - 1);
                strExp = strTemp.Substring(0, strTemp.IndexOf(")"));
                strExpression = strExpression.Replace("(" + strExp + ")", CalculateExpress(strExp).ToString());
            }
            if (strExpression.IndexOf("+") != -1 || strExpression.IndexOf("-") != -1
            || strExpression.IndexOf("*") != -1 || strExpression.IndexOf("/") != -1)
            {
                strExpression = CalculateExpress(strExpression).ToString();
            }
            return strExpression;
        }

        private string GetExpType(string strExpression)
        {
            strExpression = strExpression.ToUpper();
            if (strExpression.IndexOf("SIN") != -1)
            {
                return CalculateExExpress(strExpression.Substring(strExpression.IndexOf("N") + 1, strExpression.Length - 1 - strExpression.IndexOf("N")), TmphEnumFormula.Sin).ToString();
            }
            if (strExpression.IndexOf("COS") != -1)
            {
                return CalculateExExpress(strExpression.Substring(strExpression.IndexOf("S") + 1, strExpression.Length - 1 - strExpression.IndexOf("S")), TmphEnumFormula.Cos).ToString();
            }
            if (strExpression.IndexOf("TAN") != -1)
            {
                return CalculateExExpress(strExpression.Substring(strExpression.IndexOf("N") + 1, strExpression.Length - 1 - strExpression.IndexOf("N")), TmphEnumFormula.Tan).ToString();
            }
            if (strExpression.IndexOf("ATAN") != -1)
            {
                return CalculateExExpress(strExpression.Substring(strExpression.IndexOf("N") + 1, strExpression.Length - 1 - strExpression.IndexOf("N")), TmphEnumFormula.ATan).ToString();
            }
            if (strExpression.IndexOf("SQRT") != -1)
            {
                return CalculateExExpress(strExpression.Substring(strExpression.IndexOf("T") + 1, strExpression.Length - 1 - strExpression.IndexOf("T")), TmphEnumFormula.Sqrt).ToString();
            }
            if (strExpression.IndexOf("POW") != -1)
            {
                return CalculateExExpress(strExpression.Substring(strExpression.IndexOf("W") + 1, strExpression.Length - 1 - strExpression.IndexOf("W")), TmphEnumFormula.Pow).ToString();
            }
            return strExpression;
        }
    }
}
