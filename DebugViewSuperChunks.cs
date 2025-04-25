using System.Collections.Generic;
using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class DebugViewSuperChunks : IDebugView
{
	public const string ID = "super-chunks";

	public string Name => "Super Chunks";

	public void OnGameDraw()
	{
		CommandBuilder draw = DrawingManager.GetBuilder(renderInGame: true);
		draw.cameraTargets = new Camera[1] { Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera };
		foreach (KeyValuePair<SuperChunkCoordinate, MapSuperChunk> item in Singleton<GameCore>.G.LocalPlayer.CurrentMap.SuperChunkLookup_SC)
		{
			MapSuperChunk chunk = item.Value;
			Bounds bounds = chunk.Bounds_W;
			draw.WireBox(bounds.center, bounds.size * 0.995f, new Color(1f, 0f, 1f, 1f));
			float3 position = bounds.center;
			quaternion rotation = Quaternion.Euler(90f, 0f, 0f);
			SuperChunkCoordinate origin_SC = chunk.Origin_SC;
			string text = origin_SC.ToString();
			GlobalChunkCoordinate origin_GC = chunk.Origin_GC;
			draw.Label3D(position, rotation, " SC @ " + text + " / GC @ " + origin_GC.ToString(), 2f, new Color(1f, 1f, 0f, 1f));
			foreach (ShapeResourceSource resource in chunk.ShapeResources)
			{
				draw.WireBox(resource.Bounds_W.center, resource.Bounds_W.size * 0.995f, new Color(1f, 1f, 1f, 1f));
				GlobalChunkCoordinate[] tiles_GC = resource.Tiles_GC;
				for (int i = 0; i < tiles_GC.Length; i++)
				{
					GlobalChunkCoordinate tile_GC = tiles_GC[i];
					float3 tile_W = tile_GC.ToOrigin_W();
					draw.WireBox(tile_W, new Vector3(1f, 1f, 1f), new Color(0f, 1f, 1f, 1f));
					quaternion rotation2 = Quaternion.Euler(90f, 0f, 0f);
					origin_GC = tile_GC;
					draw.Label3D(tile_W, rotation2, " TILE @ " + origin_GC.ToString(), 1f, new Color(0f, 1f, 1f, 1f));
				}
			}
		}
		draw.Dispose();
	}
}
