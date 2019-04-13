#define TRY_CATCH
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TableCore;

namespace TableGenerator
{
    public partial class Form1 : Form
    {

        string[] mTables;
        string[] mCategories;
        TreeNode mEditing;
        int mCellRow;
        int mCellCol;
        int mStartRow = 0;
        int mStartCol = 1;
 /*
 * cfg tree:
 * __Support
 *   |____support types
 *   |   |____type0...
 *   |
 *   |____Class Define
 *       |____namespace xxx
 *       |____using xxx
 *   
 * __Properties
 *   
 * __Class Export
 *   |____name
 *   |____folder
 *       
 * __Data Export
 *   |____category
 *       |____mode
 *       |____folder
 *       |____extension
 */
        readonly string cfg_support = "Support";
        readonly string cfg_types = "Types";
        readonly string cfg_class = "Class";
        readonly string cfg_properties = "Property";
        readonly string cfg_class_export = "ClassExp";
        readonly string cfg_class_name = "ClassName";
        readonly string cfg_data_export = "DataExp";
        readonly string cfg_folder = "Folder";

        bool skipTypeDefine;

        string NameTreeFolderItem(string folder)
        {
            return string.Format("♡ 文件夹: {0}", folder);
        }

        void UpdateCfgTree()
        {
            //cfgTree.Nodes.Clear();

            var support = GetTreeNode(cfgTree, cfg_support);// cfgTree.Nodes.Add("Support");
            support.Nodes.Clear();

            // support/types
            var typedef = support.Nodes.Add("Type Define");
            typedef.Name = cfg_types;
            var types = GTStatus.Instance.Config.ActiveClass.GTTypes;
            foreach (var v in types.Values)
            {
                var tp = typedef.Nodes.Add(v.Name);
                tp.Name = v.Name;
                AppendTypeDescript(tp, v);
            }

            // support/class define
            var classdef = support.Nodes.Add("Class Define");
            classdef.Name = cfg_class;
            if (!string.IsNullOrEmpty(GTStatus.Instance.Config.ActiveClass.NamespaceValue))
                classdef.Nodes.Add(string.Format("namespace {0}", GTStatus.Instance.Config.ActiveClass.NamespaceValue));
            foreach (var use in GTStatus.Instance.Config.ActiveClass.UsingNamespace)
            {
                classdef.Nodes.Add(string.Format("using {0}", use));
            }

            var props = GetTreeNode(cfgTree, cfg_properties);// cfgTree.Nodes.Add("属性列表");
            props.Nodes.Clear();

            // class export
            var classgen = GetTreeNode(cfgTree, cfg_class_export);// cfgTree.Nodes.Add("导出代码");
            classgen.Nodes.Clear();
            var cfolder = classgen.Nodes.Add(NameTreeFolderItem(GTStatus.Instance.Config.ActiveClass.OutputFolder));
            cfolder.Name = cfg_folder;
            //cfolder.ContextMenuStrip = propertyContextMenu;
            var classmod = classgen.Nodes.Add(string.Format("♡ 类名: {0}",  GTStatus.Instance.ClassMod?.ClassName));
            classmod.Name = cfg_class_name;
            //classmod.ContextMenuStrip = propertyContextMenu;

            // export
            var output = GetTreeNode(cfgTree, cfg_data_export); //cfgTree.Nodes.Add("导出数据");
            output.Nodes.Clear();
            foreach (var cat in GTStatus.Instance.Config.GTOutputs.Values)
            {
                var catnode = output.Nodes.Add(string.Format("{0} ({1} 模式)", cat.Category, cat.DataMode));
                catnode.Name = cat.Category;
                var folder = catnode.Nodes.Add(NameTreeFolderItem(cat.DataFolder));
                folder.Name = cfg_folder;
                catnode.Nodes.Add(string.Format("扩展名: {0}", cat.DataExtension));
            }
            output.ExpandAll();
            modifyNameField.Visible = false;
            UpdatePropertyContectMenu(null);
        }

