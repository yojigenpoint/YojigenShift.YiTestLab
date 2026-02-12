using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using YojigenShift.YiFramework.Core;
using YojigenShift.YiFramework.Enums;

namespace YojigenShift.YiTestLab.UI
{
	public partial class GlobalUIController : Control
	{
		#region 1. Design Tokens

		public static readonly Color ColorBGDeep = new Color("#121212");
		public static readonly Color ColorSurface = new Color("#1E1E1E");
		public static readonly Color ColorOutline = new Color("#333333");
		public static readonly Color ColorTextPrimary = new Color("#E0E0E0");
		public static readonly Color ColorTextSecondary = new Color("#9E9E9E");
		public static readonly Color ColorAccent = new Color("#03DAC6"); // i18n highlight

		// Colors reflection of five elements
		public static readonly Dictionary<WuXingType, Color> ElementColors = new Dictionary<WuXingType, Color>
		{
			{ WuXingType.Wood, new Color("#81C784") },
			{ WuXingType.Fire, new Color("#E57373") },
			{ WuXingType.Earth, new Color("#FFD54F") },
			{ WuXingType.Metal, new Color("#DFE1E5") },
			{ WuXingType.Water, new Color("#0096D6") }
		};

		#region Modules
		[Export] public PackedScene TimeDialScene { get; set; }
		[Export] public PackedScene BaziScene { get; set; }
		[Export] public PackedScene WuXingScene { get; set; }
		[Export] public PackedScene GanZhiScene { get; set; }
		#endregion

		public event Action<bool> GenderChanged;
		public bool IsMale { get; private set; } = true;

		public event Action<DateTime> GlobalTimeChanged;
		public DateTime CurrentTime { get; private set; } = DateTime.Now;

		public enum ModuleType { Interactive, Validation }

		private class ModuleConfig
		{
			public string Id { get; set; }
			public string TrKey { get; set; }
			public ModuleType Type { get; set; }
		}

		private List<ModuleConfig> _modules = new List<ModuleConfig>
		{
			new ModuleConfig { Id = "UI_MOD_BAZI", TrKey = "MOD_BAZI", Type = ModuleType.Interactive },
			new ModuleConfig { Id = "UI_MOD_NAMING", TrKey = "MOD_NAMING", Type = ModuleType.Interactive },
			new ModuleConfig { Id = "UI_MOD_QIMEN", TrKey = "MOD_QIMEN", Type = ModuleType.Interactive },
			new ModuleConfig { Id = "UI_MOD_LIUYAO", TrKey = "MOD_LIUYAO", Type = ModuleType.Interactive },
			new ModuleConfig { Id = "UI_MOD_MEIHUA", TrKey = "MOD_MEIHUA", Type = ModuleType.Interactive },

			new ModuleConfig { Id = "UI_MOD_WUXING", TrKey = "MOD_WUXING", Type = ModuleType.Validation },
			new ModuleConfig { Id = "UI_MOD_GANZHI", TrKey = "MOD_GANZHI", Type = ModuleType.Validation },
			new ModuleConfig { Id = "UI_MOD_TRIGRAM", TrKey = "MOD_TRIGRAM", Type = ModuleType.Validation },
			new ModuleConfig { Id = "UI_MOD_HEXAGRAM", TrKey = "MOD_HEXAGRAM", Type = ModuleType.Validation }
		};

		#endregion

		#region 2. UI

		private VBoxContainer _mainVBox;
		private PanelContainer _mainHeader;
		private PanelContainer _subHeader;
		private ScrollContainer _bodyContent;
		private PanelContainer _footer;

		private OptionButton _moduleSelector;
		private Button _langToggleBtn;

		private Node _currentModuleInstance;

		private bool _isEnglish = false;
		private string _currentModuleId = "UI_MOD_WUXING";

		private TimePickerDial _dialYear, _dialMonth, _dialDay, _dialHour, _dialMinute;
		private Button _genderBtn;

		#endregion

		public override void _Ready()
		{
			RenderingServer.SetDefaultClearColor(ColorBGDeep);

			SetupLayout();

			SyncLanguageState();

			UpdateGenderButtonVisuals();

			RefreshModuleSelector();

			SelectModuleById(_currentModuleId);
		}

		public override void _Notification(int what)
		{
			if (what == NotificationTranslationChanged)
			{
				SyncLanguageState();
				RefreshModuleSelector();
				SelectModuleById(_currentModuleId);
			}
		}

