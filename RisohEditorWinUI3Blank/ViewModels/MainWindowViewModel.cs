using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Content;

namespace RisohEditorWinUI3Blank.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        // 初始显示标题
        [ObservableProperty]
        private string contentTitle = "欢迎";

        // 初始显示正文
        [ObservableProperty]
        private string contentText = "在这里显示选中节点的内容。";

        // 被选中节点后更新内容（被视图调用）
        public void SelectNode(string nodeText)
        {
            ContentTitle = nodeText ?? "<空>";
            ContentText = $"已选择：{ContentTitle}\n\n这是示例内容区。你可以在此根据节点加载真实视图或页面。";
        }
    }
}