        void AppendTypeDescript(TreeNode root, GTType gtype)
        {
            root.Nodes.Add(string.Format("Default Value: {0}", gtype.DefaultValue));
            if (!string.IsNullOrEmpty(gtype.Pattern))
                root.Nodes.Add(string.Format("Regex: {0}", gtype.Pattern));
            if (gtype.Formater != null)
                root.Nodes.Add(string.Format("Formater: {0}", gtype.Formater.GetType().Name));
        }
        
        public Form1()
        {
            InitializeComponent();
            GetCategories();
            ReadData();
            tableList.SelectedIndex = 0;
            UpdateCfgTree();
        }

        //TreeNode GetTreeNode(TreeView tree, string nodeName)
        //{
        //    var arr = nodeName.Split('/');
        //    return GetTreeNode(tree, arr);
        //}

        TreeNode GetTreeNode(TreeView tree, params string[] nodenames)
        {
            var root = tree.Nodes;
            for (int i = 0; i < nodenames.Length; i++)
            {
                var node = GetChild(root, nodenames[i]);
                if (node == null)
                    return null;
                if (i == nodenames.Length - 1)
                    return node;
                root = node.Nodes;
            }
            return null;
        }

        TreeNode GetChild(TreeNodeCollection root, string name)
        {
            for (int i = 0; i < root.Count; i++)
            {
                var node = root[i];
                if (node.Name == name)
                    return node;
            }
            return null;
        }

        bool IsSubNode(TreeNode node, TreeNode root, int level = 1)
        {
            if (node == null || root == null)
                return false;
            var n = node;
            int lv = 0;
            while (n != null)
            {
                if (n == root)
                    return lv >= level;
                n = n.Parent;
                lv++;
            }
            return false;
        }

        TreeNode GetParentNode(TreeNode node, string parentName)
        {
            if (node == null)
                return null;
            var n = node;
            while(n != null)
            {
                if (n.Name == parentName)
                    return n;
                n = n.Parent;
            }
            return null;
        }

        void GetCategories()
        {
            List<string> cats = new List<string>();
            GTStatus.Instance.Config.GetOutputCategories(cats);
            mCategories = cats.ToArray();

            genDataMenu.DropDownItems.Clear();
            foreach(var cat in cats)
            {
                var item = genDataMenu.DropDownItems.Add(cat);
                item.Click += OnGenDataClick;
            }
        }

        private void OnGenDataClick(object sender, EventArgs e)
        {
            genData(((ToolStripItem)sender).Text);
        }

        void ReadData()
        {
            var text = Utils.ReadRelativeFile("Config/data.sav");
            if (string.IsNullOrEmpty(text))
                return;
            var data = JsonConvert.DeserializeObject<JObject>(text);
            var ccfg = GTStatus.Instance.Config.GetClass(EGTLang.csharp);
            var str = data.Value<string>("cs-folder");
            if (!string.IsNullOrEmpty(str))
                ccfg.OutputFolder = str;
            foreach (var cat in mCategories)
            {
                str = data.Value<string>(StringUtil.Concat(cat, "-folder"));
                if (!string.IsNullOrEmpty(str))
                {
                    var category = GTStatus.Instance.Config.GetOutput(cat);
                    if (category != null)
                        category.DataFolder = str;
                }
            }
        }

        void SaveData()
        {
            JObject data = new JObject();
            var ccfg = GTStatus.Instance.Config.GetClass(EGTLang.csharp);
            data["cs-folder"] = ccfg.OutputFolder;
            foreach (var cat in mCategories)
            {
                var category = GTStatus.Instance.Config.GetOutput(cat);
                if (category != null)
                    data[StringUtil.Concat(cat, "-folder")] = category.DataFolder;
            }
            Utils.WriteRelativeFile("Config/data.sav", data.ToString());
        }
        