		private void SetupLayout()
		{
			_mainVBox = new VBoxContainer();
			_mainVBox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
			_mainVBox.AddThemeConstantOverride("separation", 0);
			AddChild(_mainVBox);

			// --- 1. Main Header ---
			_mainHeader = CreatePanel("MainHeader", 140);
			var headerHBox = new HBoxContainer();
			headerHBox.AddThemeConstantOverride("separation", 20);
			
			var headerMargin = new MarginContainer();
			headerMargin.AddThemeConstantOverride("margin_left", 30);
			headerMargin.AddThemeConstantOverride("margin_right", 30);
			headerMargin.AddThemeConstantOverride("margin_top", 20);
			headerMargin.AddThemeConstantOverride("margin_bottom", 20);
			headerMargin.AddChild(headerHBox);
			_mainHeader.AddChild(headerMargin);

			_moduleSelector = new OptionButton();
			_moduleSelector.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			_moduleSelector.AddThemeFontSizeOverride("font_size", 40);
			var popup = _moduleSelector.GetPopup();
			popup.AddThemeFontSizeOverride("font_size", 35);

			_moduleSelector.ItemSelected += OnModuleSelected;
			headerHBox.AddChild(_moduleSelector);

			_langToggleBtn = new Button();
			_langToggleBtn.Text = "EN";
			_langToggleBtn.CustomMinimumSize = new Vector2(100, 0);
			_langToggleBtn.AddThemeFontSizeOverride("font_size", 32);
			_langToggleBtn.ToggleMode = true;
			_langToggleBtn.Pressed += OnLanguageToggled;
			headerHBox.AddChild(_langToggleBtn);

			_mainVBox.AddChild(_mainHeader);

			// --- 2. Sub Header ---
			_subHeader = CreatePanel("SubHeader", 320);
			_subHeader.Visible = false;
			_mainVBox.AddChild(_subHeader);

			SetupTimeInputArea();

			// --- 3. Body ---
			_bodyContent = new ScrollContainer();
			_bodyContent.Name = "Body";
			_bodyContent.SizeFlagsVertical = SizeFlags.ExpandFill; 
			_mainVBox.AddChild(_bodyContent);

			// --- 4. Footer ---
			_footer = CreatePanel("Footer", 160);
			// TODO: Functional buttons
			_mainVBox.AddChild(_footer);
		}

		private void SetupTimeInputArea()
		{
			var hBox = new HBoxContainer();
			hBox.Alignment = BoxContainer.AlignmentMode.Center;
			hBox.AddThemeConstantOverride("separation", 15);

			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_top", 20);
			margin.AddThemeConstantOverride("margin_bottom", 20);
			margin.AddChild(hBox);
			_subHeader.AddChild(margin);

			// 1. Gender
			_genderBtn = new Button
			{
				CustomMinimumSize = new Vector2(160, 200),
				ToggleMode = true,
				FocusMode = FocusModeEnum.None
			};
			_genderBtn.AddThemeFontSizeOverride("font_size", 40);
			_genderBtn.Pressed += OnGenderToggled;
			hBox.AddChild(_genderBtn);
						
			hBox.AddChild(new VSeparator());

			// 2. Initialize 5 dials

			_dialYear = CreateDial("TXT_TIME_YEAR", 1900, 2100, CurrentTime.Year, hBox);
			_dialMonth = CreateDial("TXT_TIME_MONTH", 1, 12, CurrentTime.Month, hBox);
			_dialDay = CreateDial("TXT_TIME_DAY", 1, 31, CurrentTime.Day, hBox);

			hBox.AddChild(new VSeparator());

			_dialHour = CreateDial("TXT_TIME_HOUR", 0, 23, CurrentTime.Hour, hBox);
			_dialMinute = CreateDial("TXT_TIME_MIN", 0, 59, CurrentTime.Minute, hBox);
		}

		private void OnGenderToggled()
		{
			bool isFemale = _genderBtn.ButtonPressed;
			IsMale = !isFemale;

			UpdateGenderButtonVisuals();

			GenderChanged?.Invoke(IsMale);
		}


		private void UpdateGenderButtonVisuals()
		{
			if (IsMale)
			{
				_genderBtn.Text = Tr("TXT_GENDER_MALE");
				_genderBtn.Modulate = Colors.White;
			}
			else
			{
				_genderBtn.Text = Tr("TXT_GENDER_FEMALE");
				_genderBtn.Modulate = new Color("#FF80AB");
			}
		}


		private TimePickerDial CreateDial(string label, int min, int max, int current, Container parent)
		{
			TimePickerDial dial;
			if (TimeDialScene != null)
			{
				dial = TimeDialScene.Instantiate<TimePickerDial>();
			}
			else
			{
				dial = new TimePickerDial();
			}

			dial.LabelText = label;
			dial.MinValue = min;
			dial.MaxValue = max;
			dial.SetValue(current); 

			dial.ValueChanged += (val) => OnDateDialChanged(); 
			parent.AddChild(dial);
			return dial;
		}

		private void OnDateDialChanged()
		{
			// 1. Get current values
			int y = _dialYear.Value;
			int m = _dialMonth.Value;
			int d = _dialDay.Value;
			int h = _dialHour.Value;
			int min = _dialMinute.Value;

			// 2. Validate Date
			int daysInMonth = DateTime.DaysInMonth(y, m);
			if (d > daysInMonth)
			{
				d = daysInMonth;
				_dialDay.SetValue(d, false);
			}
			_dialDay.SetRange(1, daysInMonth);

			// 3. New DateTime
			try
			{
				var newTime = new DateTime(y, m, d, h, min, 0, DateTimeKind.Utc);
				CurrentTime = newTime;

				// 4. Broadcast
				GlobalTimeChanged?.Invoke(CurrentTime);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Invalid Date: {y}-{m}-{d} " + ex.Message);
			}
		}

