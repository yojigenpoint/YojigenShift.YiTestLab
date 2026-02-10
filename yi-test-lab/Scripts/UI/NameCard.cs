using Godot;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiFramework.Naming.Models;

public partial class NameCard : PanelContainer
{
	private Label _lblName;
	private Label _lblScore;
	private Label _lblPattern;
	private Label _lblTags;

	public override void _Ready()
	{
		_lblName = GetNode<Label>("HBoxContainer/Label_Name");
		_lblScore = GetNode<Label>("HBoxContainer/Label_Score");
		_lblPattern = GetNode<Label>("HBoxContainer/Label_Pattern");
		_lblTags = GetNode<Label>("HBoxContainer/Label_Tags");
	}

	public void Setup(NamingCandidate result)
	{
		string pattern = "";
		foreach (var type in result.Pattern.Sequence)
			pattern += type.GetLocalizedName() + " ";

		_lblName.Text = result.FullName;
		_lblScore.Text = $"{result.TotalScore}";
		_lblPattern.Text = result.Pattern.Description;
		_lblTags.Text = string.Join(" ", result.Warnings);

		if (string.IsNullOrWhiteSpace(_lblTags.Text))
			_lblTags.Text = "格局大吉";
	}
}
