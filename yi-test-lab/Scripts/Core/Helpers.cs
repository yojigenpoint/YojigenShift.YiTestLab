using Godot;
using YojigenShift.YiFramework.Enums;

public static class Helpers
{    
	public static Color GetColorForWuXing(WuXingType type)
	{
		switch (type)
		{
			case WuXingType.Wood:
				return Colors.ForestGreen;
			case WuXingType.Fire:
				return Colors.OrangeRed;
			case WuXingType.Earth:
				return Colors.SandyBrown;
			case WuXingType.Metal:
				return Colors.Silver;
			case WuXingType.Water:
				return Colors.DeepSkyBlue;
			case WuXingType.None:
			default:
				return Colors.Red;
		}
	}
}
