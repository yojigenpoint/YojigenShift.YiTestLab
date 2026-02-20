using Godot;
using YojigenShift.YiFramework.Core;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiFramework.QiMen.Models;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules.Components
{
	public partial class QimenCell : PanelContainer
	{
		// UI 引用
		private Label _lblGod;
		private Label _lblHeavenStem;
		private Label _lblStar;
		private Label _lblEarthStem;
		private Label _lblPalaceNum; // 左下角宫位图
		private Label _lblDoor;
		private Label _lblHiddenStem;

		// 角落标记
		private Label _lblHorse; // 马星
		private Label _lblVoid;  // 空亡

		// 状态标记 (四害)
		private ColorRect _statusIndicator; // 或者是文字颜色变化

		public override void _Ready()
		{
			CustomMinimumSize = new Vector2(300, 300); // 宫位大一点

			// 样式
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
			// 使用 MarginContainer 留出边距
			var margin = new MarginContainer();
			margin.AddThemeConstantOverride("margin_left", 10);
			margin.AddThemeConstantOverride("margin_right", 10);
			margin.AddThemeConstantOverride("margin_top", 10);
			margin.AddThemeConstantOverride("margin_bottom", 10);
			AddChild(margin);

			var vBox = new VBoxContainer();
			vBox.SizeFlagsVertical = SizeFlags.ExpandFill;
			margin.AddChild(vBox);

			// --- Top Row: God ---
			var topCenter = new CenterContainer();
			_lblGod = CreateLabel(24, Colors.LightGray);
			topCenter.AddChild(_lblGod);
			vBox.AddChild(topCenter);

			vBox.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });

			// --- Middle Row: HeavenStem | Star | EarthStem ---
			var midRow = new HBoxContainer();
			midRow.Alignment = BoxContainer.AlignmentMode.Center;
			midRow.AddThemeConstantOverride("separation", 20); // 间距

			_lblHeavenStem = CreateLabel(32, GlobalUIController.ColorTextPrimary);
			_lblStar = CreateLabel(28, GlobalUIController.ColorTextPrimary);
			_lblEarthStem = CreateLabel(32, GlobalUIController.ColorTextPrimary);

			midRow.AddChild(_lblHeavenStem);
			midRow.AddChild(_lblStar);
			midRow.AddChild(_lblEarthStem);
			vBox.AddChild(midRow);

			vBox.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });

			// --- Bottom Row: PalaceNum | Door | HiddenStem ---
			var botRow = new HBoxContainer();

			_lblPalaceNum = CreateLabel(36, Colors.Gray); // 宫位数字大一点，做底纹感觉

			// Door 和 Hidden 放中间和右边
			var botRight = new HBoxContainer();
			botRight.AddThemeConstantOverride("separation", 20);
			_lblDoor = CreateLabel(28, GlobalUIController.ColorTextPrimary);
			_lblHiddenStem = CreateLabel(24, Colors.Gray);

			botRight.AddChild(_lblDoor);
			botRight.AddChild(_lblHiddenStem);

			botRow.AddChild(_lblPalaceNum);
			botRow.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
			botRow.AddChild(botRight);
			vBox.AddChild(botRow);

			// --- Overlays (马/空) ---
			// 使用 Control 节点绝对定位
			var overlay = new Control();
			overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
			overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
			margin.AddChild(overlay);

			_lblHorse = new Label { Text = "马", Visible = false };
			_lblHorse.AddThemeColorOverride("font_color", Colors.Yellow);
			_lblHorse.SetPosition(new Vector2(0, 0)); // 左上
			overlay.AddChild(_lblHorse);

			_lblVoid = new Label { Text = "○", Visible = false };
			_lblVoid.AddThemeColorOverride("font_color", Colors.Gray);
			_lblVoid.AddThemeFontSizeOverride("font_size", 24);
			_lblVoid.SetAnchorsAndOffsetsPreset(LayoutPreset.TopRight); // 右上
																		// 注意：纯代码设置 Anchor 有时需要手动计算 Pos，这里简单处理
			_lblVoid.SetPosition(new Vector2(250, 0));
			overlay.AddChild(_lblVoid);
		}

		public void SetData(QiMenPalace p, bool isHorse, bool isVoid)
		{
			if (p == null) return;

			// 1. 设置文本
			_lblGod.Text = p.God.ToString();
			_lblHeavenStem.Text = p.HeavenPlateStem.ToString();
			_lblEarthStem.Text = p.EarthPlateStem.ToString();
			_lblStar.Text = p.Star.ToString();
			_lblDoor.Text = p.Door.ToString();
			_lblHiddenStem.Text = p.HiddenStem.ToString();

			// 宫位显示：可以是数字(4) 或者 地支(巽)
			_lblPalaceNum.Text = p.Index.ToString();

			// 2. 设置马/空
			_lblHorse.Visible = isHorse;
			_lblVoid.Visible = isVoid;

			// 3. 计算四害并着色 (核心逻辑)
			AnalyzeFourHarms(p);
		}

		/// <summary>
		/// 计算四害 (门迫、击刑、入墓) 并调整颜色
		/// </summary>
		private void AnalyzeFourHarms(QiMenPalace p)
		{
			// 默认颜色
			_lblDoor.Modulate = Colors.White;
			_lblHeavenStem.Modulate = Colors.White;

			// --- 1. 门迫 (Door Compulsion) ---
			// 定义：门克宫 (Door controls Palace)
			// 宫位五行
			WuXingType palaceWuXing = GetPalaceWuXing(p.Index);
			WuXingType doorWuXing = p.Door.GetWuXing();

			if (doorWuXing.IsOvercomes(palaceWuXing))
			{
				// 门迫 -> 红色
				_lblDoor.Modulate = new Color("#FF5252");
			}

			// --- 2. 天盘入墓 (Tomb) ---
			// 天盘干在宫位处于"墓"地
			// 宫位对应的地支（简化版：取宫位的主气地支）
			EarthlyBranch palaceBranch = GetPalaceBranch(p.Index);
			// 查长生表 (需要引用 BaziHelpers)
			// 注意：需要确保 BaziHelpers 能够访问。如果在 Core 命名空间下应该没问题。
			// 这里假设 BaziHelpers 可用。
			// 只有当 Core 中实现了 GetLifeStage 扩展方法

			// 简单模拟检查:
			// 水土墓在辰(4), 木墓在未(7), 火墓在戌(10), 金墓在丑(1)
			// 巽4宫(辰/巳), 坤2宫(未/申)... 这种映射比较模糊
			// 我们用后天八卦的标准地支映射

			bool isTomb = CheckIsTomb(p.HeavenPlateStem, p.Index);

			// --- 3. 六仪击刑 (Punishment) ---
			// 戊-3(震), 己-2(坤), 庚-8(艮), 辛-9(离), 壬-4(巽), 癸-4(巽)
			bool isPunish = CheckIsPunish(p.HeavenPlateStem, p.Index);

			// 综合着色
			if (isPunish && isTomb)
			{
				// 刑+墓 -> 蓝色
				_lblHeavenStem.Modulate = new Color("#2196F3");
			}
			else if (isPunish)
			{
				// 击刑 -> 紫色
				_lblHeavenStem.Modulate = new Color("#AB47BC");
			}
			else if (isTomb)
			{
				// 入墓 -> 绿色
				_lblHeavenStem.Modulate = new Color("#66BB6A");
			}
		}

		// --- 辅助逻辑 (Helpers) ---

		private Label CreateLabel(int fontSize, Color color)
		{
			var l = new Label();
			l.AddThemeFontSizeOverride("font_size", fontSize);
			l.AddThemeColorOverride("font_color", color);
			l.HorizontalAlignment = HorizontalAlignment.Center;
			return l;
		}

		private WuXingType GetPalaceWuXing(int index)
		{
			// 坎1水, 坤2土, 震3木, 巽4木, 中5土, 乾6金, 兑7金, 艮8土, 离9火
			return index switch
			{
				1 => WuXingType.Water,
				9 => WuXingType.Fire,
				3 => WuXingType.Wood,
				4 => WuXingType.Wood,
				6 => WuXingType.Metal,
				7 => WuXingType.Metal,
				2 => WuXingType.Earth,
				5 => WuXingType.Earth,
				8 => WuXingType.Earth,
				_ => WuXingType.None
			};
		}

		private EarthlyBranch GetPalaceBranch(int index)
		{
			// 粗略映射宫位的主地支用于定长生
			return index switch
			{
				1 => EarthlyBranch.Zi,    // 坎
				8 => EarthlyBranch.Chou,  // 艮 (丑寅) -> 取墓库地支更准? 金墓在丑
				3 => EarthlyBranch.Mao,   // 震
				4 => EarthlyBranch.Chen,  // 巽 (辰巳) -> 水墓在辰
				9 => EarthlyBranch.Wu,    // 离
				2 => EarthlyBranch.Wei,   // 坤 (未申) -> 木墓在未
				7 => EarthlyBranch.You,   // 兑
				6 => EarthlyBranch.Xu,    // 乾 (戌亥) -> 火墓在戌
				_ => EarthlyBranch.Zi
			};
		}

		private bool CheckIsTomb(HeavenlyStem stem, int palaceIdx)
		{
			// 简易查表法
			// 水/土 墓在 辰(4宫)
			// 木 墓在 未(2宫)
			// 火 墓在 戌(6宫)
			// 金 墓在 丑(8宫)
			var wx = stem.GetWuXing();

			if ((wx == WuXingType.Water || wx == WuXingType.Earth) && palaceIdx == 4) return true;
			if (wx == WuXingType.Wood && palaceIdx == 2) return true;
			if (wx == WuXingType.Fire && palaceIdx == 6) return true;
			if (wx == WuXingType.Metal && palaceIdx == 8) return true;

			return false;
		}

		private bool CheckIsPunish(HeavenlyStem stem, int palaceIdx)
		{
			// 六仪击刑口诀：
			// 戊三(震3) 己二(坤2) 庚八(艮8) 辛九(离9) 壬癸四(巽4)
			return (stem, palaceIdx) switch
			{
				(HeavenlyStem.Wu, 3) => true,
				(HeavenlyStem.Ji, 2) => true,
				(HeavenlyStem.Geng, 8) => true,
				(HeavenlyStem.Xin, 9) => true,
				(HeavenlyStem.Ren, 4) => true,
				(HeavenlyStem.Gui, 4) => true,
				_ => false
			};
		}
	}
}
