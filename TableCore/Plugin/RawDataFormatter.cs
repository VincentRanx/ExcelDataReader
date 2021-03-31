using LitJson;
using System.Text.RegularExpressions;
using System.Xml;

namespace TableCore.Plugin
{
    public class RawDataFormatter : IGenFormatter
    {
        const string integer = @"^(\-|\+)?\d+$";
        const string boolean = @"^(true|false)$";
        const string number = @"^(\-|\+)?\d+(\.\d+)?$";

        public bool IsValid(string input)
        {
            return true;
        }

        public JsonData Format(string input, GTOutputCfg category)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return null;
                if (Regex.IsMatch(input, integer))
                    return long.Parse(input);
                if (Regex.IsMatch(input, number))
                    return double.Parse(input);
                if (Regex.IsMatch(input, boolean))
                    return bool.Parse(input);
                try
                {
                    var data = JsonMapper.ToObject(input);// JsonConvert.DeserializeObject(input);
                    if (data == null)
                        return input;
                    else
                        return data;
                }
                catch
                {
                    return input;
                }
            }
            catch
            {
                return input;
            }
        }

        public IExportData ExportData(string input, string comment)
        {
            return null;
        }
    }
}
