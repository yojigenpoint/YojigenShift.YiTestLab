using Godot;
using System;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiFramework.Structs;
using YojigenShift.YiTestLab.Modules.Components;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class HexagramModule : VBoxContainer
	{
		private OptionButton _optUpper;
		private OptionButton _optLower;

		// UI
		private HexagramVisualizer _visOriginal;    // 本卦
		private HexagramVisualizer _visMutual;      // 互卦
		private HexagramVisualizer _visInverted;    // 错卦
		private HexagramVisualizer _visReversed;    // 综卦

		private RichTextLabel _lblOriginalInfo;
		private RichTextLabel _lblMutualInfo;
		private RichTextLabel _lblInvertedInfo;
		private RichTextLabel _lblReversedInfo;

		public override void _Ready()
		{
			Name = "HexagramModule";
			AddThemeConstantOverride("separation", 30);

			SetupUI();

			UpdateHexagramDisplay();
		}

		private void SetupUI()
		{
			// 1. Title
			var topBox = new VBoxContainer();
			topBox.Alignment = BoxContainer.AlignmentMode.Center;
			topBox.AddThemeConstantOverride("separation", 10);
			AddChild(topBox);

			var title = new Label { Text = Tr("MOD_HEXAGRAM_TITLE"), HorizontalAlignment = HorizontalAlignment.Center };
			title.AddThemeFontSizeOverride("font_size", 40);
			topBox.AddChild(title);

			var subTitle = new Label { Text = Tr("TXT_HEXAGRAM_SUBTITLE"), HorizontalAlignment = HorizontalAlignment.Center };
			subTitle.AddThemeColorOverride("font_color", Colors.Gray);
			topBox.AddChild(subTitle);

			// 2. Selector (Upper / Lower Trigram)
			var selectorBox = new HBoxContainer();
			selectorBox.Alignment = BoxContainer.AlignmentMode.Center;
			selectorBox.AddThemeConstantOverride("separation", 30);
			AddChild(selectorBox);

			selectorBox.AddChild(CreateTrigramSelector(Tr("TXT_HEXAGRAM_UPPER"), out _optUpper));
			var connector = new Label { Text = "➕", VerticalAlignment = VerticalAlignment.Center, SizeFlagsVertical = SizeFlags.ShrinkCenter };
			connector.AddThemeFontSizeOverride("font_size", 30);
			selectorBox.AddChild(connector);
			selectorBox.AddChild(CreateTrigramSelector(Tr("TXT_HEXAGRAM_LOWER"), out _optLower));

			// 3. Core display area (2x2 grid)
			var gridDisplay = new GridContainer();
			gridDisplay.Columns = 2;
			gridDisplay.AddThemeConstantOverride("h_separation", 60);
			gridDisplay.AddThemeConstantOverride("v_separation", 40);
			gridDisplay.SizeFlagsVertical = SizeFlags.ExpandFill;
			AddChild(gridDisplay);

			// 本卦
			var colOriginal = CreateHexColumn(Tr("TXT_HEXAGRAM_ORIGINAL"), out _visOriginal, out _lblOriginalInfo);
			gridDisplay.AddChild(colOriginal);

			// 互卦
			var colMutual = CreateHexColumn(Tr("TXT_HEXAGRAM_MUTUAL"), out _visMutual, out _lblMutualInfo);
			gridDisplay.AddChild(colMutual);

			// 错卦
			var colInverted = CreateHexColumn(Tr("TXT_HEXAGRAM_INVERTED"), out _visInverted, out _lblInvertedInfo);
			gridDisplay.AddChild(colInverted);

			// 综卦
			var colReversed = CreateHexColumn(Tr("TXT_HEXAGRAM_REVERSED"), out _visReversed, out _lblReversedInfo);
			gridDisplay.AddChild(colReversed);
		}

		private VBoxContainer CreateTrigramSelector(string title, out OptionButton opt)
		{
			var vBox = new VBoxContainer();
			vBox.Alignment = BoxContainer.AlignmentMode.Center;
			vBox.AddThemeConstantOverride("separation", 15);

			var lblTitle = new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Center };
			lblTitle.AddThemeColorOverride("font_color", GlobalUIController.ColorTextSecondary);
			lblTitle.AddThemeFontSizeOverride("font_size", 25);
			vBox.AddChild(lblTitle);

			opt = new OptionButton();
			opt.CustomMinimumSize = new Vector2(250, 60);
			opt.AddThemeFontSizeOverride("font_size", 30);
			opt.GetPopup().AddThemeFontSizeOverride("font_size", 30);

			foreach (TrigramName name in Enum.GetValues(typeof(TrigramName)))
			{
				opt.AddItem(name.GetLocalizedName(), (int)name);
			}

			opt.Selected = 7;
			opt.ItemSelected += (idx) => UpdateHexagramDisplay();

			vBox.AddChild(opt);
			return vBox;
		}

		private VBoxContainer CreateHexColumn(string title, out HexagramVisualizer vis, out RichTextLabel info)
		{
			var vBox = new VBoxContainer();
			vBox.Alignment = BoxContainer.AlignmentMode.Center;
			vBox.AddThemeConstantOverride("separation", 15);

			var lblTitle = new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Center };
			lblTitle.AddThemeColorOverride("font_color", GlobalUIController.ColorTextSecondary);
			lblTitle.AddThemeFontSizeOverride("font_size", 25);
			vBox.AddChild(lblTitle);

			var panel = new PanelContainer();
			var style = new StyleBoxFlat { BgColor = GlobalUIController.ColorSurface.Darkened(0.1f), CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8 };
			panel.AddThemeStyleboxOverride("panel", style);
			vBox.AddChild(panel);

			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_left", 20); margin.AddThemeConstantOverride("margin_right", 20);
			margin.AddThemeConstantOverride("margin_top", 20); margin.AddThemeConstantOverride("margin_bottom", 20);
			panel.AddChild(margin);

			vis = new HexagramVisualizer();
			margin.AddChild(vis);

			info = new RichTextLabel { 
				BbcodeEnabled = true,
				FitContent = true,
				ScrollActive = false,
				CustomMinimumSize = new Vector2(150, 0)
			};
			info.AddThemeFontSizeOverride("normal_font_size", 30);
			info.BbcodeEnabled = true;
			vBox.AddChild(info);

			return vBox;
		}

		// --- Core Update Logic ---

		private void UpdateHexagramDisplay()
		{
			// 1. Chosen trigrams
			TrigramName upper = (TrigramName)_optUpper.GetSelectedId();
			TrigramName lower = (TrigramName)_optLower.GetSelectedId();

			// 2. Create original hexagram
			Hexagram originalHex = new Hexagram(upper, lower);

			// 3. Calculate derived hexagrams
			Hexagram mutualHex = originalHex.GetMutual();
			Hexagram invertedHex = originalHex.GetInverted();
			Hexagram reversedHex = originalHex.GetReversed();

			// 4. Update visualizers and info labels
			_visOriginal.SetHexagram(originalHex);
			UpdateHexInfo(_lblOriginalInfo, originalHex);

			_visMutual.SetHexagram(mutualHex);
			UpdateHexInfo(_lblMutualInfo, mutualHex);

			_visInverted.SetHexagram(invertedHex);
			UpdateHexInfo(_lblInvertedInfo, invertedHex);

			_visReversed.SetHexagram(reversedHex);
			UpdateHexInfo(_lblReversedInfo, reversedHex);
		}

		private void UpdateHexInfo(RichTextLabel lbl, Hexagram hex)
		{
			string upperName = hex.Upper.GetLocalizedName();
			string lowerName = hex.Lower.GetLocalizedName();

			string hexName = "Unknown";
			if (Enum.IsDefined(typeof(HexagramName), hex.Value))
				hexName = ((HexagramName)hex.Value).GetLocalizedName();

			lbl.Text = $"[center][font_size=28][color={GlobalUIController.ColorAccent.ToHtml()}][b]{hexName}[/b][/color][/font_size]\n" +
					   $"[b]{upperName} / {lowerName}[/b]\n" + 
					   $"[color=gray]Index: {hex.Value}[/color][/center]";
		}
	}
}
