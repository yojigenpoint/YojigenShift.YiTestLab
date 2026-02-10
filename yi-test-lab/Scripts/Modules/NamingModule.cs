using Godot;
using System;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Naming;
using YojigenShift.YiFramework.Naming.Data;
using YojigenShift.YiFramework.Naming.Models;

public partial class NamingModule : Control
{
	[Export] public PackedScene NameCardPrefab;

	private Control _contentArea;
	private Control _warningArea;

	private LineEdit _inputSurname;
	private OptionButton _optLength;
	private OptionButton _optGender;
	private Button _btnGenerate;
	private VBoxContainer _resultList;

	private HanNamingService _service;

	public override void _Ready()
	{
		_contentArea = GetNode<Control>("ContentArea");
		_warningArea = GetNode<Control>("WarningArea");

		_inputSurname = GetNode<LineEdit>("ContentArea/InputArea/HBoxContainer/Input_Surname");
		_optLength = GetNode<OptionButton>("ContentArea/InputArea/HBoxContainer/Input_Length");
		_optGender = GetNode<OptionButton>("ContentArea/InputArea/HBoxContainer/Input_Gender");
		_btnGenerate = GetNode<Button>("ContentArea/InputArea/HBoxContainer/Btn_Generate");
		_resultList = GetNode<VBoxContainer>("ContentArea/ResultArea/ResultList");

		_optGender.AddItem("男 (Male)", 0);
		_optGender.AddItem("女 (Female)", 1);

		_btnGenerate.Pressed += OnGeneratePressed;

		InitializeService();

		RefreshLocalization();
	}

	private void InitializeService()
	{
		try
		{
			_service = new HanNamingService(new DemoCharProvider());
		}
		catch (Exception e)
		{
			GD.PrintErr("Naming Service Init Failed: " + e.Message);
		}
	}

	private void OnGeneratePressed()
	{
		string surname = _inputSurname.Text.Trim();
		if (string.IsNullOrEmpty(surname))
		{
			OS.Alert("请输入姓氏", "提示");
			return;
		}

		foreach (Node child in _resultList.GetChildren()) child.QueueFree();

		var request = new NamingRequest
		{
			Surname = surname,
			NameLength = _optLength.Text.Length,
			Gender = (GenderPreference)_optGender.Selected,
			SurnameWuXing = WuXingType.Metal,
			BirthTimeUtc = DateTime.Now,
			MaxResults = 50
		};

		var results = _service.RecommendNames(request);

		foreach (var res in results)
		{
			var card = NameCardPrefab.Instantiate<NameCard>();
			_resultList.AddChild(card);
			card.Setup(res);
		}
	}

	public void RefreshLocalization()
	{
		// 检查当前语言
		string currentLang = TranslationServer.GetLocale(); // "zh" or "en"

		bool isChinese = currentLang.StartsWith("zh");

		// 控制显示隐藏
		_contentArea.Visible = isChinese;
		_warningArea.Visible = !isChinese;
	}
}
