using Godot;
using System;
using YojigenShift.YiFramework.Core;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;

public partial class BaziModule : Control
{
	private SpinBox _year, _month, _day, _hour;
	private Button _btnPlot;

	private PillarView _pvYear, _pvMonth, _pvDay, _pvHour;
	private Label _lblStrength, _lblUsefulGod;

	public override void _Ready()
	{
		var inputPath = "VBoxContainer/InputArea/HBoxContainer/";
		_year = GetNode<SpinBox>(inputPath + "Input_Year");
		_month = GetNode<SpinBox>(inputPath + "Input_Month");
		_day = GetNode<SpinBox>(inputPath + "Input_Day");
		_hour = GetNode<SpinBox>(inputPath + "Input_Hour");
		_btnPlot = GetNode<Button>(inputPath + "Btn_Plot");

		var resultPath = "VBoxContainer/ResultArea/";
		_pvYear = GetNode<PillarView>(resultPath + "PillarView_Year");
		_pvMonth = GetNode<PillarView>(resultPath + "PillarView_Month");
		_pvDay = GetNode<PillarView>(resultPath + "PillarView_Day");
		_pvHour = GetNode<PillarView>(resultPath + "PillarView_Hour");

		var analysisPath = "VBoxContainer/AnalysisArea/VBoxContainer/";
		_lblStrength = GetNode<Label>(analysisPath + "Label_Strength");
		_lblUsefulGod = GetNode<Label>(analysisPath + "Label_UsefulGod");

		_btnPlot.Pressed += OnPlotClicked;

		var now = DateTime.Now;
		_year.Value = now.Year;
		_month.Value = now.Month;
		_day.Value = now.Day;
		_hour.Value = now.Hour;
	}

	private void OnPlotClicked()
	{
		try
		{
			DateTime inputTime = new DateTime(
				(int)_year.Value,
				(int)_month.Value,
				(int)_day.Value,
				(int)_hour.Value,
				0, 0, DateTimeKind.Local);

			DateTime utcTime = inputTime.ToUniversalTime();

			var bazi = BaziPlotter.Plot(utcTime);

			var result = BaziEvaluator.Evaluate((int)_year.Value, (int)_month.Value, (int)_day.Value, (int)_hour.Value);

			// 3. 更新 UI
			// UpdateUI(bazi, result);

			_pvYear.Setup("YEAR_PILLAR", bazi.YearIdx);
            _pvYear.Setup("MONTH_PILLAR", bazi.MonthIdx);
            _pvYear.Setup("DAY_PILLAR", bazi.DayIdx, GanZhiMath.GetStem(bazi.DayIdx));
            _pvYear.Setup("TIME_PILLAR", bazi.HourIdx);

			string strength = result.IsDayMasterStrong ? "STRONG" : "WEAK";
			string usefulGods = "";

			foreach (var type in result.UsefulGods)
				usefulGods += type.GetLocalizedName() + ", ";

			_lblStrength.Text = strength;
			_lblUsefulGod.Text = $"Useful God: " + usefulGods;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"排盘错误: {ex.Message}");
		}
	}

	public void RefreshLocalization()
	{
		OnPlotClicked();
	}
}