        void OpenFile(bool skipTypeDef)
        {
            DialogResult result = openFileDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
#if TRY_CATCH
                try
                {
#endif
                    this.skipTypeDefine = skipTypeDef;
                    GTStatus.Instance.UnlimitMode = skipTypeDef;
                    GTStatus.Instance.OpenFile(openFileDlg.FileName);
                    var tabs = GTStatus.Instance.TableNames;
                    mTables = new string[tabs.Count];
                    tabs.CopyTo(mTables, 0);
                    tablePreview.DataSource = GTStatus.Instance.Data;

                    mStartCol = -1;
                    mStartRow = -1;

                    tableList.Items.Clear();
                    tableList.Items.AddRange(mTables);
                    tableList.SelectedIndex = 0;

                    SelectTable(mTables[0]);

                    reuseTable.Enabled = true;
                    setAsStart.Enabled = true;
                    genDataMenu.Enabled = true;
                    checkDataMenu.Enabled = GTStatus.Instance.ClassMod != null;

#if TRY_CATCH
                }
                catch (Exception e)
                {
                    MessageBox.Show(this, e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
#endif
            }
        }

        void SelectTable(string tableName)
        {
#if TRY_CATCH
            try
            {
#endif
                tablePreview.DataMember = tableName;
                GTStatus.Instance.UseTable(tableName, mStartRow, mStartCol, false, skipTypeDefine);
                mStartRow = GTStatus.Instance.StartRow;
                mStartCol = GTStatus.Instance.StartCol;
                ClassModel mod = GTStatus.Instance.ClassMod;
                SetClassModel(mod);
#if TRY_CATCH
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif
        }
        
        void SetClassModel(ClassModel mod)
        {
            var node = GetTreeNode(cfgTree, cfg_class_export, cfg_class_name);
            if (node != null)
                node.Text = string.Format("♡ 类名: {0}", mod?.ClassName);
            node = GetTreeNode(cfgTree, cfg_properties);
            if (node != null)
            {
                node.Nodes.Clear();
                if (mod != null)
                {
                    for (int i = 0; i < mod.PropertyCount; i++)
                    {
                        var pro = mod.GetProperty(i);
                        var p = node.Nodes.Add(string.Format("{0} {1}  :{2}", pro.Ignore ? "✗" : "✓", pro.Name, pro.GenType.Name));
                        p.Name = pro.Name;
                        AppendTypeDescript(p, pro.GenType);
                    }
                }
                node.Expand();
            }
        }

        private void generateCsharpCode_Click(object sender, EventArgs e)
        {
            if (GTStatus.Instance.ClassMod == null)
                return;
            //GTStatus.Instance.IgnorePropertyWithCategoryPattern();
            CsharpGenerater gen = new CsharpGenerater(GTStatus.Instance.Config.ActiveClass);
            string file = string.Format("{0}/{1}.cs", GTStatus.Instance.Config.ActiveClass.OutputFolder, GTStatus.Instance.ClassMod.ClassName);
            gen.GenerateCode(GTStatus.Instance, file);
            MessageBox.Show("生成文件： " + file, "Complish");
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveData();
        }
        
        private void tablePreview_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            mCellRow = e.RowIndex;
            mCellCol = e.ColumnIndex;
            cell.Text = Utils.GetCellName(mCellRow, mCellCol);
        }

        private void setAsStart_Click(object sender, EventArgs e)
        {
            if (mTables != null && mTables.Length > 0 && mCellRow >= 0 && mCellRow < tablePreview.RowCount
                && mCellCol >= 0 && mCellCol < tablePreview.ColumnCount
                && tablePreview.Rows[mCellRow].Cells[mCellCol].Selected)
            {
                mStartRow = mCellRow;
                mStartCol = mCellCol;
                SelectTable(mTables[tableList.SelectedIndex]);
            }
        }
        
        bool SetCfg(bool merge)
        {
            if (!merge)
            {
                GTStatus.Instance.Config = GTConfig.NewDefaultCfg();
                GetCategories();
                UpdateCfgTree();
                return true;
            }
            DialogResult result = openCfgDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                GTStatus.Instance.Config.MergeCfg(openCfgDlg.FileName);
                GetCategories();
                UpdateCfgTree();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void reuseTable_Click(object sender, EventArgs e)
        {
            var stat = GTStatus.Instance;
            if (mTables != null && mTables.Length > 0)
            {
#if TRY_CATCH
                try
                {
#endif
                    stat.UseTable(mTables[tableList.SelectedIndex], mStartRow, mStartCol, true);
                    SetClassModel(stat.ClassMod);
#if TRY_CATCH
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
#endif
            }
        }

        private void resetCfg_Click(object sender, EventArgs e)
        {
            SetCfg(false);
        }
        
        void genData(params string[] categories)
        {
            if (categories == null || categories.Length == 0)
                return;
            using (ProgressDialog dlg = new ProgressDialog())
            {
                Thread t = new Thread(() =>
                {
#if TRY_CATCH
                    try
                    {
#endif
                        while (!dlg.IsHandleCreated)
                        {
                        }
                        StringBuilder buf = new StringBuilder();
                        foreach (var cat in categories)
                        {
                            GTStatus.Instance.Config.ActiveCategory = cat;
                            //GTStatus.Instance.IgnorePropertyWithCategoryPattern();
                            GTStatus.Instance.GenerateData((x, y) =>
                            {
                                dlg.Progress = x;
                                dlg.DisplayText = y;
                            });
                            buf.Append("Export ").Append(GTStatus.Instance.DataPath).Append("\n");
                        }
                        dlg.Stop(DialogResult.OK);
                        MessageBox.Show(buf.ToString(), "Complish");
#if TRY_CATCH
                    }
                    catch (Exception ex)
                    {
                        dlg.Stop(DialogResult.Abort);
                        MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
#endif
                });
                t.Start();
                dlg.ShowDialog();
            }
        }

        private void selDataFolder_Click(object sender, EventArgs e)
        {

        }

        private void tableList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(mTables!= null && mTables.Length > 0)
                SelectTable(mTables[tableList.SelectedIndex]);
        }

        private void openFileContextMenu_Click(object sender, EventArgs e)
        {
            OpenFile(false);
        }

        private void addCfgFileMenu_Click(object sender, EventArgs e)
        {
            SetCfg(true);
        }

        void UpdatePropertyContectMenu(TreeNode node)
        {
            cfgTree.SelectedNode = node;
            var name = node == null ? "" : node.Name;
            if (name == cfg_folder)
            {
                chooseFolderContect.Enabled = true;
                renameContectMenu.Enabled = false;
            }
            else if (name == cfg_class_name)
            {
                chooseFolderContect.Enabled = false;
                renameContectMenu.Enabled = GTStatus.Instance.ClassMod != null;
            }
            else
            {
                chooseFolderContect.Enabled = false;
                renameContectMenu.Enabled = false;
            }
            genCsharpContectMenu.Enabled = GTStatus.Instance.IsTableUsed;
            genDataContectMenu.Enabled = GTStatus.Instance.IsTableUsed;
            var prop = GetProperty(node);
            ignoreProperty.Text = (prop != null && prop.Ignore) ? "启用属性" : "忽略属性";
            ignoreProperty.Enabled = prop != null && !prop.IsID;// prop != null && !prop.Ignore && !prop.IsID;
        }

        private void cfgTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode node = e.Node;
                UpdatePropertyContectMenu(node);
            }
        }

        ClassModel.Property GetProperty(TreeNode node)
        {
            if (!IsSubNode(node, GetTreeNode(cfgTree, cfg_properties)))
                return null;
            var cmod = GTStatus.Instance.ClassMod;
            ClassModel.Property prop;
            if (cmod != null && cmod.GetProperty(node.Name, out prop))
                return prop;
            else
                return null;
        }

        bool ToggleIgnoreProperty(TreeNode node)
        {
            ClassModel.Property prop = GetProperty(node);
            if (prop != null)
            {
                prop.Ignore = !prop.Ignore;
                node.Text = string.Format("{0} {1}  :{2}", prop.Ignore ? "✗" : "✓", prop.Name, prop.GenType.Name);
                return prop.Ignore;
            }
            else
            {
                return false;
            }
        }

        private void ignoreProperty_Click(object sender, EventArgs e)
        {
            ToggleIgnoreProperty(cfgTree.SelectedNode);
        }
        
        private void chooseFolderContect_Click(object sender, EventArgs e)
        {
            var result = folderBroswerdlg.ShowDialog();
            if(result == DialogResult.OK)
            {
                var node = cfgTree.SelectedNode;
                if (node == null || node.Parent == null)
                {
                    return;
                }
                else if (node.Parent.Name == cfg_class_export)
                {
                    GTStatus.Instance.Config.ActiveClass.OutputFolder = folderBroswerdlg.SelectedPath;
                    node.Text = NameTreeFolderItem(folderBroswerdlg.SelectedPath);
                }
                else 
                {
                    var cat = GTStatus.Instance.Config.GetOutput(node.Parent.Name);
                    if(cat != null)
                    {
                        cat.DataFolder = folderBroswerdlg.SelectedPath;
                        node.Text = NameTreeFolderItem(folderBroswerdlg.SelectedPath);
                    }
                }
            }
        }

        private void renameContectMenu_Click(object sender, EventArgs e)
        {
            var node = cfgTree.SelectedNode;
            if (node != null && node.Name == cfg_class_name)
            {
                //cfgTree.Enable = false;
                mEditing = node;
                modifyNameField.Location = modifyNameField.Parent.PointToClient(cfgTree.PointToScreen(node.Bounds.Location));// node.Bounds;
                modifyNameField.Width = cfgTree.Width - node.Bounds.Left;
                modifyNameField.Visible = true;
                modifyNameField.Text = GTStatus.Instance.ClassMod.ClassName;
                modifyNameField.Focus();
            }
        }

        private void modifyNameField_Leave(object sender, EventArgs e)
        {
            CancelRename(true);
        }

        void CancelRename(bool applyUpdate)
        {
            if (applyUpdate && mEditing != null)
            {
                var cmod = GTStatus.Instance.ClassMod;
                if (cmod != null)
                {
                    cmod.ChangeClassName(modifyNameField.Text, GTStatus.Instance);
                    mEditing.Text = string.Format("♡ 类名: {0}", cmod?.ClassName);
                }
            }
            mEditing = null;
            modifyNameField.Visible = false;
            //cfgTree.Enable = true;
        }

        private void dataContextMenu_VisibleChanged(object sender, EventArgs e)
        {
            CancelRename(false);
        }
        
        private void modifyNameField_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                CancelRename(true);
            }
        }

