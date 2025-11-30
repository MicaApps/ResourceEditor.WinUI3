using DevWinUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

namespace RisohEditorWinUI3Blank.Models
{
    public class ResourceManager
    {
        private List<ResourceEntry> m_resources = new List<ResourceEntry>();
        private ResourceEntry m_selectedEntry;

        public void Clear()
        {
            m_resources.Clear();
            m_selectedEntry = null;
        }

        public void AddEntry(ResourceEntry entry)
        {
            m_resources.Add(entry);
        }

        public ResourceEntry GetSelectedEntry()
        {
            return m_selectedEntry;
        }

        public void SetSelectedEntry(ResourceEntry entry)
        {
            m_selectedEntry = entry;
        }

        public IEnumerable<ResourceEntry> GetEntries()
        {
            return m_resources;
        }

        // 添加 GroupBy 方法
        public IEnumerable<IGrouping<string, ResourceEntry>> GroupByType()
        {
            return m_resources.GroupBy(r => r.Type.ToString());
        }

        // 添加搜索方法
        public List<ResourceEntry> Search(string type = null, string name = null, ushort? language = null)
        {
            return m_resources.Where(r =>
                (type == null || r.Type.ToString() == type) &&
                (name == null || r.Name.ToString() == name) &&
                (language == null || r.Language == language.Value)
            ).ToList();
        }

        // 模拟从EXE文件加载资源
        public void LoadFromExecutable(string filename)
        {
            Clear();

            // 这里应该使用Windows API枚举资源
            // 暂时添加一些示例数据
            AddEntry(new ResourceEntry
            {
                Type = new MIdOrString { String = "RT_DIALOG" },
                Name = new MIdOrString { Id = 100 },
                Language = 0x0409,
                Data = new byte[100]
            });

            AddEntry(new ResourceEntry
            {
                Type = new MIdOrString { String = "RT_MENU" },
                Name = new MIdOrString { Id = 100 },
                Language = 0x0409,
                Data = new byte[50]
            });

            AddEntry(new ResourceEntry
            {
                Type = new MIdOrString { String = "RT_STRING" },
                Name = new MIdOrString { Id = 0 },
                Language = 0x0409,
                Data = new byte[200]
            });
        }

        // 模拟从RES文件加载资源
        public void LoadFromResFile(string filename)
        {
            Clear();
            // 实现RES文件解析逻辑
            // 暂时添加示例数据
        }

        // 模拟从RC文件加载资源
        public void LoadFromRcFile(string filename)
        {
            Clear();
            // 实现RC文件解析逻辑
            // 暂时添加示例数据
        }
    }

    public class ResourceEntry
    {
        public MIdOrString Type { get; set; }
        public MIdOrString Name { get; set; }
        public ushort Language { get; set; }
        public byte[] Data { get; set; }
        public List<ResourceEntry> LanguageEntries { get; set; } = new List<ResourceEntry>();

        public bool CanShowAsText
        {
            get
            {
                string typeStr = Type.ToString();
                return typeStr == "RT_STRING" || typeStr == "RT_MESSAGETABLE" ||
                       typeStr == "RT_DIALOG" || typeStr == "RT_MENU" ||
                       typeStr == "RT_ACCELERATOR" || typeStr == "RT_VERSION";
            }
        }

        public bool IsImage
        {
            get
            {
                string typeStr = Type.ToString();
                return typeStr == "RT_BITMAP" || typeStr == "RT_ICON" || typeStr == "RT_CURSOR";
            }
        }

        public bool CanGuiEdit()
        {
            string typeStr = Type.ToString();
            return typeStr == "RT_DIALOG" || typeStr == "RT_MENU" ||
                   typeStr == "RT_ACCELERATOR" || typeStr == "RT_TOOLBAR";
        }

        public string GetText()
        {
            string typeStr = Type.ToString();

            switch (typeStr)
            {
                case "RT_STRING":
                    return GetStringTableText();
                case "RT_DIALOG":
                    return GetDialogText();
                case "RT_MENU":
                    return GetMenuText();
                case "RT_ACCELERATOR":
                    return GetAcceleratorText();
                case "RT_VERSION":
                    return GetVersionInfoText();
                default:
                    return $"// {typeStr} 资源\n// 大小: {Data?.Length ?? 0} 字节";
            }
        }

