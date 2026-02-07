using Godot;
using System;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;

public partial class PillarView : VBoxContainer
{
	private Label _title;
	private Label _tenGods;
	private Label _stem;
	private Label _branch;
	private Label _hidden;

	public override void _Ready()
	{
		_title = GetNode<Label>("Label_Title");
		_stem = GetNode<Label>("Label_TenGods");
		_branch = GetNode<Label>("PanelContainer/VBoxContainer/Label_Stem");
		_tenGods = GetNode<Label>("PanelContainer/VBoxContainer/Label_Branch");
		_hidden = GetNode<Label>("Label_Hidden");
	}

	public void Setup(string title, HeavenlyStem s, EarthlyBranch b, string tenGodText, string hiddenText)
	{
		_title.Text = title;
		_stem.Text = s.GetLocalizedName();
		_branch.Text = b.GetLocalizedName();
		_tenGods.Text = tenGodText;
		_hidden.Text = hiddenText;
	}

	public void Clear()
	{
		_title.Text = "-";
		_stem.Text = "-";
		_branch.Text = "-";
		_tenGods.Text = "-";
		_hidden.Text = "-";
	}
}
