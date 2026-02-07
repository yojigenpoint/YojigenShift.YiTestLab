using Godot;
using YojigenShift.YiFramework.Enums;

public partial class GanzhiModule : Control
{
	[Export] public PackedScene TilePrefab;

	private GridContainer _gridStems;
	private GridContainer _gridBranches;
	private GridContainer _gridJiaZi;

	public override void _Ready()
	{
		_gridStems = GetNode<GridContainer>("TabContainer/Tab_Stems/Grid");
		_gridBranches = GetNode<GridContainer>("TabContainer/Tab_Branches/Grid");
		_gridJiaZi = GetNode<GridContainer>("TabContainer/Tab_JiaZi/Grid");

		GenerateAll();
	}

	private void GenerateAll()
	{
		GenerateStems();
		GenerateBranches();
		GenerateJiaZi();
	}

	private void GenerateStems()
	{
		ClearGrid(_gridStems);

		for (int i = 0; i < 10; i++)
		{
			var tile = TilePrefab.Instantiate<CharacterTiles>();
			_gridStems.AddChild(tile);

			tile.Setup((HeavenlyStem)i);
		}
	}

	private void GenerateBranches()
	{
		ClearGrid(_gridBranches);

		for (int i = 0; i < 12; i++)
		{
			var tile = TilePrefab.Instantiate<CharacterTiles>();
			_gridBranches.AddChild(tile);

			tile.Setup((EarthlyBranch)i);
		}
	}

	private void GenerateJiaZi()
	{
		ClearGrid(_gridJiaZi);

		for (int i = 0; i < 60; i++)
		{
			var tile = TilePrefab.Instantiate<CharacterTiles>();
			_gridJiaZi.AddChild(tile);

			tile.Setup(i);
		}
	}

	private void ClearGrid(GridContainer grid)
	{
		foreach (Node child in grid.GetChildren()) child.QueueFree();
	}

	public void RefreshLocalization()
	{
		GenerateAll();
	}
}
