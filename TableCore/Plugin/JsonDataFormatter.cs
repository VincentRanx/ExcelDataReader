using LitJson;
using System.Xml;

namespace TableCore.Plugin
{
    public class JsonDataFormatter : IGenFormatter
    {
        public IExportData ExportData(string data, string comment)
        {
            return null;
        }

        public JsonData Format(string data, GTOutputCfg catgory)
        {
            return JsonMapper.ToObject(data);// JsonConvert.DeserializeObject<JToken>(data);
        }

        public bool IsValid(string data)
        {
            return true;
        }
    }
}
