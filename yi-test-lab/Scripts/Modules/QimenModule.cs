using Godot;
using System;
using System.Collections.Generic;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.QiMen.Logic;
using YojigenShift.YiTestLab.Modules.Components;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class QimenModule : VBoxContainer
	{
		private GlobalUIController _globalController;
		private DateTime _currentTime = DateTime.UtcNow;

		// UI
		private Label _lblInfo;
		private GridContainer _grid;
		private List<QimenCell> _cells = new List<QimenCell>();

		// 3x3 Grid
		// Row 1: 4(SE), 9(S), 2(SW)
		// Row 2: 3(E),  5(C), 7(W)
		// Row 3: 8(NE), 1(N), 6(NW)
		private readonly int[] _gridToLuoshu = { 4, 9, 2, 3, 5, 7, 8, 1, 6 };

		public override void _Ready()
		{
			Name = "QimenModule";
			AddThemeConstantOverride("separation", 20);

			_globalController = GetNodeOrNull<Control>("/root/MainUI") as GlobalUIController;

			SetupUI();

			if (_globalController != null)
			{
				_currentTime = _globalController.CurrentTime;
				_globalController.GlobalTimeChanged += OnTimeChanged;
			}

			RefreshChart();
		}

		public override void _ExitTree()
		{
			if (_globalController != null) _globalController.GlobalTimeChanged -= OnTimeChanged;
		}

		private void OnTimeChanged(DateTime time)
		{
			_currentTime = time;
			RefreshChart();
		}

		private void SetupUI()
		{
			// 1. Title
			var title = new Label { Text = "Qi Men Dun Jia (奇门遁甲)", HorizontalAlignment = HorizontalAlignment.Center };
			title.AddThemeFontSizeOverride("font_size", 40);
			AddChild(title);

			// 2. Info
			_lblInfo = new Label { HorizontalAlignment = HorizontalAlignment.Center };
			_lblInfo.AddThemeColorOverride("font_color", GlobalUIController.ColorAccent);
			AddChild(_lblInfo);

			// 3. Nine Grid
			var center = new CenterContainer();
			
			center.SizeFlagsVertical = SizeFlags.ExpandFill;
			AddChild(center);

			_grid = new GridContainer { Columns = 3 };
			_grid.AddThemeConstantOverride("h_separation", 5);
			_grid.AddThemeConstantOverride("v_separation", 5);
			center.AddChild(_grid);

			for (int i = 0; i < 9; i++)
			{
				var cell = new QimenCell();
				_grid.AddChild(cell);
				_cells.Add(cell);
			}
		}

		private void RefreshChart()
		{
			try
			{
				// 1. Plotting
				var chart = QiMenPlotter.Plot(_currentTime);

				// 2. Update Info
				string dunStr = chart.Dun == DunType.Yang ? "Yang" : "Yin";
				_lblInfo.Text = $"{dunStr} Dun Ju {chart.JuNumber} | Leader: {chart.XunLeader} | ZhiFu: {chart.ZhiFuStar} | ZhiShi: {chart.ZhiShiDoor}";

				// 3. Calculation
				EarthlyBranch horseBranch = CalculateHorse(chart.HourBranch);
				List<EarthlyBranch> voidBranches = CalculateVoid(chart.XunLeader);

				// 4. Filling
				for (int i = 0; i < 9; i++)
				{
					int luoshuIdx = _gridToLuoshu[i];
					var palace = chart.Palaces[luoshuIdx];

					bool isHorse = CheckBranchInPalace(horseBranch, luoshuIdx);
					bool isVoid = false;
					foreach (var vb in voidBranches)
						if (CheckBranchInPalace(vb, luoshuIdx)) isVoid = true;

					_cells[i].SetData(palace, isHorse, isVoid);
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Qimen Error: {ex.Message}");
				_lblInfo.Text = "Error: Solar Term Calc missing?";
			}
		}

		// --- Helpers ---

		private EarthlyBranch CalculateHorse(EarthlyBranch timeBranch)
		{
			// 申子辰马在寅, 寅午戌马在申, 亥卯未马在巳, 巳酉丑马在亥
			int idx = (int)timeBranch;
			if (idx == 8 || idx == 0 || idx == 4) return EarthlyBranch.Yin; // 申子辰 -> 寅
			if (idx == 2 || idx == 6 || idx == 10) return EarthlyBranch.Shen; // 寅午戌 -> 申
			if (idx == 11 || idx == 3 || idx == 7) return EarthlyBranch.Si; // 亥卯未 -> 巳
			return EarthlyBranch.Hai; // 巳酉丑 -> 亥
		}

		private List<EarthlyBranch> CalculateVoid(HeavenlyStem xunLeader)
		{
			// 旬空：甲子旬戌亥空...
			// 旬首: 戊(甲子)->戌亥, 己(甲戌)->申酉, 庚(甲申)->午未, 辛(甲午)->辰巳, 壬(甲辰)->寅卯, 癸(甲寅)->子丑
			// 这里的 XunLeader 实际上是排盘里的“旬首干”。
			// 我们需要知道它是甲子、甲戌还是什么？
			// 实际上 chart.XunLeader 存的是六仪 (戊己庚...)。
			// 戊=甲子, 己=甲戌, 庚=甲申, 辛=甲午, 壬=甲辰, 癸=甲寅

			return xunLeader switch
			{
				HeavenlyStem.Wu => new List<EarthlyBranch> { EarthlyBranch.Xu, EarthlyBranch.Hai },
				HeavenlyStem.Ji => new List<EarthlyBranch> { EarthlyBranch.Shen, EarthlyBranch.You },
				HeavenlyStem.Geng => new List<EarthlyBranch> { EarthlyBranch.Wu, EarthlyBranch.Wei },
				HeavenlyStem.Xin => new List<EarthlyBranch> { EarthlyBranch.Chen, EarthlyBranch.Si },
				HeavenlyStem.Ren => new List<EarthlyBranch> { EarthlyBranch.Yin, EarthlyBranch.Mao },
				HeavenlyStem.Gui => new List<EarthlyBranch> { EarthlyBranch.Zi, EarthlyBranch.Chou },
				_ => new List<EarthlyBranch>()
			};
		}

		private bool CheckBranchInPalace(EarthlyBranch b, int palaceIdx)
		{
			return (b, palaceIdx) switch
			{
				(EarthlyBranch.Zi, 1) => true,
				(EarthlyBranch.Chou, 8) => true,
				(EarthlyBranch.Yin, 8) => true,
				(EarthlyBranch.Mao, 3) => true,
				(EarthlyBranch.Chen, 4) => true,
				(EarthlyBranch.Si, 4) => true,
				(EarthlyBranch.Wu, 9) => true,
				(EarthlyBranch.Wei, 2) => true,
				(EarthlyBranch.Shen, 2) => true,
				(EarthlyBranch.You, 7) => true,
				(EarthlyBranch.Xu, 6) => true,
				(EarthlyBranch.Hai, 6) => true,
				_ => false
			};
		}
	}
}
