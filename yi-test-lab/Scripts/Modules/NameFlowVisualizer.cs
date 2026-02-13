using Godot;
using System;
using System.Collections.Generic;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiFramework.Naming.Models;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class NameFlowVisualizer : Control
	{
		private List<CharAttributes> _chars = new List<CharAttributes>();
		private List<WuXingType> _usefulGods = new List<WuXingType>();

		private float _nodeRadius = 60f;
		private float _spacing = 200f;

		public void Visualize(List<CharAttributes> chars, List<WuXingType> usefulGods)
		{
			_chars = chars;
			_usefulGods = usefulGods;
			CustomMinimumSize = new Vector2(Math.Max(800, chars.Count * 250), 300);
			QueueRedraw();
		}

		public override void _Draw()
		{
			if (_chars == null || _chars.Count == 0) return;

			Vector2 startPos = new Vector2(150, Size.Y / 2);

			// 1. Draw line
			for (int i = 0; i < _chars.Count - 1; i++)
			{
				var curr = _chars[i];
				var next = _chars[i + 1];

				Vector2 p1 = startPos + new Vector2(i * _spacing, 0);
				Vector2 p2 = startPos + new Vector2((i + 1) * _spacing, 0);

				Vector2 dir = (p2 - p1).Normalized();
				Vector2 lineStart = p1 + dir * (_nodeRadius + 10);
				Vector2 lineEnd = p2 - dir * (_nodeRadius + 10);

				DrawRelationshipArrow(curr.MainWuXing, next.MainWuXing, lineStart, lineEnd);
			}

			// 2. Draw node
			for (int i = 0; i < _chars.Count; i++)
			{
				var c = _chars[i];
				Vector2 center = startPos + new Vector2(i * _spacing, 0);

				DrawCharNode(c, center, i == 0); // i==0 is surname
			}
		}

		private void DrawCharNode(CharAttributes c, Vector2 center, bool isSurname)
		{
			var color = GlobalUIController.GetElementColor(c.MainWuXing);

			// A. Draw background
			// Surname uses square, name uses circle
			if (isSurname)
				DrawRect(new Rect2(center - new Vector2(_nodeRadius, _nodeRadius), new Vector2(_nodeRadius * 2, _nodeRadius * 2)), color, true);
			else
				DrawCircle(center, _nodeRadius, color);

			// B. Border
			bool isUseful = _usefulGods.Contains(c.MainWuXing);
			if (isUseful)
			{
				DrawCircle(center, _nodeRadius + 5, Colors.Gold, false, 4.0f);
			}

			// C. Text
			var font = ThemeDB.FallbackFont;

			// Character
			DrawString(font, center + new Vector2(-20, 10), c.Character.ToString(), HorizontalAlignment.Center, -1, 40, Colors.Black);

			// Five Elements
			DrawString(font, center + new Vector2(-30, -_nodeRadius - 10), c.MainWuXing.GetLocalizedName(), HorizontalAlignment.Center, -1, 16, color);

			// Strokes
			string strokesText = $"{c.KangXiStrokes}画";
			DrawString(font, center + new Vector2(-20, _nodeRadius + 25), strokesText, HorizontalAlignment.Center, -1, 16, Colors.Gray);

			// Pin yin
			if (!string.IsNullOrEmpty(c.Pinyin))
			{
				DrawString(font, center + new Vector2(-20, _nodeRadius + 45), c.Pinyin, HorizontalAlignment.Center, -1, 14, Colors.Gray);
			}
		}

		private void DrawRelationshipArrow(WuXingType from, WuXingType to, Vector2 start, Vector2 end)
		{
			Color color = Colors.Gray;
			float width = 2.0f;
			string label = "";

			if (to.IsGeneratedBy(from)) // Generates
			{
				color = new Color("#00C853"); // Green
				width = 4.0f;
				label = "生";
			}
			else if (to.IsOvercomeBy(from)) // Overcomes
			{
				color = new Color("#FF5252"); // Red
				width = 2.0f;
				label = "克";
				// TODO: 绘制虚线 (Godot DrawLine 不直接支持虚线，这里简化为实线或自行分段绘制)
			}
			else if (from == to)
			{
				color = Colors.White;
				label = "同";
			}

			DrawLine(start, end, color, width, true);

			Vector2 dir = (end - start).Normalized();
			Vector2 arrowP1 = end - dir * 15 + dir.Rotated(Mathf.DegToRad(30)) * 10;
			Vector2 arrowP2 = end - dir * 15 + dir.Rotated(Mathf.DegToRad(-30)) * 10;
			Vector2[] arrowPoints = { end, arrowP1, arrowP2 };
			DrawColoredPolygon(arrowPoints, color);

			Vector2 mid = (start + end) / 2;
			DrawString(ThemeDB.FallbackFont, mid + new Vector2(-20, -10), label, HorizontalAlignment.Center, -1, 14, color);
		}
	}
}
