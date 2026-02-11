using Godot;
using System.Collections.Generic;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class WuXingVisualizer : Control
	{
		private float _radius = 180f;
		private float _nodeRadius = 40f;
		private WuXingType _activeElement = WuXingType.None;

		private readonly List<WuXingType> _cycleOrder = new List<WuXingType>
		{
			WuXingType.Wood,
			WuXingType.Fire,
			WuXingType.Earth,
			WuXingType.Metal,
			WuXingType.Water
		};

		private Dictionary<WuXingType, Vector2> _nodePositions = new Dictionary<WuXingType, Vector2>();

		public override void _Ready()
		{
			CustomMinimumSize = new Vector2(500, 500);
		}

		public void SetActiveElement(WuXingType type)
		{
			_activeElement = type;
			QueueRedraw();
		}

		public override void _Draw()
		{
			var center = Size / 2;

			// 1. Calculate positions
			_nodePositions.Clear();
			for (int i = 0; i < 5; i++)
			{
				// Start from -90 degress (top)，Every 72 degrees for one
				float angle = Mathf.DegToRad(i * 72 - 90);
				var pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _radius;
				_nodePositions[_cycleOrder[i]] = pos;
			}

			// 2. Draw the links
			DrawConnections();

			// 3. Draw the nodes and texts
			foreach (var type in _cycleOrder)
			{
				DrawNode(type);
			}
		}

		private void DrawConnections()
		{
			foreach (var source in _cycleOrder)
			{
				var startPos = _nodePositions[source];

				var child = source.Child();
				var endPosGen = _nodePositions[child];

				bool isGenHighlight = (_activeElement == source) || (_activeElement == child);
				var genColor = isGenHighlight ? GlobalUIController.GetElementColor(source) : new Color(1, 1, 1, 0.1f);
				float genWidth = isGenHighlight ? 6.0f : 2.0f;

				DrawLine(startPos, endPosGen, genColor, genWidth, true);

				var prisoner = source.Prisoner();
				var endPosOver = _nodePositions[prisoner];

				bool isOverHighlight = (_activeElement == source) || (_activeElement == prisoner);
				var overColor = isOverHighlight ? GlobalUIController.ColorOutline.Lightened(0.5f) : new Color(1, 1, 1, 0.05f);

				if (_activeElement == source) overColor = new Color("#E57373");

				DrawLine(startPos, endPosOver, overColor, isOverHighlight ? 3.0f : 1.0f, true);
			}
		}

		private void DrawNode(WuXingType type)
		{
			var pos = _nodePositions[type];
			var color = GlobalUIController.GetElementColor(type);

			if (_activeElement != WuXingType.None && _activeElement != type)
			{
				color = color.Darkened(0.4f);
			}

			DrawCircle(pos, _nodeRadius, color);

			if (_activeElement == type)
			{
				DrawArc(pos, _nodeRadius + 8, 0, Mathf.Tau, 32, Colors.White, 3.0f, true);
			}

			var font = ThemeDB.FallbackFont;
			var fontSize = 32;
			var text = type.GetLocalizedName(); 
			var textSize = font.GetStringSize(text, HorizontalAlignment.Center, -1, fontSize);

			DrawString(font, pos + new Vector2(-textSize.X / 2, textSize.Y / 3), text, HorizontalAlignment.Center, -1, fontSize, Colors.Black); // 字用黑色，在亮色背景上看清楚
		}
	}
}
