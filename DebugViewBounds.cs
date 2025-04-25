using Drawing;
using UnityEngine;

public class DebugViewBounds : IDebugView
{
	public const string ID = "bounds";

	public string Name => "Bounds";

	public void OnGameDraw()
	{
		CommandBuilder draw = DrawingManager.GetBuilder(renderInGame: true);
		draw.cameraTargets = new Camera[1] { Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera };
		foreach (Island island in Singleton<GameCore>.G.LocalPlayer.CurrentMap.Islands)
		{
			draw.WireBox(island.Bounds_W.center, island.Bounds_W.size, new Color(1f, 0f, 1f, 1f));
			foreach (IslandChunk chunk in island.Chunks)
			{
				draw.WireBox(chunk.Bounds_W.center, chunk.Bounds_W.size, new Color(0f, 1f, 1f, 1f));
			}
		}
		draw.Dispose();
	}
}
