using Godot;
using System;
using System.Text;
using YojigenShift.YiFramework.Core;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiTestLab.Core;
using YojigenShift.YiTestLab.UI;

public partial class BaziModule : VBoxContainer
{
	private HBoxContainer _pillarsContainer;
	private RichTextLabel _analysisLabel;

	private GlobalUIController _globalUIController;

	public override void _Ready()
	{
		_globalUIController = GetNode<Control>("/root/MainUI") as GlobalUIController;

		if (_globalUIController == null)
			_globalUIController = this.FindParent("MainUI") as GlobalUIController;

		Name = "BaziModule";
		AddThemeConstantOverride("separation", 20);

		SetupHeader();

		SetupPillarsArea();

		SetupAnalysisArea();

		if (_globalUIController != null)
		{
			UpdateChart(_globalUIController.CurrentTime);
			_globalUIController.GlobalTimeChanged += UpdateChart;
		}
		else
		{
			UpdateChart(DateTime.UtcNow);
		}
	}

	public override void _ExitTree()
	{
		if (_globalUIController != null)
			_globalUIController.GlobalTimeChanged -= UpdateChart;
	}

	public void UpdateChart(DateTime utcTime)
	{
		try
		{
			var (yIdx, mIdx, dIdx, hIdx) = BaziPlotter.Plot(utcTime);

			var result = BaziEvaluator.Evaluate(yIdx, mIdx, dIdx, hIdx);

			RenderPillars(yIdx, mIdx, dIdx, hIdx);
			RenderAnalysis(result);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Bazi Plot Error: {ex.Message}");
			_analysisLabel.Text = $"[center][color=red]Error calculating chart: {ex.Message}[/color][/center]";
		}
	}

	private void SetupHeader()
	{
		var title = new Label
		{
			Text = Tr("MOD_BAZI_TITLE"),
			HorizontalAlignment = HorizontalAlignment.Center
		};
		title.AddThemeFontSizeOverride("font_size", 48);
		AddChild(title);
	}

	private void SetupPillarsArea()
	{
		var centerContainer = new CenterContainer();
		AddChild(centerContainer);

		_pillarsContainer = new HBoxContainer();
		_pillarsContainer.AddThemeConstantOverride("separation", 15);
		centerContainer.AddChild(_pillarsContainer);
	}

	private void SetupAnalysisArea()
	{
		var panel = new PanelContainer();
		var style = new StyleBoxFlat { BgColor = GlobalUIController.ColorSurface, CornerRadiusTopLeft = 16, CornerRadiusTopRight = 16 };
		panel.AddThemeStyleboxOverride("panel", style);
		panel.SizeFlagsVertical = SizeFlags.ExpandFill;

		var margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_top", 20);
		margin.AddThemeConstantOverride("margin_left", 30);
		margin.AddThemeConstantOverride("margin_right", 30);
		margin.AddThemeConstantOverride("margin_bottom", 20);

		_analysisLabel = new RichTextLabel
		{
			BbcodeEnabled = true,
			FitContent = false,
			SizeFlagsVertical = SizeFlags.ExpandFill,
			CustomMinimumSize = new Vector2(900, 200)
		};

		margin.AddChild(_analysisLabel);
		panel.AddChild(margin);
		AddChild(panel);
	}

	private void RenderPillars(int yIdx, int mIdx, int dIdx, int hIdx)
	{
		foreach (Node child in _pillarsContainer.GetChildren()) child.QueueFree();

		var yearStem = GanZhiMath.GetStem(yIdx);
		var yearBranch = GanZhiMath.GetBranch(yIdx);

		var monthStem = GanZhiMath.GetStem(mIdx);
		var monthBranch = GanZhiMath.GetBranch(mIdx);

		var dayStem = GanZhiMath.GetStem(dIdx);
		var dayBranch = GanZhiMath.GetBranch(dIdx);

		var hourStem = GanZhiMath.GetStem(hIdx);
		var hourBranch = GanZhiMath.GetBranch(hIdx);

		AddPillar("TXT_PILLAR_YEAR", yearStem, yearBranch, dayStem);
		AddPillar("TXT_PILLAR_MONTH", monthStem, monthBranch, dayStem);
		AddPillar("TXT_PILLAR_DAY", dayStem, dayBranch, dayStem);
		AddPillar("TXT_PILLAR_HOUR", hourStem, hourBranch, dayStem);
	}

	private void AddPillar(string title, HeavenlyStem stem, EarthlyBranch branch, HeavenlyStem dayMaster)
	{
		var pillar = new PillarWidget(title, stem, branch, dayMaster);
		_pillarsContainer.AddChild(pillar);
	}

