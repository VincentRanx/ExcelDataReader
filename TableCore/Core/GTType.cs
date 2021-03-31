using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using LitJson;
using TableCore.Exceptions;
using TableCore.Plugin;

namespace TableCore
{
    public enum ECaseType
    {
        normal,
        upper,
        lower,
    }

    public class GTType : IGenFormatter
    {
        readonly static string defulatInput = "{name} = ({type}){input}[\"{name}\"];";
        readonly static int declareid = StringUtil.IgnoreCaseToHash("declare");
        readonly static int formaterid = StringUtil.IgnoreCaseToHash("formatter");
        readonly static int defaultid = StringUtil.IgnoreCaseToHash("default");
        readonly static int jsonid = StringUtil.IgnoreCaseToHash("json-input");
        readonly static int bininput = StringUtil.IgnoreCaseToHash("binary-input");
        readonly static int binoutput = StringUtil.IgnoreCaseToHash("binary-output");
        readonly static int caseid = StringUtil.IgnoreCaseToHash("case");

        public ECaseType CaseType { get; set; }
        public EGTLang Lang { get; private set; }
        public string Name { get; private set; }
        public string DefaultValue { get; private set; }
        /// <summary>
        /// 生成对象类型名
        /// </summary>
        public string GTName { get; set; }
        public IGenFormatter Formater { get; set; }
        public string JsonInput { get; set; }
        public string BinaryInput { get; set; }
        public string BinaryOutput { get; set; }
        string mPattern;
        public string Pattern { get { return mPattern; } }

        public GTType(EGTLang lang)
        {
            Lang = lang;
        }
        
        public GTType(string typeName)
        {
            Name = typeName;
            var file = Utils.GetRelativePath(string.Format("Config/Types/{0}.txt", typeName));
            if (!File.Exists(file))
                throw new TypeNotDefinedException(typeName);
            ParseInitFile(file);
        }

        void ParseInitFile(string file)
        {
            var reader = new SitcomFile();
            reader.Load(File.ReadAllText(file));
            reader.BeginRead();
            if (FindCmd(reader, declareid) && reader.NextKeyword())
            {
                if (reader.keyword.id != ':' && reader.keyword.id != '：')
                    GTName = Name;
                else if (reader.NextKeyword())
                {
                    GTName = reader.keyword.text;
                    int key;
                    string value;
                    while (NextKeyValue(reader, out key, out value))
                    {
                        if (key == defaultid)
                            DefaultValue = value;
                        else if (key == caseid)
                            CaseType = StringUtil.EqualIgnoreCase(value, "lower") ? ECaseType.lower : (StringUtil.EqualIgnoreCase(value, "upper") ? ECaseType.upper : ECaseType.normal);
                    }
                }
                else
                    GTName = Name;

                if (reader.NextContent(true))
                    mPattern = reader.keyword.text;
            }
            if (FindCmd(reader, formaterid) && reader.NextKeyword())
            {
                if ((reader.keyword.id == ':' || reader.keyword.id == '：') && reader.NextKeyword())
                {
                    var formatter = Utils.NewInstance(reader.keyword.text) as IGenFormatter;
                    if (formatter != null && formatter is IGenCmdInitializer)
                    {
                        string arg, value;
                        Dictionary<string, string> args = new Dictionary<string, string>();
                        while (NextKeyValue(reader, out arg, out value))
                        {
                            args[arg] = value;
                        }
                        var content = reader.NextContent(true) ? reader.keyword.text : null;
                        ((IGenCmdInitializer)formatter).Init(args, content);
                    }
                    Formater = formatter;
                }
            }
            if (FindCmd(reader, jsonid) && reader.NextContent(true))
                JsonInput = reader.keyword.text;
            else
                JsonInput = defulatInput.Replace("{type}", GTName);
            if (FindCmd(reader, bininput) && reader.NextContent(true))
                BinaryInput = reader.keyword.text;
            if (FindCmd(reader, binoutput) && reader.NextContent(true))
                BinaryOutput = reader.keyword.text;
        }

        bool FindCmd(SitcomFile file, int cmd)
        {
            var line = file.PresentLine;
            while(file.NextMark('@'))
            {
                if (file.NextKeyword() && file.keyword.id == cmd)
                    return true;
            }
            file.BeginRead();
            while(file.PresentLine < line && file.NextMark('@'))
            {
                if (file.NextKeyword() && file.keyword.id == cmd)
                    return true;
            }
            return false;
        }

        bool NextKeyValue(SitcomFile file, out int key, out string value)
        {
            key = 0;
            value = null;
            if (file.NextKeyword())
            {
                key = file.keyword.id;
                if (!file.NextKeyword())
                    return false;
                if (file.keyword.id != ':' && file.keyword.id != '：')
                    return false;
                if (!file.NextKeyword())
                    return false;
                value = file.keyword.text;
                return true;
            }
            return false;
        }

        bool NextKeyValue(SitcomFile file, out string arg, out string value)
        {
            arg = null;
            value = null;
            if(file.NextKeyword())
            {
                arg = file.keyword.text;
                if (!file.NextKeyword())
                    return false;
                if (file.keyword.id != ':' && file.keyword.id != '：')
                    return false;
                if (!file.NextKeyword())
                    return false;
                value = file.keyword.text;
                return true;
            }
            return false;
        }

        public void Init(bool parseFile, XmlElement element, Dictionary<string, string> patterns, Dictionary<string, IGenFormatter> formaters)
        {
            Name = element.Name;
            if (parseFile)
            {
                var file = Utils.GetRelativePath(string.Format("Config/Types/{0}.txt", Name));
                if (File.Exists(file))
                    ParseInitFile(file);
            }
            DefaultValue = element.GetAttribute("default");
            GTName = element.GetAttribute("name");
            var str = element.GetAttribute("case");
            if (!string.IsNullOrEmpty(str))
                CaseType = (ECaseType)Enum.Parse(typeof(ECaseType), str);
            var patt = element.GetAttribute("pattern");
            if (string.IsNullOrEmpty(patt) || !patterns.TryGetValue(patt, out mPattern))
            {
                mPattern = element.InnerText;
            }
            IGenFormatter formater;
            if (formaters.TryGetValue(Name, out formater))
                Formater = formater;
        }

        public string FormatInput(string input)
        {
            string v = string.IsNullOrEmpty(input) ? DefaultValue : input;
            if (v == null)
                return null;
            if (CaseType == ECaseType.upper)
                v = v.ToUpper();
            else if (CaseType == ECaseType.lower)
                v = v.ToLower();
            return v;
        }

        public bool IsValid(string input)
        {
            if (!string.IsNullOrEmpty(mPattern) && !Regex.IsMatch(input, mPattern))
                return false;
            if (Formater == null)
                return true;
            else
                return Formater.IsValid(input);
        }

        public JsonData Format(string input, GTOutputCfg category)
        {
            if (Formater == null)
                return input;
            else
                return Formater.Format(input, category);
        }
        
        public IExportData ExportData(string data, string comment)
        {
            return Formater == null ? null : Formater.ExportData(data, comment);
        }
    }
}
