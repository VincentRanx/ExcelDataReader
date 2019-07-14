using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableCore;

namespace TableTranslate
{
    class Program
    {

        const int ERROR_PARSE_ARG = 8;
        const int ERROR_DATA_ERROR = 16;

        const string ARG_JSON = "-json";
        const string ARG_JFILE = "-file";
        const string ARG_EXPORT = "-sheet";

        static string help = @"
exec [arg1] [arg2] [arg3]... [excel file]
参数列表
-json [json]            :json 数据
-file [file]            :json 数据文件
-sheet [sheet]          :导出 excel 表Sheet
";

        static string json;
        static string file;
        static string sheet;
        static string excel;

        static void ParseArgs(string[] args)
        {
            if (args == null || args.Length == 0)
                throw new Exception("参数错误。");
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case ARG_JSON:
                        if (i >= args.Length - 1)
                            throw new Exception("参数错误。");
                        json = args[++i];
                        break;
                    case ARG_JFILE:
                        if (i >= args.Length - 1)
                            throw new Exception("参数错误。");
                        file = args[++i];
                        break;
                    case ARG_EXPORT:
                        if (i >= args.Length - 1)
                            throw new Exception("参数错误。");
                        sheet = args[++i];
                        break;
                    default:
                        if (i != args.Length - 1)
                            throw new Exception("参数错误。");
                        excel = args[i];
                        break;
                }
            }
        }
        static int Main(string[] args)
        {
            int error = 0;
            try
            {
                ParseArgs(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("\n[ERROR] {0}\n{1}", e, StringUtil.LinkString(false, " ", args)));
                Console.WriteLine(help);
                Console.ReadKey();
                error |= ERROR_PARSE_ARG;
                return error;
            }
            JsonData data;
            try
            {
                JsonMapper.ToObject(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("\n[ERROR] {0}\n{1}", e, StringUtil.LinkString(false, " ", args)));
                error |= ERROR_DATA_ERROR;
                return error;
            }
            return error;
        }
    }
}
