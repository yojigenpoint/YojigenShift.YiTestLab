using Godot;
using System.Globalization;
using YojigenShift.YiFramework.Core;

public partial class MainLayout : Node
{
	private TabContainer _contentTabs;
	private OptionButton _langSelector;
	private Button _btnWuxing;
	private Button _btnGanzhi;

	public override void _Ready()
	{
		_contentTabs = GetNode<TabContainer>("AppSplit/ContentTabs");

		_langSelector = GetNode<OptionButton>("AppSplit/Sidebar/LanguageSelector");

		_btnWuxing = GetNode<Button>("AppSplit/Sidebar/Btn_WuXing");
		_btnGanzhi = GetNode<Button>("AppSplit/Sidebar/Btn_GanZhi");

		_langSelector.ItemSelected += OnLanugageChanged;

		_btnWuxing.Pressed += () => SwitchTab(0);
		_btnGanzhi.Pressed += () => SwitchTab(1);

		TranslationServer.SetLocale("zh_CN");
		YiLocalization.CurrentLanguage = "zh_CN";
		_langSelector.Selected = 0;
	}

	private void OnLanugageChanged(long index)
	{
		string langCode = index == 0 ? "zh_CN" : "en_US";

		TranslationServer.SetLocale(langCode);
		YiLocalization.CurrentLanguage = langCode;

		var culture = new CultureInfo(langCode == "zh" ? "zh-CN" : "en-US");
		CultureInfo.CurrentCulture = culture;
		CultureInfo.CurrentUICulture = culture;

		foreach (Node child in _contentTabs.GetChildren())
		{
			if (child is WuXingModule wuxing)
				wuxing.RefreshLocalization();
			else if (child is GanzhiModule ganzhi)
				ganzhi.RefreshLocalization();
		}
	}

	private void SwitchTab(int index)
	{
		_contentTabs.CurrentTab = index;
	}
}
