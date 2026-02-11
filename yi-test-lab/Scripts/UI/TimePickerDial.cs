using Godot;
using System;

namespace YojigenShift.YiTestLab.UI
{
	public partial class TimePickerDial : Control
	{
		[Export] public string LabelText = "TP_YEAR";
		[Export] public int MinValue = 1900;
		[Export] public int MaxValue = 2100;
		[Export] public int CurrentValue = 2026;

		private Label _label;
		private ScrollContainer _scroll;
		private VBoxContainer _list;
		private LineEdit _manualInput;
		private bool _isEditing = false;

		public override void _Ready()
		{
			CustomMinimumSize = new Vector2(180, 220);

			var vBox = new VBoxContainer();
			vBox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
			AddChild(vBox);

			// 1. Label
			_label = new Label { Text = LabelText, HorizontalAlignment = HorizontalAlignment.Center };
			_label.AddThemeFontSizeOverride("font_size", 32);
			vBox.AddChild(_label);

			// 2. Container
			_scroll = new ScrollContainer
			{
				CustomMinimumSize = new Vector2(0, 150),
				HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
				VerticalScrollMode = ScrollContainer.ScrollMode.ShowNever
			};
			vBox.AddChild(_scroll);

			_list = new VBoxContainer();
			_scroll.AddChild(_list);

			PopulateNumbers();

			// 3. Hidden input
			_manualInput = new LineEdit
			{
				Visible = false,
				Alignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(0, 80)
			};
			_manualInput.TextSubmitted += OnManualInputSubmitted;
			vBox.AddChild(_manualInput);
		}

		private void PopulateNumbers()
		{
			for (int i = MinValue; i <= MaxValue; i++)
			{
				var lbl = new Button
				{
					Text = i.ToString(),
					Flat = true,
					CustomMinimumSize = new Vector2(0, 60)
				};
				lbl.Pressed += () => StartManualInput();
				_list.AddChild(lbl);
			}
		}

		private void StartManualInput()
		{
			_isEditing = true;
			_scroll.Visible = false;
			_manualInput.Visible = true;
			_manualInput.Text = CurrentValue.ToString();
			_manualInput.GrabFocus();
		}

		private void OnManualInputSubmitted(string text)
		{
			if (int.TryParse(text, out int val))
			{
				CurrentValue = Math.Clamp(val, MinValue, MaxValue);
			}
			_isEditing = false;
			_scroll.Visible = true;
			_manualInput.Visible = false;
		}
	}
}
