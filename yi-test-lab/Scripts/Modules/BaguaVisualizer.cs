using Godot;
using System;
using System.Collections.Generic;
using YojigenShift.YiFramework.Enums;
using YojigenShift.YiFramework.Extensions;
using YojigenShift.YiTestLab.UI;

namespace YojigenShift.YiTestLab.Modules
{
	public partial class BaguaVisualizer : Control
	{
		public event Action<TrigramName> TrigramClicked;

		private BaguaSequence _currentSequence = BaguaSequence.EarlyHeaven;
		private float _radius = 220f;
		private float _yaoWidth = 100f; 
		private float _yaoHeight = 12f; 
		private float _yaoGap = 8f;     

		private struct TrigramNode
		{
			public TrigramName Name;
			public Vector2 Position;
			public float Radius;
		}
		private List<TrigramNode> _nodes = new List<TrigramNode>();

		// Index 0 = Top/South, Clockwise
		// 0:Top, 1:TR, 2:Right, 3:BR, 4:Bottom, 5:BL, 6:Left, 7:TL
		private readonly TrigramName[] _earlyHeavenOrder =
		{
			TrigramName.Qian, // Top (S)
			TrigramName.Xun,  // TR (SW) -> Wait, std map: Qian(S), Xun(SW), Kan(W), Gen(NW), Kun(N), Zhen(NE), Li(E), Dui(SE)
			
			// UI Clockwise (Top -> Right -> Bottom -> Left)
			TrigramName.Qian, // Top
			TrigramName.Xun,  // Top-Right
			TrigramName.Kan,  // Right
			TrigramName.Gen,  // Bottom-Right
			TrigramName.Kun,  // Bottom
			TrigramName.Zhen, // Bottom-Left
			TrigramName.Li,   // Left
			TrigramName.Dui   // Top-Left
		};

		private readonly TrigramName[] _laterHeavenOrder =
		{
			TrigramName.Li,   // Top
			TrigramName.Kun,  // Top-Right
			TrigramName.Dui,  // Right
			TrigramName.Qian, // Bottom-Right
			TrigramName.Kan,  // Bottom
			TrigramName.Gen,  // Bottom-Left
			TrigramName.Zhen, // Left
			TrigramName.Xun   // Top-Left
		};

		public override void _Ready()
		{
			CustomMinimumSize = new Vector2(600, 600);
		}

		public void SetSequence(BaguaSequence seq)
		{
			_currentSequence = seq;
			QueueRedraw();
		}

		public override void _GuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
			{
				foreach (var node in _nodes)
				{
					if (mb.Position.DistanceTo(node.Position) < 60f)
					{
						TrigramClicked?.Invoke(node.Name);
						return;
					}
				}
			}
		}

		public override void _Draw()
		{
			Vector2 center = Size / 2;
			TrigramName[] order = (_currentSequence == BaguaSequence.EarlyHeaven) ? _earlyHeavenOrder : _laterHeavenOrder;

			_nodes.Clear();

			// 1. Draw Tai Chi
			DrawCircle(center, _radius * 0.4f, GlobalUIController.ColorSurface.Lightened(0.05f));
			string centerText = _currentSequence == BaguaSequence.EarlyHeaven ? "先天\nEarly" : "后天\nLater";
			DrawString(ThemeDB.FallbackFont, center + new Vector2(-40, -15), centerText, HorizontalAlignment.Center, -1, 24, Colors.Gray);

			// 2. Draw Trigrams
			for (int i = 0; i < 8; i++)
			{
				// Calculate position: 0 index at -90 deg (Top)
				float angleDeg = i * 45 - 90;
				float angleRad = Mathf.DegToRad(angleDeg);

				Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
				Vector2 pos = center + dir * _radius;

				TrigramName trigram = order[i];

				_nodes.Add(new TrigramNode { Name = trigram, Position = pos, Radius = 60f });

				DrawTrigramSymbol(trigram, pos, angleRad + Mathf.Pi / 2);
			}
		}

		private void DrawTrigramSymbol(TrigramName name, Vector2 pos, float rotation)
		{
			Color color = GlobalUIController.GetElementColor(name.GetWuXing());

			DrawSetTransform(pos, rotation, Vector2.One);

			float startY = 20; // 离中心点的偏移

			for (int line = 1; line <= 3; line++)
			{
				bool isYang = name.GetLine(line);

				float y = startY - (line - 1) * (_yaoHeight + _yaoGap);
				y = -(line - 1) * (_yaoHeight + _yaoGap);

				Rect2 rect = new Rect2(-_yaoWidth / 2, y - _yaoHeight / 2, _yaoWidth, _yaoHeight);

				if (isYang)
				{
					DrawRect(rect, color, true);
				}
				else
				{
					float gap = 15f;
					float halfWidth = (_yaoWidth - gap) / 2;

					Rect2 leftRect = new Rect2(-_yaoWidth / 2, y - _yaoHeight / 2, halfWidth, _yaoHeight);
					Rect2 rightRect = new Rect2(gap / 2, y - _yaoHeight / 2, halfWidth, _yaoHeight);

					DrawRect(leftRect, color, true);
					DrawRect(rightRect, color, true);
				}
			}

			// Godot 4.0: DrawSetTransform affects subsequent calls. 
			// To draw text unrotated, we need to reset transform or calculate text pos manually outside.
			// Let's draw text manually outside this function or use inverse transform.

			DrawString(ThemeDB.FallbackFont, new Vector2(-20, -55), name.GetLocalizedName(), HorizontalAlignment.Center, -1, 24, color);

			DrawSetTransform(Vector2.Zero, 0, Vector2.One);
		}
	}
}