        private void genDataContectMenu_Click(object sender, EventArgs e)
        {
            var node = cfgTree.SelectedNode;
            List<string> cats = new List<string>();
            var root = GetTreeNode(cfgTree, cfg_data_export);
            bool single = IsSubNode(node, root, 1);
            for (int i = 0; i < root.Nodes.Count; i++)
            {
                var cat = root.Nodes[i];
                if (!single || IsSubNode(node, cat, 0))
                {
                    cats.Add(cat.Name);
                }
            }
            genData(cats.ToArray());
        }

        private void checkDataMenu_Click(object sender, EventArgs e)
        {
            if (GTStatus.Instance.ClassMod == null)
                return;

            for (int col = GTStatus.Instance.StartCol; col < tablePreview.ColumnCount; col++)
            {
                ClassModel.Property prodef;
                if (!GTStatus.Instance.GetPropertyDefine(col - GTStatus.Instance.StartCol, GTStatus.Instance.ClassMod.ClassName, out prodef))
                {
                    continue;
                }
                int len = tablePreview.RowCount;
                for (int row = GTStatus.Instance.DataStartRow; row < len; row++)
                {
                    var cell = tablePreview.Rows[row].Cells[col];
                    var v = cell.Value == null ? null : cell.Value.ToString();
                    bool vaild = string.IsNullOrEmpty(v) ? true : prodef.GenType.IsValid(v);
                    cell.Selected = !vaild;
                }
            }
        }

        private void openFileWithoutTypeDef_Click(object sender, EventArgs e)
        {
            OpenFile(true);
        }
    }
}
