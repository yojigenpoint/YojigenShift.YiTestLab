using Godot;
using System;
using YojigenShift.YiFramework.Core;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiFramework.LiuYao.Logic;
using YojigenShift.YiFramework.LiuYao.Models;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class LiuyaoModule : VBoxContainer
	{
		private GlobalUIController _globalController;
		private DateTime _currentTime = DateTime.UtcNow;

		private Button _btnCast;
		private RichTextLabel _lblHeader;
		private VBoxContainer _linesContainer;
		private RichTextLabel _lblSummary;

		private LiuYaoResult _currentResult = null;

		public override void _Ready()
		{
			Name = "LiuyaoModule";
			AddThemeConstantOverride("separation", 15);

			_globalController = GetNodeOrNull<Control>("/root/MainUI") as GlobalUIController;

			SetupUI();

			if (_globalController != null)
			{
				_currentTime = _globalController.CurrentTime;
				_globalController.GlobalTimeChanged += OnTimeChanged;
			}

			// 初次不排盘，等待用户点击起卦
			UpdateHeaderOnly();
		}

		public override void _ExitTree()
		{
			if (_globalController != null) _globalController.GlobalTimeChanged -= OnTimeChanged;
		}

		private void OnTimeChanged(DateTime time)
		{
			_currentTime = time;
			if (_currentResult == null)
				UpdateHeaderOnly();
			else
				RenderChart(); // 如果已经起卦，时间变动会刷新六神和旺衰
		}

		private void SetupUI()
		{
			// 1. Title & Action
			var topBox = new HBoxContainer();
			topBox.Alignment = BoxContainer.AlignmentMode.Center;
			topBox.AddThemeConstantOverride("separation", 30);
			AddChild(topBox);

			var title = new Label { Text = "Liu Yao (六爻纳甲)", HorizontalAlignment = HorizontalAlignment.Center };
			title.AddThemeFontSizeOverride("font_size", 36);
			topBox.AddChild(title);

			_btnCast = new Button
			{
				Text = "Toss Coins (摇卦)",
				CustomMinimumSize = new Vector2(180, 50)
			};
			_btnCast.AddThemeColorOverride("font_color", GlobalUIController.ColorAccent);
			_btnCast.Pressed += OnCastPressed;
			topBox.AddChild(_btnCast);

			// 2. Context Header
			_lblHeader = new RichTextLabel
			{
				BbcodeEnabled = true,
				FitContent = true,
				ScrollActive = false
			};
			AddChild(_lblHeader);

			// 3. The Hexagram Body
			var panel = new PanelContainer();
			panel.SizeFlagsVertical = SizeFlags.ExpandFill;
			var style = new StyleBoxFlat { BgColor = GlobalUIController.ColorSurface.Darkened(0.1f), CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8 };
			panel.AddThemeStyleboxOverride("panel", style);
			AddChild(panel);

			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_left", 20); margin.AddThemeConstantOverride("margin_right", 20);
			margin.AddThemeConstantOverride("margin_top", 20); margin.AddThemeConstantOverride("margin_bottom", 20);
			panel.AddChild(margin);

			_linesContainer = new VBoxContainer();
			_linesContainer.AddThemeConstantOverride("separation", 12);
			_linesContainer.Alignment = BoxContainer.AlignmentMode.Center;
			margin.AddChild(_linesContainer);

			// 4. Summary Footer
			_lblSummary = new RichTextLabel
			{
				BbcodeEnabled = true,
				FitContent = true,
				ScrollActive = false
			};
			AddChild(_lblSummary);
		}

		private void OnCastPressed()
		{
			_currentResult = CoinOracle.Cast();
			RenderChart();
		}

		private void UpdateHeaderOnly()
		{
			var bazi = BaziPlotter.Plot(_currentTime);
			var monthBranch = GanZhiMath.GetBranch(bazi.MonthIdx);
			var dayStem = GanZhiMath.GetStem(bazi.DayIdx);
			var dayBranch = GanZhiMath.GetBranch(bazi.DayIdx);

			_lblHeader.Text = $"[center][color=gray]Current Time Context: Month of [b]{monthBranch}[/b], Day of [b]{dayStem}{dayBranch}[/b][/color][/center]";
		}

		private void RenderChart()
		{
			if (_currentResult == null) return;

			// 1. 获取时间上下文
			var bazi = BaziPlotter.Plot(_currentTime);
			var monthBranch = GanZhiMath.GetBranch(bazi.MonthIdx);
			var dayStem = GanZhiMath.GetStem(bazi.DayIdx);
			var dayBranch = GanZhiMath.GetBranch(bazi.DayIdx);

			// 2. 挂载本卦与神煞
			var mounted = NaJiaLogic.Mount(_currentResult);
			SixBeastsLogic.AttachContext(mounted, dayStem, dayBranch);

			// 3. 计算旺衰状态
			var statusDict = LiuYaoEvaluator.Evaluate(mounted, monthBranch, dayBranch);

			// 4. 计算变卦 (为了获取变爻的干支和六亲)
			var futureResult = new LiuYaoResult();
			foreach (var yao in _currentResult.Lines)
			{
				futureResult.Lines.Add(yao.GetFuturePolarity() == Polarity.Yang ? YaoType.YoungYang : YaoType.YoungYin);
			}
			var mountedFuture = NaJiaLogic.Mount(futureResult);

			// 5. 渲染 Header
			string palaceHex = GlobalUIController.GetElementColor(mounted.PalaceWuXing).ToHtml();
			string hexName = Enum.IsDefined(typeof(HexagramName), mounted.Hexagram.Value) ? ((HexagramName)mounted.Hexagram.Value).ToString() : "Unknown";
			string futureHexName = Enum.IsDefined(typeof(HexagramName), mountedFuture.Hexagram.Value) ? ((HexagramName)mountedFuture.Hexagram.Value).ToString() : "Unknown";

			_lblHeader.Text = $"[center][font_size=20]Month: {monthBranch} | Day: {dayStem}{dayBranch}[/font_size]\n" +
							  $"[font_size=28][b]{hexName}[/b][/font_size]  ➔  [font_size=24][color=gray]{futureHexName}[/color][/font_size]\n" +
							  $"Palace: [color=#{palaceHex}]{mounted.Palace}[/color] ({mounted.PalaceWuXing})[/center]";

			// 6. 渲染六爻 (从上往下渲染：Index 5 -> 0)
			foreach (Node child in _linesContainer.GetChildren()) child.QueueFree();

			for (int i = 5; i >= 0; i--)
			{
				var lineInfo = mounted.Lines[i];
				var lineStatus = statusDict[lineInfo.Index];
				var originalType = _currentResult.Lines[i];
				var futureLineInfo = mountedFuture.Lines[i];

				_linesContainer.AddChild(CreateLineRow(lineInfo, originalType, lineStatus, futureLineInfo, mounted.PalaceWuXing));
			}

			// 7. 底部信息总结
			string movingLinesStr = _currentResult.MovingLines.Count > 0 ? string.Join(", ", _currentResult.MovingLines) : "None (Static Hexagram)";
			_lblSummary.Text = $"[center][color=gray]Moving Lines: {movingLinesStr} | Shi Yao: Line {mounted.ShiIndex} | Ying Yao: Line {mounted.YingIndex}[/color][/center]";
		}

		private Control CreateLineRow(YaoInfo info, YaoType type, YaoStatus status, YaoInfo futureInfo, WuXingType palaceWuXing)
		{
			var hBox = new HBoxContainer();
			hBox.Alignment = BoxContainer.AlignmentMode.Center;
			hBox.AddThemeConstantOverride("separation", 15);

			// 1. 六神 (Six Beast)
			Color beastColor = GetBeastColor(info.Beast);
			hBox.AddChild(CreateLabel(TranslateBeast(info.Beast), beastColor, 60));

			// 2. 六亲 (Six Relation)
			hBox.AddChild(CreateLabel(TranslateRelation(info.Relation), Colors.LightGray, 60));

			// 3. 干支 + 五行
			Color wxColor = GlobalUIController.GetElementColor(info.WuXing);
			hBox.AddChild(CreateLabel($"{info.Stem}{info.Branch}({info.WuXing})", wxColor, 100));

			// 4. 爻象图形 (Line Symbol)
			hBox.AddChild(CreateLineVisual(type));

			// 5. 世应 (Shi/Ying)
			string syText = info.IsShi ? "[世]" : (info.IsYing ? "[应]" : "");
			Color syColor = info.IsShi ? GlobalUIController.ColorAccent : Colors.LightGray;
			hBox.AddChild(CreateLabel(syText, syColor, 40));

			// 6. 状态 (Status - 空, 破, 冲, 合)
			string statusStr = info.IsKongWang ? "空亡 " : "";
			statusStr += status.ToString();
			hBox.AddChild(CreateLabel(statusStr, Colors.Gray, 120, HorizontalAlignment.Left));

			// 7. 变爻 (Changed Line)
			if (type.IsMoving())
			{
				// 变爻的六亲是以"本卦"的宫位五行为基准计算的
				var changedRelation = CalculateRelation(futureInfo.WuXing, palaceWuXing);
				string chgText = $"➔ {TranslateRelation(changedRelation)} {futureInfo.Stem}{futureInfo.Branch}";
				Color chgColor = GlobalUIController.GetElementColor(futureInfo.WuXing);
				hBox.AddChild(CreateLabel(chgText, chgColor, 150, HorizontalAlignment.Left));
			}
			else
			{
				// 占位符保持对齐
				hBox.AddChild(CreateLabel("", Colors.Transparent, 150));
			}

			return hBox;
		}

		// --- UI 渲染辅助 ---

		private Label CreateLabel(string text, Color color, int minWidth, HorizontalAlignment align = HorizontalAlignment.Center)
		{
			var l = new Label { Text = text, HorizontalAlignment = align, VerticalAlignment = VerticalAlignment.Center };
			l.CustomMinimumSize = new Vector2(minWidth, 0);
			l.AddThemeColorOverride("font_color", color);
			l.AddThemeFontSizeOverride("font_size", 20);
			return l;
		}

		private RichTextLabel CreateLineVisual(YaoType type)
		{
			var r = new RichTextLabel { BbcodeEnabled = true, FitContent = true, ScrollActive = false };
			r.CustomMinimumSize = new Vector2(100, 0);
			r.AddThemeFontSizeOverride("normal_font_size", 24);

			string lineColor = GlobalUIController.ColorTextPrimary.ToHtml();
			string markColor = GlobalUIController.ColorAccent.ToHtml();

			// ASCII Art 绘制爻象
			if (type == YaoType.YoungYang)
				r.Text = $"[center][color=#{lineColor}]━━━━━[/color][/center]";
			else if (type == YaoType.YoungYin)
				r.Text = $"[center][color=#{lineColor}]━━ ━━[/color][/center]";
			else if (type == YaoType.OldYang)
				r.Text = $"[center][color=#{lineColor}]━━━━━[/color] [color=#{markColor}]O[/color][/center]";
			else if (type == YaoType.OldYin)
				r.Text = $"[center][color=#{lineColor}]━━ ━━[/color] [color=#{markColor}]X[/color][/center]";

			return r;
		}

		// --- 逻辑与翻译辅助 ---

		private SixRelation CalculateRelation(WuXingType line, WuXingType palace)
		{
			var interaction = WuXingMath.Compare(line, palace);
			return interaction switch
			{
				InteractionType.Same => SixRelation.Brother,
				InteractionType.Generates => SixRelation.Parent,
				InteractionType.Overcomes => SixRelation.Officer,
				InteractionType.GeneratedBy => SixRelation.Offspring,
				InteractionType.OvercomeBy => SixRelation.Wealth,
				_ => SixRelation.None
			};
		}

		private string TranslateBeast(SixBeast beast) => beast switch
		{
			SixBeast.QingLong => "青龙",
			SixBeast.ZhuQue => "朱雀",
			SixBeast.GouChen => "勾陈",
			SixBeast.TengShe => "螣蛇",
			SixBeast.BaiHu => "白虎",
			SixBeast.XuanWu => "玄武",
			_ => "未知"
		};

		private string TranslateRelation(SixRelation relation) => relation switch
		{
			SixRelation.Brother => "兄弟",
			SixRelation.Offspring => "子孙",
			SixRelation.Wealth => "妻财",
			SixRelation.Officer => "官鬼",
			SixRelation.Parent => "父母",
			_ => "未知"
		};

		private Color GetBeastColor(SixBeast beast) => beast switch
		{
			SixBeast.QingLong => new Color("#81C784"), // Wood
			SixBeast.ZhuQue => new Color("#E57373"),   // Fire
			SixBeast.GouChen => new Color("#FFF176"),  // Earth
			SixBeast.TengShe => new Color("#FFB74D"),  // Earth/Fire
			SixBeast.BaiHu => new Color("#DFE1E5"),    // Metal
			SixBeast.XuanWu => new Color("#64B5F6"),   // Water
			_ => Colors.Gray
		};
	}
}
