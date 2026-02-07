using Godot;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;

public partial class ElementCard : PanelContainer
{
	private Label _title;
	private Label _lblGenerates;
	private Label _lblGeneratedby;
	private Label _lblOvercomes;
	private Label _lblOvercomeby;

	private WuXingType _currentType;

	public override void _Ready()
	{
		_title = GetNode<Label>("VBoxContainer/Label_Title");
		_lblGenerates = GetNode<Label>("VBoxContainer/Label_Generates");
		_lblGeneratedby = GetNode<Label>("VBoxContainer/Label_Generatedby");
		_lblOvercomes = GetNode<Label>("VBoxContainer/Label_Overcomes");
		_lblOvercomeby = GetNode<Label>("VBoxContainer/Label_Overcomeby");
	}

	public void Setup(WuXingType type)
	{
		_currentType = type;
		RefreshUI();
	}

	public void RefreshUI()
	{
		_title.Text = $"{_currentType.GetLocalizedName()}";

		_lblGenerates.Text = $"{_currentType.Child().GetLocalizedName()}";
		_lblGeneratedby.Text = $"{_currentType.Mother().GetLocalizedName()}";
		_lblOvercomes.Text = $"{_currentType.Prisoner().GetLocalizedName()}";
		_lblOvercomeby.Text = $"{_currentType.Bane().GetLocalizedName()}";

		Modulate = Helpers.GetColorForWuXing(_currentType);
	}
}
