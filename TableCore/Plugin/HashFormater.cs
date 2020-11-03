using LitJson;
using System.Text.RegularExpressions;
using System.Xml;

namespace TableCore.Plugin
{
    public class HashFormater : IGenFormater
    {
        public bool ignoreCase = false;
        readonly string num_pattern = @"^(\-|\+)?\d+$";

        public IExportData ExportData(string data, string comment)
        {
            return null;
        }

        public JsonData Format(string data, GTOutputCfg catgory)
        {
            if (Regex.IsMatch(data, num_pattern))
                return int.Parse(data);
            if (ignoreCase)
                return StringUtil.IgnoreCaseToHash(data);
            else
                return StringUtil.ToHash(data);
        }

        public void Init(XmlElement element)
        {
            ignoreCase = element.GetAttribute("ignore_case") == "true";
        }

        public bool IsValid(string data)
        {
            return true;
        }
    }
}
