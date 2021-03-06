﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using TableCore.Exceptions;

namespace TableCore
{
    /*
     * template
     * 
     
using LitJson;
$using-namespace$

namespace $namespace$
{
    public partial class $class-name$ : TableBase
    {
        $fields$

        public override void Init(JsonData data)
        {
            base.Init(data);
            $init-fields$
        }
    }
}
     */
    public class CsharpGenerater : ICodeGenerater
    {
        
        GTClassCfg mCfg;
        public CsharpGenerater(GTClassCfg cfg)
        {
            mCfg = cfg;
        }
        
        public void GenerateCode(GTStatus status, string file)
        {
            GTStatus gt = GTStatus.Instance;
            ClassModel mod = gt.ClassMod;
            if (mod == null)
                throw new NoClassDefineException();

            StringBuilder builder = new StringBuilder();
            AppendUsing(builder);
            int tab = 0;
            bool usenamespace = !string.IsNullOrEmpty(mCfg.NamespaceValue);
            if (usenamespace)
            {
                builder.Append("namespace ").Append(mCfg.NamespaceValue).Append("\n");
                builder.Append("{\n");
                tab++;
            }
            builder.AppendWithTab("public partial class ", tab).Append(mod.ClassName).Append(" : TableBase\n");
            builder.AppendWithTab("{\n", tab++);

            if (mCfg.IndexedClass.Contains(mod.ClassName))
                AppendPropertyIndex(builder, tab, mod);

            AppendConstructer(builder, tab, mod);

            for (int i = 0; i < mod.PropertyCount; i++)
            {
                ClassModel.Property p = mod.GetProperty(i);
                if (p.IsID || p.Ignore)
                    continue;
                builder.AppendWithTab("public ", tab).Append(p.GenType.GTName).Append(" ").Append(p.Name).Append(" {get; private set;}\n");
            }

            AppendOverrideInit(builder, tab, mod);
            AppendClone(builder, tab, mod);
            AppendExtendCode(builder, tab, mod);
            builder.AppendWithTab("}\n", --tab);

            if (usenamespace)
            {
                tab--;
                builder.Append("}");
            }
            FileInfo f = new FileInfo(file);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
            string text = builder.ToString();
            File.WriteAllText(file, text, Encoding.UTF8);
        }

        void AppendPropertyIndex(StringBuilder builder, int tab, ClassModel mod)
        {
            int index = 0;
            for (int i = 0; i < mod.PropertyCount; i++)
            {
                var p = mod.Properties[i];
                if (p.Ignore)
                    continue;
                builder.AppendWithTab("public const int ", tab);
                builder.Append(p.Name).Append("_index = ").Append(index++).Append(";\n");
            }
        }

        void AppendConstructer(StringBuilder builder, int tab, ClassModel mod)
        {
            builder.AppendWithTab("public ", tab);
            builder.Append(mod.ClassName).Append("() : base() {}\n");
            builder.AppendWithTab("public ", tab);
            builder.Append(mod.ClassName).Append("(int id) : base(id) {}\n");
        }

        void AppendClone(StringBuilder builder, int tab, ClassModel mod)
        {
            builder.AppendWithTab("public virtual void Clone(", tab);
            builder.Append(mod.ClassName).Append(" other)\n");
            builder.AppendWithTab("{\n", tab++);
            for (int i = 0; i < mod.PropertyCount; i++)
            {
                ClassModel.Property p = mod.GetProperty(i);
                if (p.IsID || p.Ignore)
                    continue;
                builder.AppendWithTab(p.Name, tab);
                builder.Append(" = other.").Append(p.Name).Append(";\n");
            }
            builder.AppendWithTab("}\n", --tab);
        }

        void AppendExtendCode(StringBuilder builder, int tab, ClassModel mod)
        {
            var file = Utils.GetRelativePath(string.Format("Config/Partials/code-{0}.txt", mod.ClassName));
            if (File.Exists(file))
            {
                using (var reader = File.OpenText(file))
                {
                    string str;
                    while ((str = reader.ReadLine()) != null)
                    {
                        builder.AppendWithTab(str, tab).Append("\n");
                    }
                    reader.Close();
                }
            }
        }

        void AppendOverrideInit(StringBuilder builder, int tab, ClassModel mod)
        {
            if (mod.PropertyCount < 2)
                return;
            builder.AppendWithTab("public override void Init(JsonData obj)\n", tab);
            builder.AppendWithTab("{\n", tab++);
            builder.AppendWithTab("base.Init(obj);\n", tab);
            for(int i = 0; i < mod.PropertyCount; i++)
            {
                ClassModel.Property p = mod.GetProperty(i);
                if (p.IsID || p.Ignore)
                    continue;
                if (string.IsNullOrEmpty(p.GenType.OverrideCode))
                {
                    //builder.AppendWithTab(p.Name, tab).Append(" = obj.Value<").Append(p.GenType.GTName)
                    //    .Append(">(\"").Append(p.Name).Append("\");\n");

                    builder.AppendWithTab(p.Name, tab).Append(" = (").Append(p.GenType.GTName).
                        Append(")obj[\"").Append(p.Name).Append("\"];\n");
                }
                else
                {
                    string str = p.GenType.OverrideCode.Replace("{name}", p.Name);
                    str = str.Replace("{input}", "obj");
                    string[] lines = str.Split('\n');
                    for(int n = 0; n < lines.Length; n++)
                    {
                        builder.AppendWithTab(lines[n], tab).Append("\n");
                    }
                }
            }
            var part = Utils.GetRelativePath(string.Format("Config/Partials/init-{0}.txt", mod.ClassName));
            if(File.Exists(part))
            {
                using (var reader = File.OpenText(part))
                {
                    string str;
                    while((str = reader.ReadLine()) != null)
                    {
                        builder.AppendWithTab(str, tab).Append("\n");
                    }
                    reader.Close();
                }
            }
            builder.AppendWithTab("}\n", --tab);
        }

        void AppendUsing(StringBuilder builder)
        {
            List<string> use = mCfg.UsingNamespace;
            builder.Append("using LitJson;\n");
            for(int i = 0; i < use.Count; i++)
            {
                builder.Append("using ").Append(use[i]).Append(";\n");
            }
        }

    }
}
