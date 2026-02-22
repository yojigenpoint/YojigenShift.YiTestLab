using Godot;
using System;
using System.Collections.Generic;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiFramework.QiMen.Logic;
using YojigenShift.YiTestLab.Core;
using YojigenShift.YiTestLab.Modules.Components;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class QimenModule : VBoxContainer
	{
		private GlobalUIController _globalController;
		private DateTime _currentTime = DateTime.UtcNow;

		private Label _lblInfo;
		private GridContainer _grid;
		private List<QimenCell> _cells = new List<QimenCell>();

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
			var title = new Label { Text = Tr("MOD_QIMEN_TITLE"), HorizontalAlignment = HorizontalAlignment.Center };
			title.AddThemeFontSizeOverride("font_size", 40);
			AddChild(title);

			_lblInfo = new Label { HorizontalAlignment = HorizontalAlignment.Center };
			_lblInfo.AddThemeFontSizeOverride("font_size", 30);
			_lblInfo.AddThemeColorOverride("font_color", GlobalUIController.ColorAccent);
			AddChild(_lblInfo);

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
				var chart = QiMenPlotter.Plot(_currentTime);

				string dunStr = chart.Dun.GetLocalizedName();
				_lblInfo.Text = Helpers.GetLocalizedFormat("TXT_QIMEN_INFO",
					chart.Dun.GetLocalizedName(), chart.JuNumber, chart.XunLeader.GetLocalizedName(), 
					chart.ZhiFuStar.GetLocalizedName(), chart.ZhiShiDoor.GetLocalizedName());

				for (int i = 0; i < 9; i++)
				{
					int luoshuIdx = _gridToLuoshu[i];
					var palace = chart.Palaces[luoshuIdx];

					_cells[i].SetData(chart, palace);
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Qimen Error: {ex.Message}");
				_lblInfo.Text = "Error: Solar Term Calc missing?";
			}
		}
	}
}
