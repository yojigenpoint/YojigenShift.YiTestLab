using Godot;
using System;

namespace YojigenShift.YiTestLab.UI
{
	public partial class TimePickerDial : Control
	{
		[Export] public string LabelText = "TP_YEAR";
		[Export] public int MinValue = 1900;
		[Export] public int MaxValue = 2100;

		public int Value { get; private set; }

		public event Action<int> ValueChanged;

		private Label _lblTitle;
		private Label _lblValue;
		private Button _btnUp;
		private Button _btnDown;

		public override void _Ready()
		{
			CustomMinimumSize = new Vector2(140, 200);

			var vBox = new VBoxContainer();
			vBox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
			AddChild(vBox);

			// 1. Title
			_lblTitle = new Label
			{
				Text = LabelText,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			_lblTitle.AddThemeColorOverride("font_color", GlobalUIController.ColorTextSecondary);
			vBox.AddChild(_lblTitle);

			// 2. Up button (+)
			_btnUp = CreateArrowButton("▲");
			_btnUp.Pressed += () => ChangeValue(1);
			vBox.AddChild(_btnUp);

			// 3. Display
			_lblValue = new Label
			{
				Text = Value.ToString("00"),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};
			_lblValue.SizeFlagsVertical = SizeFlags.ExpandFill;
			_lblValue.AddThemeFontSizeOverride("font_size", 40);
			vBox.AddChild(_lblValue);

			// 4. Down button (-)
			_btnDown = CreateArrowButton("▼");
			_btnDown.Pressed += () => ChangeValue(-1);
			vBox.AddChild(_btnDown);
		}

		public void SetValue(int val, bool notify = false)
		{
			Value = Math.Clamp(val, MinValue, MaxValue);
			UpdateDisplay();
			if (notify) ValueChanged?.Invoke(Value);
		}

		public void SetRange(int min, int max)
		{
			MinValue = min;
			MaxValue = max;
			if (Value < MinValue) SetValue(MinValue, true);
			else if (Value > MaxValue) SetValue(MaxValue, true);
		}

		private void ChangeValue(int delta)
		{
			int newVal = Value + delta;
			
			if (newVal > MaxValue) newVal = MinValue;
			if (newVal < MinValue) newVal = MaxValue;

			Value = newVal;
			UpdateDisplay();
			ValueChanged?.Invoke(Value);
		}

		private void UpdateDisplay()
		{
			if (_lblValue != null)
				_lblValue.Text = Value.ToString("00");
		}

		private Button CreateArrowButton(string icon)
		{
			var btn = new Button { Text = icon, Flat = true, FocusMode = FocusModeEnum.None };
			btn.AddThemeColorOverride("font_color", GlobalUIController.ColorTextPrimary);
			btn.AddThemeColorOverride("font_hover_color", GlobalUIController.ColorAccent);
			return btn;
		}
	}
}
