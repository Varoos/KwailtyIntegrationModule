using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KwailtyIntegrationModule.Models
{
    public class BL_Configdata
    {
        
        public static string CompanyCode
        {
            get { return System.Configuration.ConfigurationSettings.AppSettings["CompanyCode"]; }
        }
        public static string UserName
        {
            get { return System.Configuration.ConfigurationSettings.AppSettings["UserName"]; }
        }
        public static string Password
        {
            get { return System.Configuration.ConfigurationSettings.AppSettings["Password"]; }
        }
        public static string Focus8Path
        {
            get { return System.Configuration.ConfigurationSettings.AppSettings["Focus8exePath"]; }
        }
        public static int Focus8CompID
        {

            get
            {
                int iErpCnt = 0;
                int iPosCnt = 0;
                int iPos = 4;
                iPosCnt = iErpCnt * iPos;
                int iRet = 0;
                string sCompCode = CompanyCode;
                if (IsNumeric(sCompCode[iPosCnt]))
                {
                    iRet = (36 * 36) * int.Parse(sCompCode[iPosCnt].ToString());
                }
                else
                {
                    iRet = (36 * 36) * getCompCodeVal(sCompCode[iPosCnt]);
                }
                if (IsNumeric(sCompCode[iPosCnt + 1]))
                {
                    iRet += (36) * int.Parse(sCompCode[iPosCnt + 1].ToString());
                }
                else
                {
                    iRet += (36) * getCompCodeVal(sCompCode[iPosCnt + 1]);
                }

                if (IsNumeric(sCompCode[iPosCnt + 2]))
                {
                    iRet += (36 * 0) * int.Parse(sCompCode[iPosCnt + 2].ToString());
                }
                else
                {
                    iRet += (36 * 0) * getCompCodeVal(sCompCode[iPosCnt + 2]);
                }
                return iRet;
            }
        }
        public static bool IsNumeric(char o)
        {
            double result;
            return o != null && Double.TryParse(o.ToString(), out result);
        }
        private static int getCompCodeVal(char cCode)
        {
            int iRet = 0;
            char[] sLetters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            for (int i = 0; i < sLetters.Length; i++)
            {
                if (sLetters[i] == cCode)
                {
                    iRet = i;
                    break;
                }
            }
            return iRet + 10;
        }
        
    }
}