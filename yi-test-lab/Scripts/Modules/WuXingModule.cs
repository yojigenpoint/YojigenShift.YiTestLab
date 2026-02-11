using Godot;
using System;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class WuXingModule : VBoxContainer
	{
		private WuXingVisualizer _visualizer;
		private RichTextLabel _infoLabel;
		private HBoxContainer _btnContainer;

		public override void _Ready()
		{
			Name = "WuXingModule";
			AddThemeConstantOverride("separation", 30);
			var marginContainer = new MarginContainer();
			marginContainer.AddThemeConstantOverride("margin_left", 40);
			marginContainer.AddThemeConstantOverride("margin_right", 40);
			marginContainer.AddThemeConstantOverride("margin_top", 40);
			marginContainer.AddThemeConstantOverride("margin_bottom", 40);

			var title = new Label
			{
				Text = Tr("MOD_WUXING_TITLE"),
				HorizontalAlignment = HorizontalAlignment.Center
			};
			title.AddThemeFontSizeOverride("font_size", 48);
			AddChild(title);

			var centerBox = new CenterContainer();
			_visualizer = new WuXingVisualizer();
			centerBox.AddChild(_visualizer);
			AddChild(centerBox);

			_infoLabel = new RichTextLabel
			{
				BbcodeEnabled = true,
				FitContent = true,
				ScrollActive = false,
				CustomMinimumSize = new Vector2(800, 200),
				SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.ShrinkCenter,
				Text = $"[center]{Tr("MOD_WUXING_INFO_DEFAULT")}[/center]"
			};
			_infoLabel.AddThemeFontSizeOverride("font_size", 30);
			AddChild(_infoLabel);

			_btnContainer = new HBoxContainer();
			_btnContainer.Alignment = BoxContainer.AlignmentMode.Center;
			_btnContainer.AddThemeConstantOverride("separation", 20);
			AddChild(_btnContainer);

			SetupButtons();
		}

		private void SetupButtons()
		{
			foreach (WuXingType type in Enum.GetValues(typeof(WuXingType)))
			{
				if (type == WuXingType.None) continue;

				var btn = new Button
				{
					Text = type.GetLocalizedName(), 
					CustomMinimumSize = new Vector2(140, 80)
				};

				// 简单的样式
				var color = GlobalUIController.GetElementColor(type);
				var style = new StyleBoxFlat { BgColor = color, CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10, CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10 };
				btn.AddThemeStyleboxOverride("normal", style);
				btn.AddThemeStyleboxOverride("hover", style);
				btn.AddThemeColorOverride("font_color", Colors.Black);
				btn.AddThemeFontSizeOverride("font_size", 40);

				btn.Pressed += () => OnElementSelected(type);

				_btnContainer.AddChild(btn);
			}
		}

		private void OnElementSelected(WuXingType type)
		{
			_visualizer.SetActiveElement(type);

			var mother = type.Mother();
			var child = type.Child();
			var bane = type.Bane();
			var prisoner = type.Prisoner();

			string C(WuXingType t)
			{
				var color = GlobalUIController.GetElementColor(t);
				return $"[color=#{color.ToHtml()}][b]{t.GetLocalizedName()}[/b][/color]";
			}

			_infoLabel.Text = Helpers.GetLocalizedFormat("TXT_WUXING_RESULT",
				C(type), C(mother), C(child), C(bane), C(prisoner));
		}
	}
}