        private string GetStringTableText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("STRINGTABLE");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    IDS_HELLO    \"Hello World\"");
            sb.AppendLine("    IDS_GOODBYE  \"Goodbye\"");
            sb.AppendLine("END");
            return sb.ToString();
        }

        private string GetDialogText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Name} DIALOG 0, 0, 200, 100");
            sb.AppendLine("STYLE DS_MODALFRAME | WS_POPUP | WS_CAPTION | WS_SYSMENU");
            sb.AppendLine("CAPTION \"对话框\"");
            sb.AppendLine("FONT 8, \"MS Shell Dlg\"");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    DEFPUSHBUTTON \"确定\", IDOK, 10, 80, 50, 14");
            sb.AppendLine("    PUSHBUTTON \"取消\", IDCANCEL, 140, 80, 50, 14");
            sb.AppendLine("END");
            return sb.ToString();
        }

        private string GetMenuText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Name} MENU");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    POPUP \"文件(&F)\"");
            sb.AppendLine("    BEGIN");
            sb.AppendLine("        MENUITEM \"新建(&N)\", ID_FILE_NEW");
            sb.AppendLine("        MENUITEM \"打开(&O)\", ID_FILE_OPEN");
            sb.AppendLine("        MENUITEM SEPARATOR");
            sb.AppendLine("        MENUITEM \"退出(&X)\", ID_FILE_EXIT");
            sb.AppendLine("    END");
            sb.AppendLine("END");
            return sb.ToString();
        }

        private string GetAcceleratorText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Name} ACCELERATORS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    \"N\", ID_FILE_NEW, VIRTKEY, CONTROL");
            sb.AppendLine("    \"O\", ID_FILE_OPEN, VIRTKEY, CONTROL");
            sb.AppendLine("    VK_F4, ID_FILE_EXIT, VIRTKEY, ALT");
            sb.AppendLine("END");
            return sb.ToString();
        }

        private string GetVersionInfoText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("1 VERSIONINFO");
            sb.AppendLine("FILEVERSION 1,0,0,0");
            sb.AppendLine("PRODUCTVERSION 1,0,0,0");
            sb.AppendLine("FILEOS 0x4");
            sb.AppendLine("FILETYPE 0x1");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    BLOCK \"StringFileInfo\"");
            sb.AppendLine("    BEGIN");
            sb.AppendLine("        BLOCK \"040904b0\"");
            sb.AppendLine("        BEGIN");
            sb.AppendLine("            VALUE \"FileDescription\", \"应用程序\"");
            sb.AppendLine("            VALUE \"FileVersion\", \"1.0.0.0\"");
            sb.AppendLine("        END");
            sb.AppendLine("    END");
            sb.AppendLine("END");
            return sb.ToString();
        }

        public void Test()
        {
            string typeStr = Type.ToString();

            switch (typeStr)
            {
                case "RT_DIALOG":
                    TestDialog();
                    break;
                case "RT_MENU":
                    TestMenu();
                    break;
                default:
                    MessageBox.ShowInfoAsync($"无法测试 {typeStr} 类型的资源", "信息",
                        MessageBoxButtons.OK);
                    break;
            }
        }

        private void TestDialog()
        {
            // 在实际实现中，这里应该创建一个实际的对话框
            MessageBox.ShowInfoAsync($"测试对话框资源: {Name}", "对话框测试",
                MessageBoxButtons.OK);
        }

        private void TestMenu()
        {
            // 在实际实现中，这里应该显示一个菜单
            MessageBox.ShowInfoAsync($"测试菜单资源: {Name}", "菜单测试",
                MessageBoxButtons.OK);
        }
    }

    // 全局资源管理器实例
    public static class g_res
    {
        private static ResourceManager s_instance = new ResourceManager();

        public static void Clear() => s_instance.Clear();
        public static void AddEntry(ResourceEntry entry) => s_instance.AddEntry(entry);
        public static ResourceEntry GetSelectedEntry() => s_instance.GetSelectedEntry();
        public static void SetSelectedEntry(ResourceEntry entry) => s_instance.SetSelectedEntry(entry);
        public static IEnumerable<ResourceEntry> GetEntries() => s_instance.GetEntries();
        public static IEnumerable<IGrouping<string, ResourceEntry>> GroupByType() => s_instance.GroupByType();
        public static List<ResourceEntry> Search(string type = null, string name = null, ushort? language = null)
            => s_instance.Search(type, name, language);
        public static void LoadFromExecutable(string filename) => s_instance.LoadFromExecutable(filename);
        public static void LoadFromResFile(string filename) => s_instance.LoadFromResFile(filename);
        public static void LoadFromRcFile(string filename) => s_instance.LoadFromRcFile(filename);
    }
}