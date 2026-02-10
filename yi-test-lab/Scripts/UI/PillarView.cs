using Godot;
using System;
using YojigenShift.YiFramework.Core;
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

	public void Setup(string title, int index, HeavenlyStem? dayMasterStem = null)
	{
		var stem = GanZhiMath.GetStem(index);
		var branch = GanZhiMath.GetBranch(index);

		_title.Text = title;
		_stem.Text = stem.GetLocalizedName();
		_branch.Text = branch.GetLocalizedName();
		_tenGods.Text = dayMasterStem.HasValue ? 
			"DAY_MASTER" : BaziHelpers.CalculateTenGod(dayMasterStem.Value, stem).ToString();

		foreach(var hid in branch.GetHiddenStems())
		{
			_hidden.Text += hid.Stem.GetLocalizedName() + "\n";
		}
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
