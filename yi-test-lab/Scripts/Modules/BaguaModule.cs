using Godot;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class BaguaModule : VBoxContainer
	{
		private BaguaVisualizer _visualizer;
		private RichTextLabel _infoLabel;
		private Button _btnSwitch;

		private BaguaSequence _currentSeq = BaguaSequence.EarlyHeaven;

		public override void _Ready()
		{
			Name = "BaguaModule";
			AddThemeConstantOverride("separation", 20);

			// 1. Title
			var title = new Label { Text = Tr("MOD_TRIGRAM_TITLE"), HorizontalAlignment = HorizontalAlignment.Center };
			title.AddThemeFontSizeOverride("font_size", 40);
			AddChild(title);

			// 2. Switch Button
			var topBar = new HBoxContainer();
			topBar.Alignment = BoxContainer.AlignmentMode.Center;
			AddChild(topBar);

			_btnSwitch = new Button
			{
				Text = Tr("TXT_TRIGRAM_SWITCH_BTN_LH"),
				CustomMinimumSize = new Vector2(300, 60)
			};
			_btnSwitch.Pressed += OnSwitchPressed;
			topBar.AddChild(_btnSwitch);

			// 3. Visuals
			var centerBox = new CenterContainer();
			centerBox.SizeFlagsVertical = SizeFlags.ExpandFill;
			AddChild(centerBox);

			_visualizer = new BaguaVisualizer();
			_visualizer.TrigramClicked += OnTrigramClicked;
			centerBox.AddChild(_visualizer);

			// 4. Info
			var panel = new PanelContainer();
			var style = new StyleBoxFlat { BgColor = GlobalUIController.ColorSurface, CornerRadiusTopLeft = 16, CornerRadiusTopRight = 16 };
			panel.AddThemeStyleboxOverride("panel", style);
			panel.CustomMinimumSize = new Vector2(0, 200);

			_infoLabel = new RichTextLabel
			{
				BbcodeEnabled = true,
				FitContent = true,
				SizeFlagsVertical = SizeFlags.ShrinkCenter,
				CustomMinimumSize = new Vector2(800, 0)
			};
			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_top", 20); margin.AddThemeConstantOverride("margin_left", 30);
			margin.AddChild(_infoLabel);
			panel.AddChild(margin);

			AddChild(panel);

			// Init
			UpdateInfo(TrigramName.Qian);
		}

		private void OnSwitchPressed()
		{
			if (_currentSeq == BaguaSequence.EarlyHeaven)
			{
				_currentSeq = BaguaSequence.LaterHeaven;
				_btnSwitch.Text = Tr("TXT_TRIGRAM_SWITCH_BTN_EH");
			}
			else
			{
				_currentSeq = BaguaSequence.EarlyHeaven;
				_btnSwitch.Text = Tr("TXT_TRIGRAM_SWITCH_BTN_LH");
			}

			_visualizer.SetSequence(_currentSeq);
		}

		private void OnTrigramClicked(TrigramName trigram)
		{
			UpdateInfo(trigram);
		}

		private void UpdateInfo(TrigramName t)
		{
			var wuxing = t.GetWuXing();
			var preNum = t.GetPreHeavenNumber();
			var postNum = t.GetPostHeavenNumber();

			string colorHex = GlobalUIController.GetElementColor(wuxing).ToHtml();

			// Placeholder
			string image = GetImageMeaning(t);

			_infoLabel.Text = $"[center][font_size=48]Selected: [color=#{colorHex}]{t}[/color][/font_size]\n" +
							  $"[font_size=28]Nature: {image} | Element: {wuxing}[/font_size]\n" +
							  $"[font_size=24]Early Num: {preNum} | Later Num: {postNum}[/font_size][/center]";
		}

		private string GetImageMeaning(TrigramName t)
		{
			return t switch
			{
				TrigramName.Qian => "Heaven (天)",
				TrigramName.Kun => "Earth (地)",
				TrigramName.Zhen => "Thunder (雷)",
				TrigramName.Xun => "Wind (风)",
				TrigramName.Kan => "Water (水)",
				TrigramName.Li => "Fire (火)",
				TrigramName.Gen => "Mountain (山)",
				TrigramName.Dui => "Lake (泽)",
				_ => ""
			};
		}
	}
}
