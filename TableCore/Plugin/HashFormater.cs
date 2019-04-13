using LitJson;
using System.Xml;

namespace TableCore.Plugin
{
    public class HashFormater : IGenFormater
    {
        public bool ignoreCase = false;

        public IExportData ExportData(string data, string comment)
        {
            return null;
        }

        public JsonData Format(string data, GTOutputCfg catgory)
        {
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
