using Godot;
using System;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules.Components
{
	public partial class HexagramVisualizer : Control
	{
		private YaoType[] _lines = new YaoType[6];
		private bool _hasData = false;

		private float _lineHeight = 16f;
		private float _lineWidth = 120f;
		private float _lineGap = 12f; 
		private float _verticalSpacing = 28f;

		public override void _Ready()
		{
			CustomMinimumSize = new Vector2(180, 200);
		}

		/// <summary>
		/// Pass 6 YaoType to draw the hexagram. The array should be ordered from bottom (index 0) to top (index 5).
		/// </summary>
		public void SetLines(YaoType[] lines)
		{
			if (lines == null || lines.Length != 6) return;
			Array.Copy(lines, _lines, 6);
			_hasData = true;
			QueueRedraw();
		}

		/// <summary>
		/// Simplified version that accepts a Hexagram struct and extracts the line information. It will convert dynamic lines to their static counterparts for visualization.
		/// </summary>
		public void SetHexagram(YojigenShift.YiFramework.Structs.Hexagram hex)
		{
			for (int i = 0; i < 6; i++)
			{
				_lines[i] = hex.GetLine(i + 1) == Polarity.Yang ? YaoType.YoungYang : YaoType.YoungYin;
			}
			_hasData = true;
			QueueRedraw();
		}

		public override void _Draw()
		{
			if (!_hasData) return;

			Vector2 center = Size / 2;

			float totalHeight = 5 * _verticalSpacing + _lineHeight;
			float startY = center.Y + totalHeight / 2 - _lineHeight / 2;

			for (int i = 0; i < 6; i++)
			{
				YaoType yao = _lines[i];
				float y = startY - i * _verticalSpacing;

				bool isYang = (yao == YaoType.YoungYang || yao == YaoType.OldYang);
				bool isMoving = (yao == YaoType.OldYang || yao == YaoType.OldYin);

				Color lineColor = isMoving ? GlobalUIController.ColorAccent : Colors.LightGray;

				// 1. Draw the line (solid for Yang, split for Yin)
				if (isYang)
				{
					// Yang Yao: a single solid line
					Rect2 rect = new Rect2(center.X - _lineWidth / 2, y - _lineHeight / 2, _lineWidth, _lineHeight);
					DrawRect(rect, lineColor, true);
				}
				else
				{
					// Yin Yao: two shorter lines with a gap in the middle
					float halfWidth = (_lineWidth - _lineGap) / 2;

					Rect2 leftRect = new Rect2(center.X - _lineWidth / 2, y - _lineHeight / 2, halfWidth, _lineHeight);
					Rect2 rightRect = new Rect2(center.X + _lineGap / 2, y - _lineHeight / 2, halfWidth, _lineHeight);

					DrawRect(leftRect, lineColor, true);
					DrawRect(rightRect, lineColor, true);
				}

				// 2. Draw changing Yao (O or X)
				if (isMoving)
				{
					string symbol = yao == YaoType.OldYang ? "O" : "X";
					Vector2 textPos = new Vector2(center.X + _lineWidth / 2 + 15, y + 6);
					DrawString(ThemeDB.FallbackFont, textPos, symbol, HorizontalAlignment.Left, -1, 20, GlobalUIController.ColorAccent);
				}
			}
		}
	}
}
