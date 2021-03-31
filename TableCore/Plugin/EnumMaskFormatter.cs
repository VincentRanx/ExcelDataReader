using LitJson;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace TableCore.Plugin
{
    public class EnumMaskFormatter : IGenFormatter, IGenCmdInitializer, IGenXmlInitializer
    {
        string[] mEnums;
        int[] mValues;
        public void Init(XmlElement element)
        {
            string arg = element.GetAttribute("enums");
            if (string.IsNullOrEmpty(arg))
                mEnums = new string[0];
            else
                mEnums = arg.ToLower().Split(',');
            arg = element.GetAttribute("values");
            if (string.IsNullOrEmpty(arg))
            {
                mValues = new int[mEnums.Length];
                for (int i = 0; i < mValues.Length; i++)
                    mValues[i] = i > 0 ? (1 << (i - 1)) : 0;
            }
            else
            {
                mValues = new int[mEnums.Length];
                string[] values = arg.Split(',');
                int n = 1;
                for (int i = 0; i < mValues.Length; i++)
                {
                    if (i < values.Length)
                        n = int.Parse(values[i]);
                    else
                        n <<= 1;
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
                    mValues[i] = 1 << i;
                }
            }
        }

        public bool IsValid(string input)
        {
            return true;
        }

        int GetEnumValue(string enumname)
        {
            if (string.IsNullOrEmpty(enumname))
                return 0;
            string str = enumname.ToLower().Trim();
            for (int i = 0; i < mEnums.Length; i++)
            {
                if (mEnums[i] == str)
                    return mValues[i];
            }
            return 0;
        }

        public JsonData Format(string input, GTOutputCfg category)
        {
            int n = 0;
            if (input == "[all]")
            {
                for (int i = 0; i < mValues.Length; i++)
                {
                    n |= mValues[i];
                }
            }
            else
            {
                if (int.TryParse(input, out n))
                    return n;
                string[] args = input.Split(',');
                for (int i = 0; i < args.Length; i++)
                {
                    n |= GetEnumValue(args[i]);
                }
            }
            return n;
        }

        public IExportData ExportData(string input, string comment)
        {
            return null;
        }
    }
}
