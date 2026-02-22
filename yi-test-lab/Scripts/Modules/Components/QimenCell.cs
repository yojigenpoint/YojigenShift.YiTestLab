using Godot;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiFramework.QiMen.Logic;
using YojigenShift.YiFramework.QiMen.Models;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules.Components
{
	public partial class QimenCell : PanelContainer
	{
		private Label _lblGod;
		private RichTextLabel _lblHeavenStem;
		private RichTextLabel _lblStar;
		private RichTextLabel _lblEarthStem;
		private Label _lblPalaceNum;
		private Label _lblDoor;
		private Label _lblHiddenStem;

		private Label _lblHorse;
		private Label _lblVoid;

		public override void _Ready()
		{
			CustomMinimumSize = new Vector2(300, 300);

			var style = new StyleBoxFlat
			{
				BgColor = GlobalUIController.ColorSurface.Lightened(0.02f),
				BorderWidthTop = 2,
				BorderWidthLeft = 2,
				BorderWidthRight = 2,
				BorderWidthBottom = 2,
				BorderColor = GlobalUIController.ColorOutline
			};
			AddThemeStyleboxOverride("panel", style);

			SetupLayout();
		}

		private void SetupLayout()
		{
			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_left", 10);
			margin.AddThemeConstantOverride("margin_right", 10);
			margin.AddThemeConstantOverride("margin_top", 10);
			margin.AddThemeConstantOverride("margin_bottom", 10);
			AddChild(margin);

			var vBox = new VBoxContainer();
			vBox.SizeFlagsVertical = SizeFlags.ExpandFill;
			margin.AddChild(vBox);

			// --- Top Row ---
			var topCenter = new CenterContainer();
			_lblGod = CreateLabel(24, Colors.LightGray);
			topCenter.AddChild(_lblGod);
			vBox.AddChild(topCenter);

			vBox.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });

			// --- Middle Row ---
			var midRow = new HBoxContainer();
			midRow.Alignment = BoxContainer.AlignmentMode.Center;
			midRow.AddThemeConstantOverride("separation", 25);

			_lblHeavenStem = CreateRichLabel(28, GlobalUIController.ColorTextPrimary);
			_lblStar = CreateRichLabel(26, GlobalUIController.ColorTextPrimary);
			_lblEarthStem = CreateRichLabel(28, GlobalUIController.ColorTextPrimary);

			midRow.AddChild(_lblHeavenStem);
			midRow.AddChild(_lblStar);
			midRow.AddChild(_lblEarthStem);
			vBox.AddChild(midRow);

			vBox.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });

			// --- Bottom Row ---
			var botRow = new HBoxContainer();

			_lblPalaceNum = CreateLabel(36, Colors.Gray);

			var botRight = new HBoxContainer();
			botRight.AddThemeConstantOverride("separation", 20);
			_lblDoor = CreateLabel(28, GlobalUIController.ColorTextPrimary);
			_lblHiddenStem = CreateLabel(24, Colors.Gray);

			botRight.AddChild(_lblDoor);
			botRight.AddChild(_lblHiddenStem);

			botRow.AddChild(_lblPalaceNum);
			botRow.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
			botRow.AddChild(botRight);
			vBox.AddChild(botRow);

			// --- Overlays ---
			var overlay = new Control();
			overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
			overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
			margin.AddChild(overlay);

			_lblHorse = new Label { Text = "马", Visible = false };
			_lblHorse.AddThemeColorOverride("font_color", Colors.Yellow);
			_lblHorse.SetPosition(new Vector2(0, 0));
			overlay.AddChild(_lblHorse);

			_lblVoid = new Label { Text = "O", Visible = false };
			_lblVoid.AddThemeColorOverride("font_color", Colors.Gray);
			_lblVoid.AddThemeFontSizeOverride("font_size", 24);
			_lblVoid.SetAnchorsAndOffsetsPreset(LayoutPreset.TopRight);
			_lblVoid.SetPosition(new Vector2(250, 0));
			overlay.AddChild(_lblVoid);
		}

		/// <summary>
		/// Directly pass in Chart and Palace
		/// </summary>
		public void SetData(QiMenChart chart, QiMenPalace p)
		{
			if (p == null) return;

			// 1. Setup logical text content
			_lblGod.Text = p.God.GetLocalizedName();

			// Heaven Plate and Parasitic Stems
			if (p.HeavenPlateParasiticStem.HasValue)
				_lblHeavenStem.Text = $"[center]{p.HeavenPlateStem.GetLocalizedName()}\n[color=gray]{p.HeavenPlateParasiticStem.Value.GetLocalizedName()}[/color][/center]";
			else
				_lblHeavenStem.Text = $"[center]{p.HeavenPlateStem.GetLocalizedName()}[/center]";

			// Earth Plate and Parasitic Stems
			if (p.EarthPlateParasiticStem.HasValue)
				_lblEarthStem.Text = $"[center]{p.EarthPlateStem.GetLocalizedName()}\n[color=gray]{p.EarthPlateParasiticStem.Value.GetLocalizedName()}[/color][/center]";
			else
				_lblEarthStem.Text = $"[center]{p.EarthPlateStem.GetLocalizedName()}[/center]";

			// Nine Stars and Parasitic Star
			if (p.ParasiticStar.HasValue)
				_lblStar.Text = $"[center]{p.Star.GetLocalizedName()}\n[color=gray]{p.ParasiticStar.Value.GetLocalizedName()}[/color][/center]";
			else
				_lblStar.Text = $"[center]{p.Star.GetLocalizedName()}[/center]";

			_lblDoor.Text = p.Door.GetLocalizedName();
			_lblHiddenStem.Text = p.HiddenStem.GetLocalizedName();
			_lblPalaceNum.Text = p.Index.ToString();

			// 2. Get palace status for coloring and overlays
			var status = QiMenEvaluator.EvaluatePalaceStatus(chart, p.Index);

			// Setup Horse/Empty
			_lblHorse.Visible = status.Contains("Horse");
			_lblVoid.Visible = status.Contains("KongWang");

			// 3. Coloring
			_lblDoor.Modulate = Colors.White;
			_lblHeavenStem.Modulate = Colors.White;

			// Men Po (Death Door)
			if (status.Contains("MenPo")) _lblDoor.Modulate = new Color("#FF5252");

			// JiXing (Punishment) and RuMu (Entering Tomb) - Heaven Stem
			bool isJiXing = status.Contains("JiXing");
			bool isRuMu = status.Contains("RuMu");

			if (isJiXing && isRuMu)
				_lblHeavenStem.Modulate = new Color("#2196F3"); // Blue: JiXing + RuMu
			else if (isJiXing)
				_lblHeavenStem.Modulate = new Color("#AB47BC"); // Purple: JiXing only
			else if (isRuMu)
				_lblHeavenStem.Modulate = new Color("#66BB6A"); // Green: RuMu only
		}

		private Label CreateLabel(int fontSize, Color color)
		{
			var l = new Label();
			l.AddThemeFontSizeOverride("font_size", fontSize);
			l.AddThemeColorOverride("font_color", color);
			l.HorizontalAlignment = HorizontalAlignment.Center;
			return l;
		}

		private RichTextLabel CreateRichLabel(int fontSize, Color color)
		{
			var l = new RichTextLabel();
			l.BbcodeEnabled = true;
			l.FitContent = true;
			l.ScrollActive = false;
			l.CustomMinimumSize = new Vector2(70, 0);
			l.AddThemeFontSizeOverride("normal_font_size", fontSize);
			l.AddThemeColorOverride("default_color", color);
			return l;
		}
	}
}