		private void RefreshModuleSelector()
		{
			if (_moduleSelector == null) return;

			_moduleSelector.Clear();

			foreach (var mod in _modules.Where(m => m.Type == ModuleType.Interactive))
				AddModuleItem(mod);

			_moduleSelector.AddSeparator();

			foreach (var mod in _modules.Where(m => m.Type == ModuleType.Validation))
				AddModuleItem(mod);
		}


		private void AddModuleItem(ModuleConfig mod)
		{
			string label = Tr(mod.TrKey);
			_moduleSelector.AddItem(label);
			int index = _moduleSelector.ItemCount - 1;
			_moduleSelector.SetItemMetadata(index, mod.Id);
		}

		private void OnModuleSelected(long index)
		{
			if (_moduleSelector == null) return;

			var meta = _moduleSelector.GetItemMetadata((int)index);
			if (meta.VariantType == Variant.Type.Nil) return;

			string modId = meta.AsString();

			_currentModuleId = modId;

			var config = _modules.Find(m => m.Id == modId);
			if (config == null) return;

			if (_subHeader != null)
				_subHeader.Visible = (config.Type == ModuleType.Interactive);

			LoadModuleScene(modId);
		}


		private void SelectModuleById(string modId)
		{
			if (_moduleSelector == null) return;

			for (int i = 0; i < _modules.Count; i++)
			{
				var meta = _moduleSelector.GetItemMetadata(i);
				if (meta.VariantType != Variant.Type.Nil && meta.AsString() == modId)
				{
					_moduleSelector.Select(i);
					OnModuleSelected(i);
					return;
				}
			}
		}

		private void LoadModuleScene(string moduleId)
		{
			if (_currentModuleInstance != null)
			{
				_currentModuleInstance.QueueFree();
				_currentModuleInstance = null;
			}

			PackedScene targetScene = null;

			switch (moduleId)
			{
				case "UI_MOD_BAZI":
					targetScene = BaziScene;
					break;
				case "UI_MOD_WUXING":
					targetScene = WuXingScene;
					break;
				case "UI_MOD_GANZHI":
					targetScene = GanZhiScene;
					break;
			}

			if (targetScene != null)
			{
				_currentModuleInstance = targetScene.Instantiate();

				if (_currentModuleInstance is Control ctrl)
				{
					ctrl.SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.ShrinkCenter;
					ctrl.SizeFlagsVertical = SizeFlags.ExpandFill;
				}

				_bodyContent.AddChild(_currentModuleInstance);
			}
			else
			{
				var label = new Label
				{
					Text = Tr("MOD_WIP"),
					HorizontalAlignment = HorizontalAlignment.Center
				};
				_bodyContent.AddChild(label);
				_currentModuleInstance = label;
			}
		}


		private void OnLanguageToggled()
		{
			_isEnglish = !_isEnglish;

			string godotLocale = _isEnglish ? "en" : "zh_CN";
			string yiLangCode = _isEnglish ? "en_US" : "zh_CN";

			TranslationServer.SetLocale(godotLocale);
			YiLocalization.CurrentLanguage = yiLangCode;

			UpdateLangButtonVisuals();			
		}

		// --- Helpers ---

		private void SyncLanguageState()
		{
			string currentLocale = TranslationServer.GetLocale();
			_isEnglish = !currentLocale.StartsWith("zh");

			string yiLangCode = _isEnglish ? "en_US" : "zh_CN";
			YiLocalization.CurrentLanguage = yiLangCode;

			UpdateLangButtonVisuals();
		}

		private void UpdateLangButtonVisuals()
		{
			if (_langToggleBtn == null) return;

			_langToggleBtn.Text = _isEnglish ? "EN" : "中";
			_langToggleBtn.ButtonPressed = !_isEnglish;

			if (!_isEnglish)
				_langToggleBtn.Modulate = ColorAccent;
			else
				_langToggleBtn.Modulate = Colors.White;
		}

		private PanelContainer CreatePanel(string name, int minHeight)
		{
			var panel = new PanelContainer();
			panel.Name = name;
			panel.CustomMinimumSize = new Vector2(0, minHeight);

			var style = new StyleBoxFlat();
			style.BgColor = ColorSurface;
			style.BorderWidthBottom = name == "MainHeader" ? 2 : 0;
			style.BorderWidthTop = name == "Footer" ? 2 : 0;
			style.BorderColor = ColorOutline;

			panel.AddThemeStyleboxOverride("panel", style);
			return panel;
		}

		public static Color GetElementColor(WuXingType element)
		{
			return ElementColors.ContainsKey(element) ? ElementColors[element] : ColorTextPrimary;
		}
	}
}
