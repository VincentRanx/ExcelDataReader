using LitJson;
using System.Xml;

namespace TableCore.Plugin
{
    public class CsharpBoolFormatter : IGenFormatter
    {
        public bool IsValid(string input)
        {
            string str = input.ToLower();
            return str == "true" || str == "false";
        }

        public JsonData Format( string input, GTOutputCfg category)
        {
            return input.ToLower() == "true" ? true : false;
        }

        public IExportData ExportData(string input, string comment)
        {
            return null;
        }
    }
}
