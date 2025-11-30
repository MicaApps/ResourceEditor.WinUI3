using DevWinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging; 
using RisohEditorWinUI3Blank.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;

namespace RisohEditorWinUI3Blank
{
    public sealed partial class MainWindow : Window
    {
        private bool _sidebarVisible = true;
        IntPtr hWnd=IntPtr.Zero;
        public MainWindow()
        {
            this.InitializeComponent();

            hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            g_hMainWnd = WinRT.Interop.WindowNative.GetWindowHandle(this); ;
            //ParseCommandLine(args);

            InitializeNavigationTree();
        }
        #region 暂时保留的内容

        private void InitializeNavigationTree()
        {
            // 示例节点：可以根据实际数据动态构建
            var root1 = new TreeViewNode { Content = "项目 A" };
            root1.Children.Add(new TreeViewNode { Content = "文件 A1.txt" });
            root1.Children.Add(new TreeViewNode { Content = "文件 A2.txt" });

            var root2 = new TreeViewNode { Content = "项目 B" };
            root2.Children.Add(new TreeViewNode { Content = "子项 B1" });
            var sub = new TreeViewNode { Content = "子项 B2" };
            sub.Children.Add(new TreeViewNode { Content = "文件 B2-1.md" });
            root2.Children.Add(sub);

            m_treeView.RootNodes.Clear();
            m_treeView.RootNodes.Add(root1);
            m_treeView.RootNodes.Add(root2);
        }

        private async void NavigationTree_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            // InvokedItem 通常为节点的 Content（此处我们使用字符串）
            var invoked = args.InvokedItem;
            var text = invoked?.ToString() ?? "<空>";
            ContentTitle.Text = text;
            m_codeEditor.Text = $"已选择：{text}\n\n这是示例内容区。你可以在此根据节点加载真实视图或页面。";
        }

