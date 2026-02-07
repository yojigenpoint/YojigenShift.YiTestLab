using Godot;
using YojigenShift.YiFramework.Core;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;

public partial class CharacterTiles : PanelContainer
{
	private Label _mainText;
	private Label _subText;
	private ColorRect _colorStrip;

	public override void _Ready()
	{
		_mainText = GetNode<Label>("VBoxContainer/Label_Main");
		_subText = GetNode<Label>("VBoxContainer/Label_Sub");
		_colorStrip = GetNode<ColorRect>("VBoxContainer/ColorRect");
	}

	public void Setup(HeavenlyStem stem)
	{
		_mainText.Text = stem.GetLocalizedName();
		_subText.Text = $"({stem.GetPolarity().GetLocalizedName()}) {stem.GetWuXing().GetLocalizedName()}";
		_colorStrip.Color = Helpers.GetColorForWuXing(stem.GetWuXing());
	}

	public void Setup(EarthlyBranch branch)
	{
		_mainText.Text = branch.GetLocalizedName();
		_subText.Text = $"{branch.GetZodiacKey()}";
		_colorStrip.Color = Helpers.GetColorForWuXing(branch.GetWuXing());
	}

	public void Setup(int index)
	{
		var ganzhi = GanZhiMath.Mod60(index);

		_mainText.Text = $"{GanZhiMath.GetStem(ganzhi).GetLocalizedName()} {GanZhiMath.GetBranch(ganzhi).GetLocalizedName()}";
		_subText.Text = $"#{ganzhi + 1}";
		_colorStrip.Color = new Color(0, 0, 0, 0);
	}
}