	private void RenderAnalysis(BaziResult result)
	{
		var sbScores = new StringBuilder();
		foreach (var kvp in result.Scores)
		{
			string c = GlobalUIController.GetElementColor(kvp.Key).ToHtml();
			
			sbScores.Append($"[color=#{c}]{kvp.Key.GetLocalizedName()}: {kvp.Value:F1}[/color]  ");
		}

		string strengthColor = result.IsDayMasterStrong ? "#FFD700" : "#03DAC6"; 
		string strengthText = result.IsDayMasterStrong ? Tr("TXT_PILLAR_STRONG") : Tr("TXT_PILLAR_WEAK");

		var sbGods = new StringBuilder();
		foreach (var god in result.UsefulGods)
		{
			string c = GlobalUIController.GetElementColor(god).ToHtml();
			sbGods.Append($"[color=#{c}][b]{god.GetLocalizedName()}[/b][/color] ");
		}

		_analysisLabel.Text = Helpers.GetLocalizedFormat("TXT_BAZI_RESULT", strengthColor, strengthText,
			sbScores, sbGods, result.Comment);
	}

	// --- UI Logic ---
	private partial class PillarWidget : PanelContainer
	{
		public PillarWidget(string title, HeavenlyStem stem, EarthlyBranch branch, HeavenlyStem dayMaster)
		{
			CustomMinimumSize = new Vector2(220, 450);
			var style = new StyleBoxFlat
			{
				BgColor = GlobalUIController.ColorSurface.Lightened(0.05f),
				CornerRadiusTopLeft = 12,
				CornerRadiusTopRight = 12,
				CornerRadiusBottomLeft = 12,
				CornerRadiusBottomRight = 12,
				BorderWidthBottom = 6,
				BorderColor = GlobalUIController.GetElementColor(branch.GetWuXing())
			};
			AddThemeStyleboxOverride("panel", style);

			var vBox = new VBoxContainer();
			vBox.AddThemeConstantOverride("separation", 8);
			AddChild(vBox);

			// A. Title
			var lblTitle = new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Center };
			lblTitle.AddThemeColorOverride("font_color", Colors.Gray);
			vBox.AddChild(lblTitle);

			// B. Ten Gods
			string tenGodStr = "";
			Color tenGodColor = Colors.White;

			if (title.Contains("DAY"))
			{
				tenGodStr = "TXT_PILLAR_DAYMASTER"; 
				tenGodColor = GlobalUIController.ColorAccent;
			}
			else
			{
				var tenGod = BaziHelpers.CalculateTenGod(dayMaster, stem);
				tenGodStr = tenGod.GetLocalizedName();
				tenGodColor = GlobalUIController.ColorTextPrimary;
			}

			var lblTenGod = new Label
			{
				Text = tenGodStr,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			lblTenGod.AddThemeFontSizeOverride("font_size", 20);
			lblTenGod.AddThemeColorOverride("font_color", tenGodColor);
			vBox.AddChild(lblTenGod);

			// C. Stems
			vBox.AddChild(CreateBigChar(stem.GetLocalizedName(), stem.GetWuXing()));

			// D. Branches
			vBox.AddChild(CreateBigChar(branch.GetLocalizedName(), branch.GetWuXing()));

			// E. Hidden
			var hiddenContainer = new VBoxContainer();
			hiddenContainer.Alignment = BoxContainer.AlignmentMode.Center;

			var hiddenStems = branch.GetHiddenStems();
			foreach (var hidden in hiddenStems)
			{
				var hTenGod = BaziHelpers.CalculateTenGod(dayMaster, hidden.Stem);

				// Format: [Stem] [TenGod] (Power%)
				// e.g. Jia (7K) 60%
				string hText = $"{hidden.Stem.GetLocalizedName()} <{hTenGod.GetLocalizedName()}>";

				var lbl = new Label
				{
					Text = hText,
					HorizontalAlignment = HorizontalAlignment.Center
				};
				lbl.AddThemeFontSizeOverride("font_size", 16);

				Color baseColor = hidden.IsMainQi ? Colors.LightGray : Colors.Gray;
				lbl.AddThemeColorOverride("font_color", baseColor);

				hiddenContainer.AddChild(lbl);
			}
			vBox.AddChild(hiddenContainer);
		}

		private Label CreateBigChar(string text, WuXingType wuxing)
		{
			var lbl = new Label
			{
				Text = text,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			lbl.AddThemeFontSizeOverride("font_size", 56);
			lbl.AddThemeColorOverride("font_color", GlobalUIController.GetElementColor(wuxing));
			return lbl;
		}
	}
}
