using Drawing;
using UnityEngine;

public class DebugViewUpdateOrder : IDebugView
{
	public const string ID = "update-order";

	public string Name => "Update order";

	public void OnGameDraw()
	{
		int islandUpdateIndex = 0;
		foreach (Island island in Singleton<GameCore>.G.LocalPlayer.CurrentMap.IslandsInUpdateOrder)
		{
			CommandBuilder draw = DrawingManager.GetBuilder(renderInGame: true);
			draw.cameraTargets = new Camera[1] { Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera };
			draw.Label3D(island.Bounds_GC.ToCenter_W(2f), Quaternion.identity, "I" + islandUpdateIndex++, 1f, LabelAlignment.Center, new Color(1f, 1f, 1f));
			draw.Dispose();
			int buildingUpdateIndex = 0;
			foreach (MapEntity building in island.Buildings.BuildingsInUpdateOrder)
			{
				draw = DrawingManager.GetBuilder(renderInGame: true);
				draw.cameraTargets = new Camera[1] { Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera };
				draw.Label3D(building.Tile_I.To_W(island) + 0.9f * WorldDirection.Up, Quaternion.identity, "E" + buildingUpdateIndex++, 0.15f, LabelAlignment.Center, new Color(1f, 1f, 1f));
				draw.Dispose();
			}
		}
	}
}
