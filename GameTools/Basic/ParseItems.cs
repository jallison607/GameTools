using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameTools.Basic
{
    public static class ParseItems
    {
        //Static Parser Methods for items, All three must get the SAME tmpString
        // parseID - Retreives ID of item
        // parseName - Retreives name of item
        // parseOther - Retreives the rest of the item values

        public static string parseStringFrom(string tmpString, int tmpLength)
        {
            string tmpOther;
            tmpOther = tmpString.Substring(0, tmpLength);
            return tmpOther;
        }

        public static int parseIntFrom(string tmpString, int tmpLength)
        {
            string tmpOther;
            tmpOther = tmpString.Substring(0, tmpLength);
            return Int32.Parse(tmpOther);
        }

        public static string convertToLength(int tmpOrig, int length)
        {
            string tmpConverted = tmpOrig.ToString();

            while (tmpConverted.Length < length)
            {
                tmpConverted = "0" + tmpConverted;
            }

            return tmpConverted;
        }
    }
}
