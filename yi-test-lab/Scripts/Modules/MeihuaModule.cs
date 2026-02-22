using Godot;
using System;
using YojigenShift.YiFramework.Core;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiFramework.Structs;
using YojigenShift.YiTestLab.Modules.Components;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class MeihuaModule : VBoxContainer
	{
		private GlobalUIController _globalController;
		private DateTime _currentTime = DateTime.UtcNow;

		// UI 可视化组件
		private HexagramVisualizer _visOriginal;    // 本卦
		private HexagramVisualizer _visMutual;      // 互卦
		private HexagramVisualizer _visChanged;     // 变卦

		private RichTextLabel _lblOriginalInfo;
		private RichTextLabel _lblMutualInfo;
		private RichTextLabel _lblChangedInfo;

		private RichTextLabel _lblAnalysis;         // 结果分析报告

		public override void _Ready()
		{
			Name = "MeihuaModule";
			AddThemeConstantOverride("separation", 20);

			_globalController = GetNodeOrNull<Control>("/root/MainUI") as GlobalUIController;

			SetupUI();

			if (_globalController != null)
			{
				_currentTime = _globalController.CurrentTime;
				_globalController.GlobalTimeChanged += OnTimeChanged;
			}

			RefreshCast();
		}

		public override void _ExitTree()
		{
			if (_globalController != null) _globalController.GlobalTimeChanged -= OnTimeChanged;
		}

		private void OnTimeChanged(DateTime time)
		{
			_currentTime = time;
			RefreshCast();
		}

		private void SetupUI()
		{
			// 1. 标题
			var topBox = new VBoxContainer();
			topBox.Alignment = BoxContainer.AlignmentMode.Center;
			AddChild(topBox);

			var title = new Label { Text = "Mei Hua Yi Shu (梅花易数)", HorizontalAlignment = HorizontalAlignment.Center };
			title.AddThemeFontSizeOverride("font_size", 40);
			topBox.AddChild(title);

			var subTitle = new Label { Text = "Plum Blossom Divination - Time Casting", HorizontalAlignment = HorizontalAlignment.Center };
			subTitle.AddThemeColorOverride("font_color", Colors.Gray);
			topBox.AddChild(subTitle);

			// 2. 核心展示区 (本卦 -> 互卦 -> 变卦)
			var displayBox = new HBoxContainer();
			displayBox.Alignment = BoxContainer.AlignmentMode.Center;
			displayBox.AddThemeConstantOverride("separation", 50);
			AddChild(displayBox);

			var colOriginal = CreateHexColumn("Original (本卦)", out _visOriginal, out _lblOriginalInfo);
			displayBox.AddChild(colOriginal);

			displayBox.AddChild(CreateArrow("➔"));

			var colMutual = CreateHexColumn("Mutual (互卦)", out _visMutual, out _lblMutualInfo);
			displayBox.AddChild(colMutual);

			displayBox.AddChild(CreateArrow("➔"));

			var colChanged = CreateHexColumn("Changed (变卦)", out _visChanged, out _lblChangedInfo);
			displayBox.AddChild(colChanged);

			// 3. 详细分析报告面板
			var panel = new PanelContainer();
			panel.SizeFlagsVertical = SizeFlags.ExpandFill;
			var style = new StyleBoxFlat { BgColor = GlobalUIController.ColorSurface.Darkened(0.05f), CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12, CornerRadiusBottomLeft = 12, CornerRadiusBottomRight = 12 };
			panel.AddThemeStyleboxOverride("panel", style);
			AddChild(panel);

			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_left", 30); margin.AddThemeConstantOverride("margin_right", 30);
			margin.AddThemeConstantOverride("margin_top", 20); margin.AddThemeConstantOverride("margin_bottom", 20);
			panel.AddChild(margin);

			_lblAnalysis = new RichTextLabel
			{
				BbcodeEnabled = true,
				FitContent = true,
				ScrollActive = false
			};
			margin.AddChild(_lblAnalysis);
		}

		private VBoxContainer CreateHexColumn(string title, out HexagramVisualizer vis, out RichTextLabel info)
		{
			var vBox = new VBoxContainer();
			vBox.Alignment = BoxContainer.AlignmentMode.Center;
			vBox.AddThemeConstantOverride("separation", 15);

			var lblTitle = new Label { Text = title, HorizontalAlignment = HorizontalAlignment.Center };
			lblTitle.AddThemeColorOverride("font_color", GlobalUIController.ColorTextSecondary);
			vBox.AddChild(lblTitle);

			var panel = new PanelContainer();
			var style = new StyleBoxFlat { BgColor = GlobalUIController.ColorSurface.Lightened(0.05f), CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8 };
			panel.AddThemeStyleboxOverride("panel", style);
			vBox.AddChild(panel);

			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_left", 20); margin.AddThemeConstantOverride("margin_right", 20);
			margin.AddThemeConstantOverride("margin_top", 20); margin.AddThemeConstantOverride("margin_bottom", 20);
			panel.AddChild(margin);

			vis = new HexagramVisualizer();
			margin.AddChild(vis);

			info = new RichTextLabel
			{
				BbcodeEnabled = true,
				FitContent = true,
				ScrollActive = false,
				CustomMinimumSize = new Vector2(180, 0)
			};
			info.AddThemeFontSizeOverride("normal_font_size", 20);
			vBox.AddChild(info);

			return vBox;
		}

		private Label CreateArrow(string text)
		{
			var l = new Label { Text = text, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
			l.AddThemeFontSizeOverride("font_size", 40);
			l.AddThemeColorOverride("font_color", GlobalUIController.ColorOutline);
			return l;
		}

		// --- 核心起卦逻辑 ---

		private void RefreshCast()
		{
			try
			{
				// 1. 准备时间参数 (需要转成农历数字，这里暂用八字排盘取地支，公历月日代替农历月日作为演示)
				// 专业的易学软件需要包含公农历转换器，提取真正的 LunarMonth 和 LunarDay
				var bazi = BaziPlotter.Plot(_currentTime);

				// 梅花中地支数：子=1, 丑=2... 亥=12 (假设 EarthlyBranch 枚举是 0-11)
				int yearNum = (int)GanZhiMath.GetBranch(bazi.YearIdx) + 1;
				int monthNum = _currentTime.Month; // 理想状态应为 LunarMonth
				int dayNum = _currentTime.Day;     // 理想状态应为 LunarDay
				int hourNum = (int)GanZhiMath.GetBranch(bazi.HourIdx) + 1;

				// 2. 起卦
				MeiHuaResult result = MeiHuaCaster.CastByTime(yearNum, monthNum, dayNum, hourNum);
				MeiHuaAnalysis analysis = MeiHuaAnalyzer.Analyze(result);

				// 3. 渲染可视化
				// 互卦和变卦没有动爻，直接转为 Hexagram 渲染
				_visMutual.SetHexagram(new Hexagram(result.Mutual.Upper, result.Mutual.Lower));
				_visChanged.SetHexagram(new Hexagram(result.Changed.Upper, result.Changed.Lower));

				// 本卦包含动爻，需要提取成 YaoType[] 才能画 O 和 X
				YaoType[] originalLines = ConvertToYaoTypes(result.Original, result.MovingLine);
				_visOriginal.SetLines(originalLines);

				// 4. 更新文本信息 (标注体用)
				bool isLowerMoving = result.MovingLine <= 3;

				UpdateGuaText(_lblOriginalInfo, result.Original, isLowerMoving, "本卦");
				UpdateGuaText(_lblMutualInfo, result.Mutual, isLowerMoving, "互卦");
				UpdateGuaText(_lblChangedInfo, result.Changed, isLowerMoving, "变卦");

				// 5. 更新吉凶分析报告
				UpdateAnalysisReport(result, analysis, yearNum, monthNum, dayNum, hourNum);
			}
			catch (Exception ex)
			{
				_lblAnalysis.Text = $"[color=red]Error computing Mei Hua: {ex.Message}[/color]";
			}
		}

		// --- 辅助方法 ---

		/// <summary>
		/// 将 MeiHuaGua 转化为带有老阴/老阳的 YaoType 数组，供 HexagramVisualizer 绘制 O/X
		/// </summary>
		private YaoType[] ConvertToYaoTypes(MeiHuaGua gua, int movingLine)
		{
			YaoType[] lines = new YaoType[6];
			for (int i = 0; i < 6; i++)
			{
				int linePos = i + 1; // 1-6
				bool isYang;

				if (linePos <= 3) isYang = gua.Lower.GetLine(linePos);
				else isYang = gua.Upper.GetLine(linePos - 3);

				if (linePos == movingLine)
				{
					lines[i] = isYang ? YaoType.OldYang : YaoType.OldYin; // 动爻
				}
				else
				{
					lines[i] = isYang ? YaoType.YoungYang : YaoType.YoungYin; // 静爻
				}
			}
			return lines;
		}

		private void UpdateGuaText(RichTextLabel lbl, MeiHuaGua gua, bool isLowerMoving, string title)
		{
			// 上卦如果是体/用？ 动爻所在的卦是用卦，另一个是体卦
			string upperTag = !isLowerMoving ? "[color=orange][用][/color]" : "[color=cyan][体][/color]";
			string lowerTag = isLowerMoving ? "[color=orange][用][/color]" : "[color=cyan][体][/color]";

			Color upColor = GlobalUIController.GetElementColor(gua.Upper.GetWuXing());
			Color downColor = GlobalUIController.GetElementColor(gua.Lower.GetWuXing());

			string upperText = $"[color=#{upColor.ToHtml()}]{gua.Upper}[/color] {upperTag}";
			string lowerText = $"[color=#{downColor.ToHtml()}]{gua.Lower}[/color] {lowerTag}";

			lbl.Text = $"[center]{upperText}\n{lowerText}[/center]";
		}

		private void UpdateAnalysisReport(MeiHuaResult result, MeiHuaAnalysis analysis, int y, int m, int d, int h)
		{
			string colorCode = analysis.Level switch
			{
				AuspiceLevel.GreatFortune => "#00E676", // 绿
				AuspiceLevel.Fortune => "#81C784",      // 浅绿
				AuspiceLevel.SmallFortune => "#FFF176", // 黄
				AuspiceLevel.SmallMisfortune => "#FFB74D", // 橙
				AuspiceLevel.GreatMisfortune => "#FF5252", // 红
				_ => "#FFFFFF"
			};

			string title = $"[font_size=28][b]Analysis Report (五行生克推断)[/b][/font_size]\n";
			string paramLog = $"[color=gray]Time Params: Year({y}) + Month({m}) + Day({d}) + Hour({h}) = Moving Line: {result.MovingLine}[/color]\n\n";

			string elements = $"[font_size=24]Body (体): [color=cyan]{analysis.BodyTrigram}[/color] ({analysis.BodyWuXing})  vs  " +
							  $"App (用): [color=orange]{analysis.AppTrigram}[/color] ({analysis.AppWuXing})[/font_size]\n";

			string resultStr = $"[font_size=32]Result: [color={colorCode}][b]{analysis.Level} ({analysis.Comment})[/b][/color][/font_size]";

			_lblAnalysis.Text = title + paramLog + elements + resultStr;
		}
	}
}
