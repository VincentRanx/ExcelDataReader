﻿using LitJson;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace TableCore.Plugin
{
    public class EnumDataFormatter : IGenFormatter, IGenCmdInitializer, IGenXmlInitializer
    {
        string[] mEnums;
        int[] mValues;
        string mNumPattern = @"^(\-|\+)?\d+$";

        public void Init(XmlElement element)
        {
            string arg = element.GetAttribute("enums");
            if (string.IsNullOrEmpty(arg))
                mEnums = new string[0];
            else
                mEnums = arg.Split(',');
            arg = element.GetAttribute("values");
            if (string.IsNullOrEmpty(arg))
            {
                mValues = new int[mEnums.Length];
                for (int i = 0; i < mValues.Length; i++)
                    mValues[i] = i;
            }
            else
            {
                mValues = new int[mEnums.Length];
                string[] values = arg.Split(',');
                int n = 0;
                for (int i = 0; i < mValues.Length; i++)
                {
                    if (i < values.Length)
                        n = int.Parse(values[i]);
                    else
                        n++;
                    mValues[i] = n;
                }
            }
        }

        public void Init(Dictionary<string, string> args, string content)
        {
            if (string.IsNullOrEmpty(content))
                mEnums = new string[0];
            else
                mEnums = content.Split('\n');
            var pattern = @"^(\-|\+)?\d+ *: *[\w\W]+$";
            mValues = new int[mEnums.Length];
            for (int i = 0; i < mEnums.Length; i++)
            {
                if (Regex.IsMatch(mEnums[i], pattern))
                {
                    var n = mEnums[i].IndexOf(':');
                    mValues[i] = int.Parse(mEnums[i].Substring(0, n).Trim());
                    mEnums[i] = mEnums[i].Substring(n + 1).Trim();
                }
                else
                {
                    mEnums[i] = mEnums[i].Trim();
                    mValues[i] = i;
                }
            }
        }

        public bool IsValid(string input)
        {
            if (Regex.IsMatch(input, mNumPattern))
                return true;
            for (int i = 0; i < mEnums.Length; i++)
            {
                if (EqualV(mEnums[i], input))
                    return true;
            }
            return false;
        }

        public JsonData Format(string input, GTOutputCfg category)
        {
            int num;
            if (int.TryParse(input, out num))
                return num;
            for(int i = 0; i < mEnums.Length; i++)
            {
                if (EqualV(mEnums[i], input))
                    return mValues[i];
            }
            return 0;
        }

        bool EqualV(string a, string b)
        {
            return a.ToLower() == b.ToLower();
        }

        public IExportData ExportData(string input, string comment)
        {
            return null;
        }

    }
}