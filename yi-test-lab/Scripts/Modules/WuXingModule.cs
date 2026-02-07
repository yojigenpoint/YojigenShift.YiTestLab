using Godot;
using System;
using YojigenShift.YiFramework.Enums;

public partial class WuXingModule : Control
{
	[Export] public PackedScene CardPrefab;
	private HBoxContainer _grid;

	public override void _Ready()
	{
		_grid = GetNode<HBoxContainer>("VBoxContainer/Grid");

		GenerateCards();
	}

	public void RefreshLocalization()
	{
		foreach (Node child in _grid.GetChildren())
		{
			if (child is ElementCard card)
				card.RefreshUI();
		}
	}

	private void GenerateCards()
	{
		foreach (Node child in _grid.GetChildren()) child.QueueFree();

		foreach (WuXingType type in Enum.GetValues(typeof(WuXingType)))
		{
			if (type == WuXingType.None) continue;

			var card = CardPrefab.Instantiate<ElementCard>();
			_grid.AddChild(card);
			card.Setup(type);
		}
	}
}
