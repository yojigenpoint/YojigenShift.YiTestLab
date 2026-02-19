using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YojigenShift.YiFramework.Core;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiFramework.Naming;
using YojigenShift.YiFramework.Naming.Data;
using YojigenShift.YiFramework.Naming.Interfaces;
using YojigenShift.YiFramework.Naming.Logic;
using YojigenShift.YiFramework.Naming.Models;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class NamingModule : VBoxContainer
	{
		private GlobalUIController _globalController;
		private ICharInfoProvider _charProvider;
		private HanNamingService _namingService;

		private LineEdit _inputSurname;
		private LineEdit _inputName;
		private Button _btnAnalyze;
		private Button _btnRecommend;
		private RichTextLabel _reportLabel;
		private NameFlowVisualizer _visualizer;
		private Label _lblUsefulGods;

		private List<WuXingType> _currentUsefulGods = new List<WuXingType>();

		public override void _Ready()
		{
			Name = "NamingModule";

			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill;
			SizeFlagsVertical = SizeFlags.Expand | SizeFlags.Fill;

			_charProvider = new DemoCharProvider();
			_namingService = new HanNamingService(_charProvider);

			_globalController = GetNodeOrNull<Control>("/root/MainUI") as GlobalUIController;

			SetupUI();

			if (_globalController != null)
			{
				UpdateBaziContext(_globalController.CurrentTime);
				_globalController.GlobalTimeChanged += UpdateBaziContext;
			}
		}

		public override void _ExitTree()
		{
			if (_globalController != null)
				_globalController.GlobalTimeChanged -= UpdateBaziContext;
		}

		private void SetupUI()
		{
			var outerMargin = new MarginContainer();
			outerMargin.SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill;
			outerMargin.SizeFlagsVertical = SizeFlags.Expand | SizeFlags.Fill;

			int margin = 50;
			outerMargin.AddThemeConstantOverride("margin_left", margin);
			outerMargin.AddThemeConstantOverride("margin_right", margin);
			outerMargin.AddThemeConstantOverride("margin_top", margin);
			outerMargin.AddThemeConstantOverride("margin_bottom", margin);

			AddChild(outerMargin);

			var contentVBox = new VBoxContainer();
			contentVBox.SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill;
			contentVBox.SizeFlagsVertical = SizeFlags.Expand | SizeFlags.Fill;
			contentVBox.AddThemeConstantOverride("separation", 20);
			outerMargin.AddChild(contentVBox);

			// 1. Title
			var headerBox = new VBoxContainer();
			contentVBox.AddChild(headerBox);

			var title = new Label { Text = "五行姓名实验室", HorizontalAlignment = HorizontalAlignment.Center };
			title.AddThemeFontSizeOverride("font_size", 40);
			headerBox.AddChild(title);

			_lblUsefulGods = new Label
			{
				Text = "当前喜用神：计算中……",
				HorizontalAlignment = HorizontalAlignment.Center
			};
			_lblUsefulGods.AddThemeColorOverride("font_color", GlobalUIController.ColorAccent);
			_lblUsefulGods.AddThemeFontSizeOverride("font_size", 25);
			headerBox.AddChild(_lblUsefulGods);

			// 2. Input
			var inputBox = new HBoxContainer();
			inputBox.Alignment = BoxContainer.AlignmentMode.Center;
			inputBox.CustomMinimumSize = new Vector2(0, 50);
			inputBox.AddThemeConstantOverride("separation", 10);
			contentVBox.AddChild(inputBox);

			_inputSurname = new LineEdit { PlaceholderText = "姓", CustomMinimumSize = new Vector2(150, 30), SizeFlagsHorizontal = SizeFlags.ExpandFill };
			_inputName = new LineEdit { PlaceholderText = "名", CustomMinimumSize = new Vector2(200, 30), SizeFlagsHorizontal = SizeFlags.ExpandFill };

			_btnAnalyze = new Button { Text = "分析名字", SizeFlagsHorizontal = SizeFlags.ExpandFill };
			_btnAnalyze.Pressed += OnAnalyzePressed;

			_btnRecommend = new Button { Text = "生成姓名", SizeFlagsHorizontal = SizeFlags.ExpandFill };
			_btnRecommend.Pressed += OnRecommendPressed;

			_inputSurname.AddThemeFontSizeOverride("font_size", 30);
			_inputName.AddThemeFontSizeOverride("font_size", 30);
			_btnAnalyze.AddThemeFontSizeOverride("font_size", 30);
			_btnRecommend.AddThemeFontSizeOverride("font_size", 30);

			inputBox.AddChild(_inputSurname);
			inputBox.AddChild(_inputName);
			inputBox.AddChild(_btnAnalyze);
			inputBox.AddChild(_btnRecommend);

			// 3. Visuals
			var visContainer = new PanelContainer();
			var visStyle = new StyleBoxFlat { BgColor = GlobalUIController.ColorSurface.Darkened(0.1f), CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10, CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10 };
			visContainer.AddThemeStyleboxOverride("panel", visStyle);
			visContainer.CustomMinimumSize = new Vector2(0, 320);
			contentVBox.AddChild(visContainer);

			var centerVis = new CenterContainer();
			visContainer.AddChild(centerVis);

			_visualizer = new NameFlowVisualizer();
			centerVis.AddChild(_visualizer);

			// 4. Report
			var reportPanel = new PanelContainer();
			reportPanel.SizeFlagsVertical = SizeFlags.ExpandFill;
			var reportStyle = new StyleBoxFlat { BgColor = GlobalUIController.ColorSurface };
			reportPanel.AddThemeStyleboxOverride("panel", reportStyle);
			contentVBox.AddChild(reportPanel);

			_reportLabel = new RichTextLabel
			{
				BbcodeEnabled = true,
				FitContent = false,
				SizeFlagsVertical = SizeFlags.ExpandFill,
			};
			var textMargin = new MarginContainer();
			textMargin.AddThemeConstantOverride("margin_left", 20); textMargin.AddThemeConstantOverride("margin_right", 20); textMargin.AddThemeConstantOverride("margin_top", 20);
			textMargin.AddChild(_reportLabel);
			reportPanel.AddChild(textMargin);
		}

		// --- Core Logic ---

		private void UpdateBaziContext(DateTime time)
		{
			try
			{
				var (y, m, d, h) = BaziPlotter.Plot(time);
				var result = BaziEvaluator.Evaluate(y, m, d, h);
				_currentUsefulGods = result.UsefulGods;

				string godStr = string.Join(", ", _currentUsefulGods.Select(g => g.GetLocalizedName()));
				_lblUsefulGods.Text = $"八字喜用神推荐: {godStr}";
			}
			catch
			{
				_lblUsefulGods.Text = "八字错误";
			}
		}

		private void OnAnalyzePressed()
		{
			string surname = _inputSurname.Text.Trim();
			string name = _inputName.Text.Trim();

			if (string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(name))
			{
				_reportLabel.Text = "[color=red]请同时输入姓和名[/color]";
				return;
			}

			// 1. Character Attributes
			var fullString = surname + name;
			var charList = new List<CharAttributes>();
			var missingChars = new List<char>();

			foreach (char c in fullString)
			{
				var attr = _charProvider.GetCharInfo(c);
				if (attr != null)
				{
					charList.Add(attr);
				}
				else
				{
					missingChars.Add(c);
					charList.Add(new CharAttributes { Character = c, KangXiStrokes = 0, MainWuXing = WuXingType.None, Pinyin = "?" });
				}
			}

			// 2. 如果姓氏数据缺失，尝试数理回退 (HanNumerology)
			// 这里为了演示，我们假设用户在 DemoCharProvider 里输入了这些字

			// 3. Update Visuals
			_visualizer.Visualize(charList, _currentUsefulGods);

			// 4. Generates Report
			var sb = new StringBuilder();
			sb.Append("[font_size=25]");
			sb.Append($"[font_size=30][b]姓名分析：{surname}{name}[/b][/font_size]\n\n");

			if (missingChars.Count > 0)
			{
				sb.Append($"[color=yellow]【警告】汉字不在数据库： {string.Join("", missingChars)}。分析结果也许会不准确[/color]\n\n");
			}

			sb.Append("[b]结构五行流向：[/b]\n");
			for (int i = 0; i < charList.Count - 1; i++)
			{
				var c1 = charList[i];
				var c2 = charList[i + 1];
				if (c1.MainWuXing == WuXingType.None || c2.MainWuXing == WuXingType.None) continue;

				string rel = "同";
				string color = "white";
				if (c2.MainWuXing.IsGeneratedBy(c1.MainWuXing)) { rel = "顺生"; color = "green"; }
				else if (c2.MainWuXing.IsOvercomeBy(c1.MainWuXing)) { rel = "受克"; color = "red"; }
				else if (c1.MainWuXing.IsOvercomeBy(c2.MainWuXing)) { rel = "逆克"; color = "red"; }

				sb.Append($"- {c1.Character}（{c1.MainWuXing.GetLocalizedName()}）-> {rel} -> {c2.Character}（{c2.MainWuXing.GetLocalizedName()}）[color={color}]●[/color]\n");
			}

			sb.Append("\n[b]八字相合：[/b]\n");
			int matches = 0;
			foreach (var c in charList)
			{
				if (c == charList[0]) continue;

				if (_currentUsefulGods.Contains(c.MainWuXing))
				{
					string cCode = GlobalUIController.GetElementColor(c.MainWuXing).ToHtml();
					sb.Append($"- {c.Character}：适配喜用神 [color=#{cCode}]{c.MainWuXing.GetLocalizedName()}[/color] (+10)\n");
					matches++;
				}
			}
			if (matches == 0) sb.Append("[color=gray]名字没有与喜用神相配[/color][/font_size]\n");

			_reportLabel.Text = sb.ToString();
		}

		private void OnRecommendPressed()
		{
			string surnameStr = _inputSurname.Text.Trim();
			if (string.IsNullOrEmpty(surnameStr))
			{
				_reportLabel.Text = "[color=red]请输入姓以获取推荐名[/color]";
				return;
			}

			var sAttr = _charProvider.GetCharInfo(surnameStr[0]);
			WuXingType sWuXing = sAttr != null ? sAttr.MainWuXing : WuXingType.Earth; // Fallback

			var patterns = PatternGenerator.GeneratePatterns(sWuXing, _currentUsefulGods, 2);

			var sb = new StringBuilder();
			sb.Append("[font_size=25]");
			sb.Append($"[font_size=30][b]推荐组合[/b][/font_size]\n");
			sb.Append($"【基本信息】姓：{sWuXing.GetLocalizedName()}，用神：{string.Join("，", _currentUsefulGods.Select(g => g.GetLocalizedName()))}\n\n");

			foreach (var p in patterns.Take(5)) // First 5
			{
				string desc = p.Description;
				string seq = string.Join(" -> ", p.Sequence.Select(s => s.GetLocalizedName()));
				string color = p.Score >= 80 ? "green" : "white";

				sb.Append($"[color={color}]★ {seq} ({p.Score}分)[/color]\n");
				sb.Append($"   {desc}\n\n");
			}

			// Recommend name list
			GenderPreference genderReq = _globalController.IsMale ? GenderPreference.MaleOnly : GenderPreference.FemaleOnly;

			var request = new NamingRequest
			{
				Surname = surnameStr,
				SurnameWuXing = sWuXing,
				BirthTimeUtc = _globalController.CurrentTime,
				Gender = genderReq,
				NameLength = 2,
				MaxResults = 10
			};
			
			var candidates = _namingService.RecommendNames(request);

			sb.Append($"[font_size=30][b]推荐姓名[/b][/font_size]\n");

			if (candidates.Count == 0)
				sb.Append("当前数据库找不到合适的名字。");
			else
			{
				int rank = 1;
				foreach (var c in candidates)
				{
					string scoreColor = c.TotalScore > 80 ? "#00E676" : "#FFFFFF";

					sb.Append($"[font_size=35]{rank}. [color={scoreColor}][b]{c.FullName}[/b][/color]【{c.TotalScore}分】[/font_size]\n");

					var details = c.NameChars.Select(ch => $"{ch.Character}（{ch.MainWuXing.GetLocalizedName()}）").ToList();
					sb.Append($"	详情：{string.Join(" + ", details)}\n");

					sb.Append($"	组合：{c.Pattern.Description}\n");

					foreach (var w in c.Warnings)
						sb.Append($"	[color=orange]⚠ {w}[/color]\n");

					sb.Append("\n");
					rank++;
				}
			}

				_reportLabel.Text = sb.Append("[/font_size]").ToString();
		}
	}
}
