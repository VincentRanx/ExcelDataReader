using LitJson;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace TableCore.Plugin
{
    public class ExportTextFormatter : IGenFormatter, IGenCmdInitializer, IGenXmlInitializer
    {
        readonly string voice_pattern = @"^\[voice:[a-zA-Z0-9_]+\]";

        public class ExportText : IExportData
        {
            public string ExportExcelFile { get; set; }

            public string ExportExcelSheet { get; set; }

            public int ExcelStartRow { get; set; }

            public int ExcelStartCol { get; set; }

            public JsonData ExportData { get; set; }
            

            public ExportText(JsonData data, string excelFile, string sheet,int startRow, int startCol)
            {
                ExportData = data;
                ExportExcelFile = excelFile;
                ExportExcelSheet = sheet;
                ExcelStartRow = startRow;
                ExcelStartCol = startCol;
            }

            public ExportText()
            {

            }
        }

        int mStartRow = 0;
        int mStartCol = 1;
        string mFile = "TextRes.xlsx";
        string mSheet = "TextRes_cn";

        public void Init(XmlElement element)
        {
            string s;
            mFile = element.GetAttribute("file");
            s = element.GetAttribute("sheet");
            mSheet = s;
            s = element.GetAttribute("row");
            if (!string.IsNullOrEmpty(s))
                mStartRow = int.Parse(s);
            s = element.GetAttribute("col");
            if (!string.IsNullOrEmpty(s))
                mStartCol = int.Parse(s);
        }

        public void Init(Dictionary<string, string> args, string content)
        {
            string v;
            if (args.TryGetValue("file", out v))
                mFile = v;
            if (args.TryGetValue("sheet", out v))
                mSheet = v;
            if (args.TryGetValue("row", out v))
                mStartRow = int.Parse(v);
            if (args.TryGetValue("col", out v))
                mStartCol = int.Parse(v);
        }

        public bool IsValid(string input)
        {
            return true;
        }

        public JsonData Format(string input, GTOutputCfg category)
        {
            if (string.IsNullOrEmpty(input))
                return 0;
            else
            {
                Match mat = Regex.Match(input, voice_pattern);
                if (mat == null || mat.Length <= 8)
                    return StringUtil.ToHash(input);
                else
                    return StringUtil.ToHash(input.Substring(mat.Length));
            }
        }

        public IExportData ExportData(string input, string comment)
        {
            if (string.IsNullOrEmpty(input))
                return null;
            Match mat = Regex.Match(input, voice_pattern);
            string txt;
            string voice;
            if (mat == null || !mat.Success)
            {
                txt = input;
                voice = "";
            }
            else
            {
                txt = input.Substring(mat.Length);
                voice = input.Substring(7, mat.Length - 8);
            }
            JsonData data = new JsonData();
            data["comment"] = comment;
            data["id"] = StringUtil.ToHash(txt);// StringUtil.ToHash(input);
            data["text"] = txt;
            data["voice"] = voice;
            return new ExportText(data, mFile, mSheet, mStartRow, mStartCol);
        }

    }
}
