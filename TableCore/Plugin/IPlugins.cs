﻿using LitJson;
using System.Collections.Generic;
using System.Xml;

namespace TableCore.Plugin
{
    // 数据导出
    public interface IExportData
    {
        string ExportExcelFile { get; }
        string ExportExcelSheet { get; }
        int ExcelStartRow { get; }
        int ExcelStartCol { get; }
        JsonData ExportData { get; }
    }

    // 数据类型输出格式化
    public interface IGenFormatter
    {
        //void Init(Dictionary<string, string> args, string content);
        bool IsValid(string data);
        IExportData ExportData(string data, string comment);
        JsonData Format(string data, GTOutputCfg catgory);
    }

    public interface IGenXmlInitializer
    {
        void Init(XmlElement element);
    }

    public interface IGenCmdInitializer
    {
        void Init(Dictionary<string, string> args, string content);
    }

    // 数据输出时格式化
    public interface IDataModify
    {
        void Init(XmlElement element);

        bool PrepareTable(GTStatus status, string catgory, string tableName);

        GenData[] FixOutputData(string catgory, GenData data, GenData previours, GenData next);
    }
    
}
