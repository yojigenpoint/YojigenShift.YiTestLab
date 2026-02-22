using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using YojigenShift.YiFramework.Core;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiTestLab.Core;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class GanZhiModule : VBoxContainer
	{
		private HeavenlyStem _selectedStem = HeavenlyStem.Jia;
		private EarthlyBranch _selectedBranch = EarthlyBranch.Zi;

		private Dictionary<HeavenlyStem, Button> _stemButtons = new Dictionary<HeavenlyStem, Button>();
		private Dictionary<EarthlyBranch, Button> _branchButtons = new Dictionary<EarthlyBranch, Button>();
		private RichTextLabel _infoLabel;

		private Button _stemTabBtn;
		private Button _branchTabBtn;
		private Control _stemContent;
		private Control _branchContent;

		private StyleBoxFlat _styleNormal;
		private StyleBoxFlat _styleSelected;
		private StyleBoxFlat _styleCombine; // Combine (Gold)
		private StyleBoxFlat _styleClash;   // Clash (Red)
		private StyleBoxFlat _styleSanHe;   // San He (Green)
		private StyleBoxFlat _stylePunish;  // Punish (Orange)

		private StyleBoxFlat _styleTabActive;
		private StyleBoxFlat _styleTabInactive;

		public override void _Ready()
		{
			Name = "GanZhiModule";
			AddThemeConstantOverride("separation", 20);

			InitStyles();

			var title = new Label
			{
				Text = Tr("MOD_GANZHI_TITLE"),
				HorizontalAlignment = HorizontalAlignment.Center
			};
			title.AddThemeFontSizeOverride("font_size", 48);
			AddChild(title);

			var tabBar = new HBoxContainer();
			tabBar.AddThemeConstantOverride("separation", 0);
			AddChild(tabBar);

			_stemTabBtn = CreateTabButton(Tr("MOD_GANZHI_STEM_TITLE"), true);
			_branchTabBtn = CreateTabButton(Tr("MOD_GANZHI_BRANCH_TITLE"), false);

			tabBar.AddChild(_stemTabBtn);
			tabBar.AddChild(_branchTabBtn);

			var contentContainer = new MarginContainer();
			contentContainer.AddThemeConstantOverride("margin_top", 10);
			contentContainer.AddThemeConstantOverride("margin_bottom", 10);
			AddChild(contentContainer);

			_stemContent = CreateStemPage();
			_branchContent = CreateBranchPage();

			contentContainer.AddChild(_stemContent);
			contentContainer.AddChild( _branchContent);

			var panel = new PanelContainer();
			panel.SizeFlagsVertical = SizeFlags.ExpandFill;
			var panelStyle = new StyleBoxFlat { BgColor = GlobalUIController.ColorSurface, CornerRadiusTopLeft = 16, CornerRadiusTopRight = 16 };
			panel.AddThemeStyleboxOverride("panel", panelStyle);

			_infoLabel = new RichTextLabel
			{
				BbcodeEnabled = true,
				FitContent = false,
				SizeFlagsVertical = SizeFlags.ExpandFill,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
			};

			var textMargin = new MarginContainer();
			textMargin.AddThemeConstantOverride("margin_left", 30);
			textMargin.AddThemeConstantOverride("margin_right", 30);
			textMargin.AddThemeConstantOverride("margin_top", 20);
			textMargin.AddThemeConstantOverride("margin_bottom", 20);
			textMargin.AddChild(_infoLabel);
			panel.AddChild(textMargin);
			AddChild(panel);

			SwitchTab(true);
			OnStemClicked(HeavenlyStem.Jia);
		}

		private void InitStyles()
		{
			_styleNormal = new StyleBoxFlat { CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8, BorderWidthBottom = 4, BorderColor = new Color(0, 0, 0, 0.3f) };

			_styleSelected = (StyleBoxFlat)_styleNormal.Duplicate();
			_styleSelected.BorderColor = Colors.White;
			_styleSelected.BorderWidthTop = 4; _styleSelected.BorderWidthLeft = 4; _styleSelected.BorderWidthRight = 4; _styleSelected.BorderWidthBottom = 4;

			_styleCombine = (StyleBoxFlat)_styleNormal.Duplicate();
			_styleCombine.BorderColor = new Color("#FFD700"); // Gold
			_styleCombine.BorderWidthTop = 4; _styleCombine.BorderWidthLeft = 4; _styleCombine.BorderWidthRight = 4; _styleCombine.BorderWidthBottom = 4;

			_styleClash = (StyleBoxFlat)_styleNormal.Duplicate();
			_styleClash.BorderColor = new Color("#FF5252"); // Red
			_styleClash.BorderWidthTop = 4; _styleClash.BorderWidthLeft = 4; _styleClash.BorderWidthRight = 4; _styleClash.BorderWidthBottom = 4;

			_styleSanHe = (StyleBoxFlat)_styleNormal.Duplicate();
			_styleSanHe.BorderColor = new Color("#03DAC6"); // Green
			_styleSanHe.BorderWidthTop = 4; _styleSanHe.BorderWidthLeft = 4; _styleSanHe.BorderWidthRight = 4; _styleSanHe.BorderWidthBottom = 4;

			_stylePunish = (StyleBoxFlat)_styleNormal.Duplicate();
			_stylePunish.BorderColor = new Color("#FF9800"); // Orange
			_stylePunish.BorderWidthTop = 4; _stylePunish.BorderWidthLeft = 4; _stylePunish.BorderWidthRight = 4; _stylePunish.BorderWidthBottom = 4;

			_styleTabActive = new StyleBoxFlat { BgColor = GlobalUIController.ColorAccent, ContentMarginBottom = 10, ContentMarginTop = 10 };
			_styleTabInactive = new StyleBoxFlat { BgColor = GlobalUIController.ColorSurface.Darkened(0.2f), ContentMarginBottom = 10, ContentMarginTop = 10 };
		}

		#region Tab & Layout

		private Button CreateTabButton(string text, bool isStem)
		{
			var btn = new Button
			{
				Text = text,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				FocusMode = FocusModeEnum.None
			};
			btn.AddThemeColorOverride("font_color", Colors.White);
			btn.AddThemeColorOverride("font_pressed_color", Colors.Black);

			btn.Pressed += () => SwitchTab(isStem);
			return btn;
		}

		private void SwitchTab(bool showStem)
		{
			_stemContent.Visible = showStem;
			_branchContent.Visible = !showStem;

			_stemTabBtn.AddThemeStyleboxOverride("normal", showStem ? _styleTabActive : _styleTabInactive);
			_stemTabBtn.AddThemeStyleboxOverride("hover", showStem ? _styleTabActive : _styleTabInactive);
			_stemTabBtn.AddThemeColorOverride("font_color", showStem ? Colors.Black : Colors.Gray);

			_branchTabBtn.AddThemeStyleboxOverride("normal", !showStem ? _styleTabActive : _styleTabInactive);
			_branchTabBtn.AddThemeStyleboxOverride("hover", !showStem ? _styleTabActive : _styleTabInactive);
			_branchTabBtn.AddThemeColorOverride("font_color", !showStem ? Colors.Black : Colors.Gray);

			if (showStem) OnStemClicked(_selectedStem);
			else OnBranchClicked(_selectedBranch);
		}

		#endregion

		#region Stem Logic

		private Control CreateStemPage()
		{
			var center = new CenterContainer();
			var grid = new GridContainer { Columns = 5 };
			grid.AddThemeConstantOverride("h_separation", 20);
			grid.AddThemeConstantOverride("v_separation", 20);

			foreach (HeavenlyStem stem in Enum.GetValues(typeof(HeavenlyStem)))
			{
				var btn = new Button
				{
					Text = stem.GetLocalizedName(),
					CustomMinimumSize = new Vector2(160, 100)
				};

				var color = GlobalUIController.GetElementColor(stem.GetWuXing());
				var btnStyle = (StyleBoxFlat)_styleNormal.Duplicate();
				btnStyle.BgColor = color;

				btn.AddThemeStyleboxOverride("normal", btnStyle);
				btn.AddThemeStyleboxOverride("hover", btnStyle);
				btn.AddThemeStyleboxOverride("pressed", btnStyle);
				btn.AddThemeColorOverride("font_color", Colors.Black);
				btn.AddThemeFontSizeOverride("font_size", 40);

				btn.Pressed += () => OnStemClicked(stem);
				grid.AddChild(btn);
				_stemButtons[stem] = btn;
			}
			center.AddChild(grid);
			return center;
		}

		private void OnStemClicked(HeavenlyStem selected)
		{
			_selectedStem = selected;

			foreach (var kvp in _stemButtons)
			{
				var target = kvp.Key;
				var btn = kvp.Value;
				var baseStyle = (StyleBoxFlat)btn.GetThemeStylebox("normal");

				StyleBoxFlat finalStyle;

				if (target == selected) finalStyle = (StyleBoxFlat)_styleSelected.Duplicate();
				else if (selected.IsCombine(target)) finalStyle = (StyleBoxFlat)_styleCombine.Duplicate();
				else if (selected.IsClash(target)) finalStyle = (StyleBoxFlat)_styleClash.Duplicate();
				else finalStyle = (StyleBoxFlat)_styleNormal.Duplicate();

				finalStyle.BgColor = baseStyle.BgColor;
				btn.AddThemeStyleboxOverride("normal", finalStyle);
				btn.AddThemeStyleboxOverride("hover", finalStyle);
				btn.AddThemeStyleboxOverride("pressed", finalStyle);
			}
			UpdateStemInfo(selected);
		}

		private void UpdateStemInfo(HeavenlyStem stem)
		{
			var wuxing = stem.GetWuXing();
			string combineInfo = "";
			string clashInfo = "";

			foreach (HeavenlyStem other in Enum.GetValues(typeof(HeavenlyStem)))
			{
				if (stem.IsCombine(other))
					combineInfo += Helpers.GetLocalizedFormat("TXT_COMBINE_INFO", "FFD700",
						other.GetLocalizedName(), stem.GetCombinationWuXing().GetLocalizedName());
				if (stem.IsClash(other))
					clashInfo += Helpers.GetLocalizedFormat("TXT_CLASH_INFO", "FF5252", other.GetLocalizedName());
			}

			string c = GlobalUIController.GetElementColor(wuxing).ToHtml();
			_infoLabel.Text = Helpers.GetLocalizedFormat("TXT_STEM_RESULT",
				c, stem.GetLocalizedName(), wuxing.GetLocalizedName(), stem.GetPolarity().GetLocalizedName(),
				combineInfo, clashInfo);
		}

		#endregion

		#region Branch Logic

		private Control CreateBranchPage()
		{
			var center = new CenterContainer();
			var grid = new GridContainer { Columns = 4 };
			grid.AddThemeConstantOverride("h_separation", 20);
			grid.AddThemeConstantOverride("v_separation", 20);

			foreach (EarthlyBranch branch in Enum.GetValues(typeof(EarthlyBranch)))
			{
				var btn = new Button
				{
					Text = branch.GetLocalizedName(),
					CustomMinimumSize = new Vector2(210, 100)
				};

				var color = GlobalUIController.GetElementColor(branch.GetWuXing());
				var btnStyle = (StyleBoxFlat)_styleNormal.Duplicate();
				btnStyle.BgColor = color;

				btn.AddThemeStyleboxOverride("normal", btnStyle);
				btn.AddThemeStyleboxOverride("hover", btnStyle);
				btn.AddThemeColorOverride("font_color", Colors.Black);
				btn.AddThemeFontSizeOverride("font_size", 40);

				btn.Pressed += () => OnBranchClicked(branch);
				grid.AddChild(btn);
				_branchButtons[branch] = btn;
			}
			center.AddChild(grid);
			return center;
		}

		private void OnBranchClicked(EarthlyBranch selected)
		{
			_selectedBranch = selected;

			foreach (var kvp in _branchButtons)
			{
				var target = kvp.Key;
				var btn = kvp.Value;
				var baseStyle = (StyleBoxFlat)btn.GetThemeStylebox("normal");

				StyleBoxFlat finalStyle = null;

				if (target == selected)
					finalStyle = (StyleBoxFlat)_styleSelected.Duplicate();
				else if (selected.IsClash(target))
					finalStyle = (StyleBoxFlat)_styleClash.Duplicate(); // Red
				else if (selected.IsCombine(target))
					finalStyle = (StyleBoxFlat)_styleCombine.Duplicate(); // Gold
				else if (selected.IsSanHe(target))
					finalStyle = (StyleBoxFlat)_styleSanHe.Duplicate(); // Green
				else if (selected.IsPunishment(target))
					finalStyle = (StyleBoxFlat)_stylePunish.Duplicate(); // Orange
				else
					finalStyle = (StyleBoxFlat)_styleNormal.Duplicate();

				finalStyle.BgColor = baseStyle.BgColor;

				btn.AddThemeStyleboxOverride("normal", finalStyle);
				btn.AddThemeStyleboxOverride("hover", finalStyle);
				btn.AddThemeStyleboxOverride("pressed", finalStyle);
			}

			UpdateBranchInfo(selected);
		}

		private void UpdateBranchInfo(EarthlyBranch b)
		{
			StringBuilder sbRelations = new StringBuilder();

			foreach (EarthlyBranch other in Enum.GetValues(typeof(EarthlyBranch)))
			{
				if (b == other && !b.IsPunishment(other)) continue;

				List<string> rels = new List<string>();

				if (b.IsCombine(other)) rels.Add($"[color=#FFD700]{Tr("TXT_LIUHE")}[/color]");
				if (b.IsSanHe(other)) rels.Add($"[color=#03DAC6]{Tr("TXT_SANHE")}[/color]");
				if (b.IsClash(other)) rels.Add($"[color=#FF5252]{Tr("TXT_CLASH")}[/color]");
				if (b.IsHarm(other)) rels.Add($"[color=#9E9E9E]{Tr("TXT_HARM")}[/color]");
				if (b.IsDestruction(other)) rels.Add($"[color=#757575]{Tr("TXT_DESTRUCTION")}[/color]");
				if (b.IsPunishment(other)) rels.Add($"[color=#FF9800]{Tr("TXT_PUNISHEMENT")}[/color]");

				if (rels.Count > 0)
				{
					sbRelations.Append($"{other.GetLocalizedName()}: {string.Join(", ", rels)}\n");
				}
			}

			string c = GlobalUIController.GetElementColor(b.GetWuXing()).ToHtml();

			_infoLabel.Text = Helpers.GetLocalizedFormat("TXT_BRANCH_RESULT",
				c, b.GetLocalizedName(), b.GetZodiac(), b.GetWuXing().GetLocalizedName(), b.GetSeason().GetLocalizedName(), sbRelations);
		}

		#endregion
	}
}
