using Godot;
using Godot.Collections;
using System;
using System.Text;
using YojigenShift.YiFramework.Core;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiTestLab.Core;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class BaziModule : VBoxContainer
	{
		private GlobalUIController _globalUIController;
		private DateTime _birthTime = DateTime.UtcNow;
		private bool _isMale = true;

		private HBoxContainer _pillarsContainer;
		private HBoxContainer _dayunContainer;
		private GridContainer _liunianGrid;
		private RichTextLabel _analysisLabel;

		private (HeavenlyStem YS, EarthlyBranch YB, HeavenlyStem MS, EarthlyBranch MB) _cacheChart;
		private Dictionary<int, int> _dayunStartYears = new Dictionary<int, int>();
		private int _selectedDayunIndex = -1;

		public override void _Ready()
		{
			Name = "BaziModule";
			AddThemeConstantOverride("separation", 20);

			_globalUIController = GetNode<Control>("/root/MainUI") as GlobalUIController;
			if (_globalUIController == null)
				_globalUIController = this.FindParent("MainUI") as GlobalUIController;

			SetupUI();

			if (_globalUIController != null)
			{
				_birthTime = _globalUIController.CurrentTime;
				_isMale = _globalUIController.IsMale;

				_globalUIController.GlobalTimeChanged += OnTimeChanged;
				_globalUIController.GenderChanged += OnGenderChanged;
			}

			RefreshAll();
		}

		public override void _ExitTree()
		{
			if (_globalUIController != null)
			{
				_globalUIController.GlobalTimeChanged -= OnTimeChanged;
				_globalUIController.GenderChanged -= OnGenderChanged;
			}
		}

		private void SetupUI()
		{
			SetupHeader();
			SetupPillarsArea();
			SetupDaYunArea();
			SetupLiuNianArea();
			SetupAnalysisArea();
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

		private void SetupDaYunArea()
		{
			var panel = new PanelContainer();
			var style = new StyleBoxFlat { BgColor = GlobalUIController.ColorSurface.Darkened(0.2f), ContentMarginTop = 10, ContentMarginBottom = 10 };
			panel.AddThemeStyleboxOverride("panel", style);

			var vBox = new VBoxContainer();
			panel.AddChild(vBox);

			var lblTitle = new Label { Text = "TXT_DAYUN_TITLE", HorizontalAlignment = HorizontalAlignment.Center };
			lblTitle.AddThemeColorOverride("font_color", GlobalUIController.ColorTextSecondary);
			vBox.AddChild(lblTitle);

			var scroll = new ScrollContainer { VerticalScrollMode = ScrollContainer.ScrollMode.Disabled, CustomMinimumSize = new Vector2(0, 160) };
			vBox.AddChild(scroll);

			_dayunContainer = new HBoxContainer();
			_dayunContainer.Alignment = BoxContainer.AlignmentMode.Center;
			_dayunContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			_dayunContainer.AddThemeConstantOverride("separation", 10);
			scroll.AddChild(_dayunContainer);

			AddChild(panel);
		}

		private void SetupLiuNianArea()
		{
			var center = new CenterContainer();
			_liunianGrid = new GridContainer { Columns = 5 };
			_liunianGrid.AddThemeConstantOverride("h_separation", 15);
			_liunianGrid.AddThemeConstantOverride("v_separation", 15);
			_liunianGrid.Visible = false;

			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_top", 10);
			margin.AddThemeConstantOverride("margin_bottom", 20);
			margin.AddChild(_liunianGrid);

			center.AddChild(margin);
			AddChild(center);
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

		private void OnTimeChanged(DateTime time)
		{
			_birthTime = time;
			_selectedDayunIndex = -1;
			_liunianGrid.Visible = false;
			RefreshAll();
		}

		private void OnGenderChanged(bool isMale)
		{
			_isMale = isMale;
			_selectedDayunIndex = -1;
			_liunianGrid.Visible = false;
			RenderDaYun();
		}

		private void RefreshAll()
		{
			try
			{
				var (yIdx, mIdx, dIdx, hIdx) = BaziPlotter.Plot(_birthTime);

				var ys = GanZhiMath.GetStem(yIdx);
				var ms = GanZhiMath.GetStem(mIdx);
				var mb = GanZhiMath.GetBranch(mIdx);
				_cacheChart = (ys, GanZhiMath.GetBranch(yIdx), ms, mb);

				RenderPillars(yIdx, mIdx, dIdx, hIdx);
				RenderDaYun();

				var result = BaziEvaluator.Evaluate(yIdx, mIdx, dIdx, hIdx);
				RenderAnalysis(result);
			}
			catch (Exception ex)
			{
				GD.PrintErr(ex);
			}
		}

		private void RenderDaYun()
		{
			foreach (Node child in _dayunContainer.GetChildren()) child.QueueFree();
			_dayunStartYears.Clear();

			var dyList = BaziHelpers.GetDayunSequence(_cacheChart.YS, _cacheChart.MS, _cacheChart.MB, _isMale);

			int startAge = 3;
			int birthYear = _birthTime.Year;

			for (int i = 0; i < dyList.Count; i++)
			{
				var (stem, branch) = dyList[i];
				int currentAge = startAge + i * 10;
				int currentYear = birthYear + currentAge;

				_dayunStartYears[i] = currentYear;

				// 创建可点击的大运柱
				var pillBtn = CreateClickableMiniPillar(stem, branch, currentAge, i);
				_dayunContainer.AddChild(pillBtn);
			}
		}

		private void RenderLiuNian(int daYunStartYear)
		{
			foreach (Node child in _liunianGrid.GetChildren()) child.QueueFree();
			_liunianGrid.Visible = true;

			for (int i = 0; i < 10; i++)
			{
				int year = daYunStartYear + i;

				int age = (year - _birthTime.Year);

				int ganZhiIdx = GanZhiMath.Mod60(year - 1984);

				var stem = GanZhiMath.GetStem(ganZhiIdx);
				var branch = GanZhiMath.GetBranch(ganZhiIdx);

				var pill = CreateLiuNianPillar(stem, branch, year, age);
				_liunianGrid.AddChild(pill);
			}
		}

		private void OnDaYunClicked(int index)
		{
			_selectedDayunIndex = index;

			foreach (var node in _dayunContainer.GetChildren())
			{
				if (node is Button btn)
				{
					bool isSelected = (int)btn.GetMeta("idx") == index;
					btn.Flat = !isSelected;
				}
			}

			if (_dayunStartYears.ContainsKey(index))
			{
				RenderLiuNian(_dayunStartYears[index]);
			}
		}

		private Button CreateClickableMiniPillar(HeavenlyStem s, EarthlyBranch b, int age, int index)
		{
			var btn = new Button
			{
				CustomMinimumSize = new Vector2(100, 140),
				ToggleMode = true,
				Flat = (_selectedDayunIndex != index)
			};
			btn.SetMeta("idx", index);

			var vBox = new VBoxContainer();
			vBox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
			vBox.MouseFilter = Control.MouseFilterEnum.Ignore;
			btn.AddChild(vBox);

			var lblS = new Label { Text = s.GetLocalizedName(), HorizontalAlignment = HorizontalAlignment.Center };
			lblS.AddThemeColorOverride("font_color", GlobalUIController.GetElementColor(s.GetWuXing()));
			lblS.AddThemeFontSizeOverride("font_size", 24);
			vBox.AddChild(lblS);

			var lblB = new Label { Text = b.GetLocalizedName(), HorizontalAlignment = HorizontalAlignment.Center };
			lblB.AddThemeColorOverride("font_color", GlobalUIController.GetElementColor(b.GetWuXing()));
			lblB.AddThemeFontSizeOverride("font_size", 24);
			vBox.AddChild(lblB);

			var lblAge = new Label { Text = $"{age}" + Tr("TXT_AGE"), HorizontalAlignment = HorizontalAlignment.Center };
			lblAge.AddThemeFontSizeOverride("font_size", 20);
			lblAge.AddThemeColorOverride("font_color", Colors.Gray);
			vBox.AddChild(lblAge);

			btn.Pressed += () => OnDaYunClicked(index);

			return btn;
		}

		private PanelContainer CreateLiuNianPillar(HeavenlyStem s, EarthlyBranch b, int year, int age)
		{
			var p = new PanelContainer();
			var style = new StyleBoxFlat
			{
				BgColor = GlobalUIController.ColorSurface.Lightened(0.05f),
				CornerRadiusTopLeft = 8,
				CornerRadiusTopRight = 8,
				CornerRadiusBottomLeft = 8,
				CornerRadiusBottomRight = 8,
				BorderWidthBottom = 2,
				BorderColor = GlobalUIController.GetElementColor(b.GetWuXing())
			};
			p.AddThemeStyleboxOverride("panel", style);
			p.CustomMinimumSize = new Vector2(160, 100);

			var vBox = new VBoxContainer();
			p.AddChild(vBox);

			var lblYear = new Label { Text = $"{year}", HorizontalAlignment = HorizontalAlignment.Center };
			lblYear.AddThemeColorOverride("font_color", Colors.LightGray);
			lblYear.AddThemeFontSizeOverride("font_size", 20);
			vBox.AddChild(lblYear);

			var hBox = new HBoxContainer();
			hBox.Alignment = BoxContainer.AlignmentMode.Center;
			vBox.AddChild(hBox);

			var lblS = new Label { Text = s.GetLocalizedName() };
			lblS.AddThemeColorOverride("font_color", GlobalUIController.GetElementColor(s.GetWuXing()));
			lblS.AddThemeFontSizeOverride("font_size", 24);
			hBox.AddChild(lblS);

			var lblB = new Label { Text = b.GetLocalizedName() };
			lblB.AddThemeColorOverride("font_color", GlobalUIController.GetElementColor(b.GetWuXing()));
			lblB.AddThemeFontSizeOverride("font_size", 24);
			hBox.AddChild(lblB);

			return p;
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

			AddPillar(_pillarsContainer, "TXT_PILLAR_YEAR", yearStem, yearBranch, dayStem);
			AddPillar(_pillarsContainer, "TXT_PILLAR_MONTH", monthStem, monthBranch, dayStem);
			AddPillar(_pillarsContainer, "TXT_PILLAR_DAY", dayStem, dayBranch, dayStem);
			AddPillar(_pillarsContainer, "TXT_PILLAR_HOUR", hourStem, hourBranch, dayStem);
		}

		private void AddPillar(Container parent, string title, HeavenlyStem stem, EarthlyBranch branch, HeavenlyStem dayMaster)
		{
			var pillar = new PillarWidget(title, stem, branch, dayMaster);
			parent.AddChild(pillar);
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
					lbl.AddThemeFontSizeOverride("font_size", 20);

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
}