        // 以下为已有菜单处理器（示例占位）
        private async void New_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "新建", Content = "新建（示例）", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "打开", Content = "打开（示例）", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "保存", Content = "保存（示例）", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            try { this.Close(); } catch (Exception ex) { Debug.WriteLine(ex); }
        }

        private async void Undo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "撤销", Content = "撤销（示例）", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private async void Redo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "重做", Content = "重做（示例）", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private async void Cut_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "剪切", Content = "剪切（示例）", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private async void Copy_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "复制", Content = "复制（示例）", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private async void Paste_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "粘贴", Content = "粘贴（示例）", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private async void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            var col = RootGrid.ColumnDefinitions[0];
            if (_sidebarVisible)
            {
                col.Width = new Microsoft.UI.Xaml.GridLength(0);
            }
            else
            {
                col.Width = new Microsoft.UI.Xaml.GridLength(3, Microsoft.UI.Xaml.GridUnitType.Star);
            }
            _sidebarVisible = !_sidebarVisible;

            var dlg = new ContentDialog { Title = "视图", Content = $"侧边栏：{(_sidebarVisible ? "可见" : "隐藏")}", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private async void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "放大", Content = "放大（示例）", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private async void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "缩小", Content = "缩小（示例）", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private async void Preferences_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "首选项", Content = "首选项（示例）", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }

        private async void About_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ContentDialog { Title = "关于", Content = "RisohEditorWinUI3Blank - 示例菜单", CloseButtonText = "确定" };
            await dlg.ShowAsync();
        }
        #endregion

        private const int TV_WIDTH = 250;
        private const int BV_WIDTH = 160;
        private const int BE_HEIGHT = 90;
        private const int CX_STATUS_PART = 80;

        private static bool s_bModified = false;
        public static IntPtr g_hMainWnd = IntPtr.Zero;
        private static int s_ret = 0;

        private FileType m_file_type = FileType.FT_NONE;
        private string m_szFile = "";
        private string m_szResourceH = "";
        private bool m_bUpxCompressed = false;

        // 选择状态
        private MIdOrString m_type;
        private MIdOrString m_name;
        private ushort m_lang;

        // 设置
        private RisohSettings g_settings = new RisohSettings();
        private bool DoExportRC(string filename)
        {
            try
            {
                StringBuilder rcContent = new StringBuilder();

                // 生成 RC 文件内容
                rcContent.AppendLine("// 由 RisohEditor 生成的资源脚本");
                rcContent.AppendLine();

                foreach (var entry in g_res.GetEntries())
                {
                    string typeStr = entry.Type.ToString();
                    string nameStr = entry.Name.ToString();

                    rcContent.AppendLine($"{nameStr} {typeStr} \"{filename}\"");
                }

                File.WriteAllText(filename, rcContent.ToString(), Encoding.Default);

                // 同时生成 resource.h 文件
                string resourceHPath = Path.ChangeExtension(filename, ".h");
                GenerateResourceH(resourceHPath);

                MessageBox.ShowInfoAsync($"成功导出 RC 文件: {filename}", "导出完成",
                    MessageBoxButtons.OK);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.ShowErrorAsync($"导出 RC 文件时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK);
                return false;
            }
        }
        private void GenerateResourceH(string filepath)
        {
            try
            {
                StringBuilder hContent = new StringBuilder();

                hContent.AppendLine("//{{NO_DEPENDENCIES}}");
                hContent.AppendLine("// Microsoft Visual C++ generated include file.");
                hContent.AppendLine("// 由 RisohEditor 生成");
                hContent.AppendLine();

                // 添加资源 ID 定义
                var resourceIds = g_res.GetEntries()
                    .Select(e => e.Name)
                    .Where(n => n.IsInt && n.Id > 0)
                    .Distinct();

                foreach (var id in resourceIds)
                {
                    hContent.AppendLine($"#define {id.Id}\t\t\t\t{id.Id}");
                }

                File.WriteAllText(filepath, hContent.ToString(), Encoding.Default);
            }
            catch (Exception ex)
            {
                MessageBox.ShowWarningAsync($"生成 resource.h 时出错: {ex.Message}", "警告",
                    MessageBoxButtons.OK);
            }
        }
        private bool DoSaveExeAs(string filename)
        {
            try
            {
                // 使用资源更新API
                // 检查文件是否被占用
                if (File.Exists(filename))
                {
                    try
                    {
                        using (FileStream fs = File.OpenWrite(filename))
                        {
                            //这里待完善----
                            MessageBox.ShowErrorAsync($"这里待完善!!!!", "错误",
                    MessageBoxButtons.OK);
                        }
                    }
                    catch
                    {
                        MessageBox.ShowErrorAsync($"文件被占用，无法保存: {filename}", "错误",
                            MessageBoxButtons.OK);
                        return false;
                    }
                }
                MessageBox.ShowInfoAsync($"成功保存可执行文件: {filename}", "保存完成",
                    MessageBoxButtons.OK);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.ShowErrorAsync($"保存可执行文件时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK);
                return false;
            }
        }
        private bool DoSaveResAs(string filename)
        {
            try
            {
                // 这里应该生成 .res 文件格式
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    // 写入 RES 文件头和数据
                    // 完整的 RES 文件格式
                    MessageBox.ShowErrorAsync($"这里待完善!!!!", "错误",
                 MessageBoxButtons.OK);
                }

                MessageBox.ShowInfoAsync($"成功保存资源文件: {filename}", "保存完成",
                    MessageBoxButtons.OK);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.ShowErrorAsync($"保存资源文件时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK);
                return false;
            }
        }


        private void ParseCommandLine(string[] args)
        {
            if (args.Length > 0)
            {
                string filename = args[0];
                if (File.Exists(filename))
                {
                    DoLoadFile(filename);
                }
            }
        }

        private async Task OnNew()
        {
            if (!await DoQuerySaveChange()) return;

            g_res.Clear();
            m_treeView.RootNodes.Clear();
            m_codeEditor.Text = ""; 
            m_hexViewer.Text = ""; 

            UpdateFileInfo(FileType.FT_NONE, null, false);
            DoSetFileModified(false);
        }

        async Task OnOpen()
        {
            var picker = new FileOpenPicker()
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Desktop
            };

            // 按需添加过滤器
            picker.FileTypeFilter.Clear();
            picker.FileTypeFilter.Add(".exe");
            picker.FileTypeFilter.Add(".dll");
            picker.FileTypeFilter.Add(".res");
            picker.FileTypeFilter.Add(".rc");
            picker.FileTypeFilter.Add("*");

            // 将 WinUI 窗口句柄传入 Picker（必要）
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // 如果需要绝对路径（桌面应用通常可用），可用 file.Path
                string path = file.Path;
                // 调用现有加载逻辑
                DoLoadFile(path);
            }
        }


        //private async Task OnOpen()
        //{
        //    if (!await DoQuerySaveChange()) return;

        //    using (OpenFileDialog dlg = new OpenFileDialog())
        //    {
        //        dlg.Filter = "可执行文件 (*.exe;*.dll;*.ocx;*.cpl;*.scr)|*.exe;*.dll;*.ocx;*.cpl;*.scr|资源文件 (*.res)|*.res|资源脚本 (*.rc)|*.rc|所有文件 (*.*)|*.*";
        //        dlg.Title = "打开文件";

        //        if (dlg.ShowDialog() == DialogResult.OK)
        //        {
        //            DoLoadFile(dlg.FileName);
        //        }
        //    }
        //}

        private async Task<bool> OnSave()
        {
            if (string.IsNullOrEmpty(m_szFile))
            {
                return await OnSaveAs();
            }

            try
            {
                switch (m_file_type)
                {
                    case FileType.FT_EXECUTABLE:
                        return DoSaveExeAs(m_szFile);
                    case FileType.FT_RES:
                        return DoSaveResAs(m_szFile);
                    case FileType.FT_RC:
                        return DoExportRC(m_szFile);
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                await MessageBox.ShowErrorAsync($"保存文件时出错: {ex.Message}", "错误",
                    MessageBoxButtons.OK);
                return false;
            }
        }

        async Task<bool> OnSaveAs()
        {
            try
            {
                var picker = new FileSavePicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                InitializeWithWindow.Initialize(picker, hwnd);
                // 必须设置至少一个文件类型
                picker.FileTypeChoices.Add("可执行文件(*.exe)", new[] { @".exe" });
                picker.FileTypeChoices.Add("动态链接库(*.dll)", new[] { @".dll" });
                //savePicker.FileTypeChoices.Add("可执行文件", new[] { @"*.exe;*.dll;*.ocx;*.cpl;*.scr" });
                picker.SuggestedFileName = "Untitled";

                StorageFile file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    // 用户选择了文件，可以写入内容
                    await FileIO.WriteTextAsync(file, "这是保存的内容。");
                    // 可选：提示用户保存成功
                    // 例如使用 ContentDialog 或 InfoBar
                    return true;
                }
                else
                {
                    // 用户取消了操作
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //private bool OnSaveAs()
        //{
        //    using (SaveFileDialog dlg = new SaveFileDialog())
        //    {
        //        dlg.Filter = "可执行文件 (*.exe;*.dll;*.ocx;*.cpl;*.scr)|*.exe;*.dll;*.ocx;*.cpl;*.scr|资源文件 (*.res)|*.res|资源脚本 (*.rc)|*.rc";
        //        dlg.Title = "另存为";

        //        if (dlg.ShowDialog() == DialogResult.OK)
        //        {
        //            m_szFile = dlg.FileName;
        //            return OnSave();
        //        }
        //    }
        //    return false;
        //}

        private void OnGuiEdit()
        {
            var entry = g_res.GetSelectedEntry();
            if (entry == null || !entry.CanGuiEdit()) return;

            switch (entry.Type.ToString())
            {
                case "RT_DIALOG":
                    ShowDialogEditor(entry);
                    break;
                case "RT_MENU":
                    ShowMenuEditor(entry);
                    break;
            }
        }

        #region 工具按钮事件处理程序
        private void m_newButton_Click(object sender, EventArgs e)
        {
            OnNew();
        }

        private void m_openButton_Click(object sender, EventArgs e)
        {
            OnOpen();
        }

        private void m_saveButton_Click(object sender, EventArgs e)
        {
            OnSave();
        }

        private void m_saveAsButton_Click(object sender, EventArgs e)
        {
            OnSaveAs();
        }

        private void m_exitButton_Click(object sender, EventArgs e)
        {
            //this.Close();
        }

        private void m_extractButton_Click(object sender, EventArgs e)
        {

        }

        private void m_replaceButton_Click(object sender, EventArgs e)
        {

        }

        private void m_deleteButton_Click(object sender, EventArgs e)
        {

        }

        private void m_addButton_Click(object sender, EventArgs e)
        {

        }

        private void m_searchButton_Click(object sender, EventArgs e)
        {

        }

        private void m_aboutButton_Click(object sender, EventArgs e)
        {

        }
        #endregion

        private void OnTest()
        {
            var entry = g_res.GetSelectedEntry();
            if (entry != null)
            {
                entry.Test();
            }
        }

        private void ShowDialogEditor(ResourceEntry entry)
        {
            //DialogEditorForm editor = new DialogEditorForm(entry);
            //if (editor.ShowDialog() == DialogResult.OK)
            //{
            //    entry.Data = editor.GetResourceData();
            //    DoSetFileModified(true);
            //    SelectResource(entry);
            //}
        }
        private void ShowMenuEditor(ResourceEntry entry)
        {
            //MenuEditorForm editor = new MenuEditorForm(entry);
            //if (editor.ShowDialog() == DialogResult.OK)
            //{
            //    entry.Data = editor.GetResourceData();
            //    DoSetFileModified(true);
            //    SelectResource(entry);
            //}
        }

        /// <summary>
        /// 加载文件的主要方法
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="filterIndex">过滤器索引</param>
        /// <param name="forceDecompress">是否强制解压缩</param>
        /// <returns>是否加载成功</returns>
        private bool DoLoadFile(string fileName, uint filterIndex = 0, bool forceDecompress = false)
        {
            // 显示等待光标
            //using (var waitCursor = new WaitCursor())
            {
                string resolvedPath;
                string fullPath;
                if (GetPathOfShortcut(fileName, out resolvedPath))
                {
                    fullPath = Path.GetFullPath(resolvedPath);
                    filterIndex = (uint)ApiCalls.LoadFilterIndex.LFI_NONE;
                }
                else
                {
                    fullPath = Path.GetFullPath(fileName);
                }

                string extension = Path.GetExtension(fullPath)?.ToLower();

                if (!string.IsNullOrEmpty(extension))
                {
                    switch ((ApiCalls.LoadFilterIndex)filterIndex)
                    {
                        case ApiCalls.LoadFilterIndex.LFI_NONE:
                        case ApiCalls.LoadFilterIndex.LFI_ALL:
                        case ApiCalls.LoadFilterIndex.LFI_LOADABLE:
                            filterIndex = (uint)ApiCalls.LoadFilterIndex.LFI_NONE;

                            if (extension == ".res")
                                filterIndex = (uint)ApiCalls.LoadFilterIndex.LFI_RES;
                            else if (extension == ".rc" || extension == ".rc2")
                                filterIndex = (uint)ApiCalls.LoadFilterIndex.LFI_RC;
                            else if (IsExecutableExtension(extension))
                                filterIndex = (uint)ApiCalls.LoadFilterIndex.LFI_EXECUTABLE;
                            break;
                    }
                }

                switch ((ApiCalls.LoadFilterIndex)filterIndex)
                {
                    case ApiCalls.LoadFilterIndex.LFI_RES:
                        return DoLoadRES(fullPath);
                    case ApiCalls.LoadFilterIndex.LFI_RC:
                        return DoLoadRC(fullPath);
                    default:
                        return DoLoadEXE(fullPath, forceDecompress);
                }
            }
        }

        /// <summary>
        /// 检查是否是可执行文件扩展名
        /// </summary>
        private bool IsExecutableExtension(string extension)
        {
            string[] executableExtensions =
            {
                ".exe", ".dll", ".ocx", ".cpl", ".scr", ".mui", ".ime"
            };

            foreach (string ext in executableExtensions)
            {
                if (extension == ext)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 解析快捷方式文件
        /// </summary>
        private bool GetPathOfShortcut(string lnkFile, out string resolvedPath)
        {
            resolvedPath = null;

            try
            {
                if (Path.GetExtension(lnkFile)?.ToLower() != ".lnk")
                    return false;

                if (!File.Exists(lnkFile))
                    return false;
                resolvedPath = ResolveShortcut(lnkFile);
                return !string.IsNullOrEmpty(resolvedPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析快捷方式失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 使用Windows Shell解析快捷方式
        /// </summary>
        private string ResolveShortcut(string lnkFile)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType != null)
                {
                    dynamic shell = Activator.CreateInstance(shellType);
                    dynamic shortcut = shell.CreateShortcut(lnkFile);
                    string targetPath = shortcut.TargetPath;
                    Marshal.ReleaseComObject(shortcut);
                    Marshal.ReleaseComObject(shell);
                    return targetPath;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WScript.Shell方法失败: {ex.Message}");
            }

            try
            {
                return ResolveShortcutWithAPI(lnkFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"API方法失败: {ex.Message}");
            }

            return null;
        }



        private string ResolveShortcutWithAPI(string lnkFile)
        {

            return lnkFile;
        }
        /// <summary>
        /// 加载RES文件
        /// </summary>
        private bool DoLoadRES(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.ShowErrorAsync($"文件不存在: {filePath}", "错误",
                        MessageBoxButtons.OK);
                    return false;
                }

                g_res.Clear();

                // 该解析.res文件格式，待完成！
                byte[] fileData = File.ReadAllBytes(filePath);
                AddSampleResourcesForResFile(filePath);
                UpdateFileInfo(FileType.FT_RES, filePath, false);
                PopulateTreeView();
                ChangeStatusText($"已加载RES文件: {Path.GetFileName(filePath)}");
                DoSetFileModified(false);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.ShowErrorAsync($"加载RES文件失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK);
                return false;
            }
        }

        /// <summary>
        /// 加载RC文件
        /// </summary>
        private bool DoLoadRC(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.ShowErrorAsync($"文件不存在: {filePath}", "错误",
                        MessageBoxButtons.OK);
                    return false;
                }

                g_res.Clear();

                string rcContent = File.ReadAllText(filePath, Encoding.Default);
                // 具体需要实现解析RC脚本语法，待测
                ParseRcFileContent(rcContent, filePath);

                UpdateFileInfo(FileType.FT_RC, filePath, false);

                PopulateTreeView();

                ChangeStatusText($"已加载RC文件: {Path.GetFileName(filePath)}");
                DoSetFileModified(false);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.ShowErrorAsync($"加载RC文件失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK);
                return false;
            }
        }

        /// <summary>
        /// 加载EXE/DLL等可执行文件
        /// </summary>
        private bool DoLoadEXE(string filePath, bool forceDecompress)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.ShowErrorAsync($"文件不存在: {filePath}", "错误",
                        MessageBoxButtons.OK);
                    return false;
                }

                if (forceDecompress || IsCompressedExecutable(filePath))
                {
                    if (!DecompressExecutable(filePath))
                    {
                        MessageBox.ShowErrorAsync("文件解压缩失败", "错误",
                            MessageBoxButtons.OK);
                        return false;
                    }
                }

                g_res.Clear();

                if (!LoadResourcesFromExecutable(filePath))
                {
                    MessageBox.ShowErrorAsync("无法从可执行文件中读取资源", "错误",
                        MessageBoxButtons.OK);
                    return false;
                }

                UpdateFileInfo(FileType.FT_EXECUTABLE, filePath, IsCompressedExecutable(filePath));

                PopulateTreeView();

                ChangeStatusText($"已加载: {Path.GetFileName(filePath)}");
                DoSetFileModified(false);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.ShowErrorAsync($"加载可执行文件失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK);
                return false;
            }
        }
        /// <summary>
        /// 检查可执行文件是否被压缩
        /// </summary>
        private bool IsCompressedExecutable(string filePath)
        {
            try
            {
                // 简单的检查方法：读取文件头判断是否是UPX压缩
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(fs))
                {
                    // 检查UPX签名
                    byte[] header = reader.ReadBytes(4);
                    if (header.Length >= 3)
                    {
                        // UPX签名: "UPX"
                        if (header[0] == 0x55 && header[1] == 0x50 && header[2] == 0x58)
                            return true;
                    }
                }

                // 还可以添加其他压缩格式的检查=》待补充

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 解压缩可执行文件
        /// </summary>
        private bool DecompressExecutable(string filePath)
        {
            return false;
            //try
            //{
            //    // 调用UPX或其他解压工具,待实现
            //    // 暂时返回true表示成功
            //    MessageBox.Show("检测到压缩的可执行文件，请手动解压后重新加载", "信息",
            //        MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return false; // 暂时不支持自动解压
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"解压缩失败: {ex.Message}", "错误",
            //        MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return false;
            //}
        }
        /// <summary>
        /// 使用Windows API从可执行文件加载资源
        /// </summary>
        private bool LoadResourcesFromExecutable(string filePath)
        {
            try
            {
                // 使用LoadLibraryEx以数据文件方式加载
                IntPtr hModule = ApiCalls.LoadLibraryEx(filePath, IntPtr.Zero,
                    (uint)ApiCalls.LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE);

                if (hModule == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"无法加载模块 (错误代码: {error})");
                    return false;
                }

                try
                {
                    // 枚举资源类型 - 使用匿名委托
                    ApiCalls.EnumResTypeProcDelegate enumTypeProc = delegate (IntPtr hMod, IntPtr type, IntPtr lParam)
                    {
                        // 枚举指定类型的资源名称
                        ApiCalls.EnumResNameProcDelegate enumNameProc = delegate (IntPtr hMod2, IntPtr type2, IntPtr name, IntPtr lParam2)
                        {
                            // 枚举指定资源的语言
                            ApiCalls.EnumResLangProcDelegate enumLangProc = delegate (IntPtr hMod3, IntPtr type3, IntPtr name3, ushort lang, IntPtr lParam3)
                            {
                                return EnumResLangProcImpl(hMod3, type3, name3, lang, lParam3);
                            };

                            return ApiCalls.EnumResourceLanguages(hMod2, type2, name, enumLangProc, IntPtr.Zero);
                        };

                        return ApiCalls.EnumResourceNames(hMod, type, enumNameProc, IntPtr.Zero);
                    };

                    if (!ApiCalls.EnumResourceTypes(hModule, enumTypeProc, IntPtr.Zero))
                    {
                        System.Diagnostics.Debug.WriteLine("枚举资源类型失败");
                        return false;
                    }
                    return true;
                }
                finally
                {
                    ApiCalls.FreeLibrary(hModule);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载资源失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 资源语言枚举回调的具体实现
        /// </summary>
        private bool EnumResLangProcImpl(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, ushort wIDLanguage, IntPtr lParam)
        {
            try
            {
                // 加载资源数据
                IntPtr hResource = ApiCalls.FindResourceEx(hModule, lpszType, lpszName, wIDLanguage);
                if (hResource == IntPtr.Zero)
                    return true;

                IntPtr hGlobal = ApiCalls.LoadResource(hModule, hResource);
                if (hGlobal == IntPtr.Zero)
                    return true;

                IntPtr pData = ApiCalls.LockResource(hGlobal);
                if (pData == IntPtr.Zero)
                    return true;

                uint size = ApiCalls.SizeofResource(hModule, hResource);
                if (size == 0)
                    return true;

                // 复制资源数据
                byte[] data = new byte[size];
                Marshal.Copy(pData, data, 0, (int)size);

                // 创建资源条目
                var entry = new ResourceEntry
                {
                    Type = GetResourceIdOrString(lpszType),
                    Name = GetResourceIdOrString(lpszName),
                    Language = wIDLanguage,
                    Data = data
                };

                g_res.AddEntry(entry);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"处理资源失败: {ex.Message}");
                return true;
            }
        }

        /// <summary>
        /// 将资源标识符转换为MIdOrString
        /// </summary>
        private MIdOrString GetResourceIdOrString(IntPtr lpString)
        {
            // 如果高位为0，则是整数ID
            if (((ulong)lpString >> 16) == 0)
            {
                return new MIdOrString { Id = (ushort)lpString };
            }
            else
            {
                // 否则是字符串
                string str = Marshal.PtrToStringAuto(lpString);
                return new MIdOrString { String = str };
            }
        }

        // 添加示例资源的方法-》用于测试
        private void AddSampleResourcesForExeFile(string filePath)
        {
            var sampleEntries = new[]
            {
                new ResourceEntry
                {
                    Type = new MIdOrString { String = "RT_DIALOG" },
                    Name = new MIdOrString { Id = 100 },
                    Language = 0x0409,
                    Data = new byte[256] // 模拟对话框资源数据
                },
                new ResourceEntry
                {
                    Type = new MIdOrString { String = "RT_MENU" },
                    Name = new MIdOrString { Id = 100 },
                    Language = 0x0409,
                    Data = new byte[128] // 模拟菜单资源数据
                }
            };

            foreach (var entry in sampleEntries)
            {
                g_res.AddEntry(entry);
            }
        }

        private void AddSampleResourcesForResFile(string filePath)
        {
            // 这里准备实现RES文件资源
        }

        private void ParseRcFileContent(string content, string filePath)
        {
            // 解析RC文件内容的简单实现
            // 在实际应用中需要完整的RC语法解析器
            // 这里添加资源
            AddSampleResourcesForExeFile(filePath);
        }

        private string GetResourceDisplayName(ResourceEntry resource)
        {
            string name = resource.Name.ToString();
            string type = resource.Type.ToString();

            if (resource.Name.IsInt)
            {
                switch (type)
                {
                    case "RT_DIALOG":
                        return $"对话框 ({name})";
                    case "RT_MENU":
                        return $"菜单 ({name})";
                    case "RT_STRING":
                        return $"字符串表 ({name})";
                    case "RT_ACCELERATOR":
                        return $"加速键 ({name})";
                    case "RT_VERSION":
                        return $"版本信息 ({name})";
                    case "RT_BITMAP":
                        return $"位图 ({name})";
                    case "RT_ICON":
                        return $"图标 ({name})";
                    case "RT_CURSOR":
                        return $"光标 ({name})";
                    default:
                        return $"{type} ({name})";
                }
            }
            else
            {
                return $"{type}: {name}";
            }
        }

        public struct DataStruct
        {
            public object Tag;
            public string Content;
            public string ImageKey;
            public string SelectedImageKey;
        }


        // 树视图填充
        private void PopulateTreeView()
        {
            //m_treeView.BeginUpdate();
            //m_treeView.Nodes.Clear();

            try
            {
                // 按类型组织资源
                var resourcesByType = g_res.GroupByType();

                foreach (var group in resourcesByType)
                {
                    var dataRoot = new DataStruct();
                    dataRoot.Tag = group.Key;
                    dataRoot.Content = $"{group.Key} ({group.Count()})";
                    var typeNode = new TreeViewNode() { Content= dataRoot }; 
                    m_treeView.RootNodes.Add(typeNode); 
                    foreach (var resource in group)
                    {
                        string resourceName = GetResourceDisplayName(resource);
                        TreeViewNode resourceNode = new TreeViewNode() { Content= resourceName };
                        var dataNode = new DataStruct();
                        dataNode.Tag = resource;
                        dataNode.ImageKey = "resource";
                        dataNode.SelectedImageKey = "resource";
                        typeNode.Children.Add(resourceNode);
                    }

                    typeNode.IsExpanded = false;
                }

                if (m_treeView.RootNodes.Count == 0)
                {
                    m_treeView.RootNodes.Add(new TreeViewNode() {Content= "无资源" });
                }
            }
            finally
            {
                //m_treeView.EndUpdate();
            }
        }
        //private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        //{
        //    if (e.Node?.Tag is ResourceEntry entry)
        //    {
        //        SelectResource(entry);
        //    }
        //}

        //private void TreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        //{
        //    if (e.Node?.Tag is ResourceEntry entry)
        //    {
        //        SelectResource(entry, true);
        //    }
        //}

        private void m_treeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs e)
        {
            
            if (sender.SelectedNode.Content is ResourceEntry entry)
            {
                SelectResource(entry);
            }
        }

        private void m_treeView_SelectionChanged_1(TreeView sender, TreeViewSelectionChangedEventArgs e)
        {
            if (sender.SelectedNode.Content is ResourceEntry entry)
            {
                SelectResource(entry, true);
            }
        }

        private void SelectResource(ResourceEntry entry, bool doubleClick = false)
        {
            if (entry == null) return;

            g_res.SetSelectedEntry(entry);

            if (entry.CanShowAsText)
            {
                m_codeEditor.Text = entry.GetText();
                m_codeEditor.IsEnabled = true;
                m_codeEditor.Background = new SolidColorBrush(Colors.White);//SystemColors.Window
            }
            else
            {
                m_codeEditor.Text = $"// 此资源类型 ({entry.Type}) 无法以文本形式显示\n// 大小: {entry.Data?.Length ?? 0} 字节";
                m_codeEditor.IsEnabled = false;
                m_codeEditor.Background = new SolidColorBrush(Colors.Black);//SystemColors.Control
            }

            m_hexViewer.Text = entry.Data != null ? DumpBinaryAsText(entry.Data) : "// 无数据";

            if (entry.IsImage && entry.Data != null)
            {
                ShowBitmap(entry.Data);
            }
            else
            {
                //MyTool.BitmapHelper.ParseAndDisplayBitmap(entry.Data, image1);
                //string debugInfo = BitmapHelper.GetDetailedAnalysis(entry.Data);
                //m_bmpView.BackgroundImage = null;
                //m_bmpView.BackColor = SystemColors.Control;
            }

            ChangeStatusText($"已选择: {GetResourceDisplayName(entry)}");

            if (doubleClick && entry.CanGuiEdit())
            {
                OnGuiEdit();
            }
        }

        async Task<byte[]?> ReadFileFromLocalFolderAsync(string folder, string fileName)
        {
            try
            {
                StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(folder);// ApplicationData.Current.LocalFolder;
                StorageFile storageFile = await localFolder.GetFileAsync(fileName);
                return await ReadStorageFileAsBytesAsync(storageFile);
            }
            catch (FileNotFoundException ex)
            {
                // 文件不存在
                return null;
            }
        }


        async Task<byte[]> ReadStorageFileAsBytesAsync(StorageFile file)
        {
            using var stream = await file.OpenReadAsync();
            using var reader = new DataReader(stream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)stream.Size);
            var buffer = new byte[stream.Size];
            reader.ReadBytes(buffer);
            return buffer;
        }


        private void ChangeStatusText(string text)
        {
            //if (m_statusBar.Items.Count > 0)
            //{
            //    m_statusBar.Items[0].Text = text;
            //}
        }
        // 工具方法
        private string DumpBinaryAsText(byte[] data)
        {
            if (data == null || data.Length == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("+ADDRESS  +0 +1 +2 +3 +4 +5 +6 +7  +8 +9 +A +B +C +D +E +F  0123456789ABCDEF");
            sb.AppendLine("--------  -----------------------  -----------------------  ----------------");

            for (int i = 0; i < data.Length; i += 16)
            {
                // 地址
                sb.Append($"{i:X8}  ");

                // 十六进制数据
                for (int j = 0; j < 16; j++)
                {
                    if (i + j < data.Length)
                    {
                        sb.Append($"{data[i + j]:X2} ");
                    }
                    else
                    {
                        sb.Append("   ");
                    }

                    if (j == 7) sb.Append(" ");
                }

                sb.Append(" ");

                // ASCII 显示
                for (int j = 0; j < 16; j++)
                {
                    if (i + j < data.Length)
                    {
                        byte b = data[i + j];
                        if (b >= 32 && b <= 126)
                            sb.Append((char)b);
                        else
                            sb.Append('.');
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                }

                sb.AppendLine();
            }
             
            return sb.ToString();
        }

        private async Task ShowBitmap(byte[] data)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                {

                    var bmp =await  ToBitmapSource(ms.GetBuffer());
                    image1.Source = bmp;                     
                }
            }
            catch
            {
                image1.Source=null;
            }
        }

        async Task<BitmapSource> ToBitmapSource(byte[] data)
        {
            using(MemoryStream ms = new MemoryStream(data))
            {
                var bms = await ConverterHelper.Stream2RandomAccessStream(ms);
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(bms); 
                return bitmapImage;
            }
        }

        private async Task<bool> DoQuerySaveChange()
        {
            if (s_bModified)
            {
                MessageBoxResult result =await MessageBox.ShowQuestionAsync(
                    "文件已修改，是否保存更改？",
                    "确认",
                    MessageBoxButtons.YesNoCancel
                    );

                if (result == MessageBoxResult.CANCEL)
                    return false;

                if (result == MessageBoxResult.YES)
                    return await OnSave();
            }

            return true;
        }

        private void UpdateFileInfo(FileType ft, string file, bool compressed)
        {
            m_file_type = ft;
            m_szFile = file ?? "";
            m_bUpxCompressed = compressed;

            UpdateTitleBar();
        }

        private void UpdateTitleBar()
        {
            string title = "RisohEditor - C# 版本";
            if (!string.IsNullOrEmpty(m_szFile))
            {
                title += $" - {Path.GetFileName(m_szFile)}";
                if (s_bModified)
                    title += " *";
            }
            this.Title = title;
        }

        public static void DoSetFileModified(bool modified)
        {
            s_bModified = modified;
            if (g_hMainWnd != IntPtr.Zero)
            {

            }
        }

       

        //protected override async Task OnFormClosing(FormClosingEventArgs e)
        //{
        //    if (!await DoQuerySaveChange())
        //    {
        //        e.Cancel = true;
        //    }
        //    base.OnFormClosing(e);
        //}

        private void Window_Closed(object sender, WindowEventArgs args)
        {

        }


    }

    public enum FileType
    {
        FT_NONE,
        FT_EXECUTABLE,
        FT_RC,
        FT_RES
    }

    public class MIdOrString
    {
        public ushort Id { get; set; }
        public string String { get; set; }

        public bool IsInt { get { return String == null; } }

        public override string ToString()
        {
            return IsInt ? Id.ToString() : String;
        }
    }

    /// <summary>
    /// 等待光标辅助类
    /// </summary>
    //public class WaitCursor : IDisposable
    //{
    //    private Cursor m_previousCursor;

    //    public WaitCursor()
    //    {
    //        m_previousCursor = Cursor.Current;
    //        Cursor.Current = Cursors.WaitCursor;
    //    }

    //    public void Dispose()
    //    {
    //        Cursor.Current = m_previousCursor;
    //    }
    //}
    public class RisohSettings
    {
        public bool ShowToolBar { get; set; } = true;
        public bool ShowBinEdit { get; set; } = false;
        public bool WordWrap { get; set; } = false;
        public bool UseBeginEnd { get; set; } = true;
        public bool HideID { get; set; } = false;
    }
}