using Unity.Mathematics;
using UnityEngine;

public static class ScreenUtils
{
	private static Plane ChunkGridPlane = new Plane(Vector3.up, new float3(0f, -3f, 0f));

	private static Plane RailGridPlane = new Plane(Vector3.up, new float3(0f, TrainManager.RAIL_HEIGHT_W, 0f));

	public static Plane GetPlaneForTileLayer(float layer)
	{
		return new Plane(Vector3.up, new float3(0f, layer, 0f));
	}

	public static bool TryGetTileAtCursor(Player player, float layer, out GlobalTile tile)
	{
		return TryGetTile(player, layer, player.Viewport.CursorScreenPosition, out tile);
	}

	public static bool TryGetChunkCoordinateAtCursor(PlayerViewport viewport, out GlobalChunkCoordinate chunkCoordinate)
	{
		return TryGetChunkCoordinate(viewport, viewport.CursorScreenPosition, out chunkCoordinate);
	}

	public static bool TryGetRailCoordinateAtCursor(PlayerViewport playerViewport, out float2 railCoordinate)
	{
		return TryGetRailCoordinate(playerViewport, playerViewport.CursorScreenPosition, out railCoordinate);
	}

	public static bool TryGetTile(Player player, float layer, in float2 screenCoordinate, out GlobalTile tile)
	{
		if (!TryGetTileCoordinate(player.Viewport, layer, in screenCoordinate, out var tileCoordinate))
		{
			tile = default(GlobalTile);
			return false;
		}
		tile = player.CurrentMap.GetGlobalTileAt_G(in tileCoordinate);
		return true;
	}

	public static bool TryGetChunkCoordinate(PlayerViewport viewport, in float2 screenCoordinate, out GlobalChunkCoordinate chunkCoordinate)
	{
		Ray mouseRay = viewport.MainCamera.ScreenPointToRay(new float3(screenCoordinate, 0f));
		if (!ChunkGridPlane.Raycast(mouseRay, out var enter))
		{
			chunkCoordinate = GlobalChunkCoordinate.Origin;
			return false;
		}
		Vector3 worldCoordinate = mouseRay.GetPoint(enter);
		chunkCoordinate = worldCoordinate.To_G().To_GC();
		return true;
	}

	public static bool TryGetRailCoordinate(PlayerViewport playerViewport, in float2 screenCoordinate, out float2 railCoordinate)
	{
		Ray mouseRay = playerViewport.ScreenCoordinateToRay(screenCoordinate);
		if (!RailGridPlane.Raycast(mouseRay, out var enter))
		{
			railCoordinate = default(float2);
			return false;
		}
		Vector3 worldPosition = mouseRay.GetPoint(enter);
		railCoordinate = TrainRailNode.TG_From_W(worldPosition);
		return true;
	}

	public static bool TryGetTileCoordinate(PlayerViewport playerViewport, float layer, in float2 screenCoordinate, out GlobalTileCoordinate tile_G)
	{
		if (!TryGetWorldCoordinate(playerViewport, layer, screenCoordinate, out var worldCoordinate))
		{
			tile_G = default(GlobalTileCoordinate);
			return false;
		}
		tile_G = worldCoordinate.To_G();
		return true;
	}

	public static bool TryGetWorldCoordinate(PlayerViewport playerViewport, float layer, float2 screenCoordinate, out float3 worldCoordinate)
	{
		Plane plane = GetPlaneForTileLayer(layer);
		Ray mouseRay = playerViewport.ScreenCoordinateToRay(screenCoordinate);
		if (!plane.Raycast(mouseRay, out var enter))
		{
			worldCoordinate = default(Vector3);
			return false;
		}
		worldCoordinate = mouseRay.GetPoint(enter);
		worldCoordinate.y = layer;
		return true;
	}

	public static MapEntity FindEntityAtCursor(Player player, out GlobalTileCoordinate entityTileCoordinate)
	{
		PlayerViewport viewport = player.Viewport;
		short currentLayer = viewport.Layer;
		bool showUpperLayers = viewport.ShowAllLayers;
		short maxLayer = Singleton<GameCore>.G.Mode.MaxLayer;
		Plane upperPlane = new Plane(Vector3.up, new float3(0f, (float)maxLayer + 1f - 0.01f, 0f));
		Plane lowerPlane = new Plane(Vector3.up, new float3(0f, 0f, 0f));
		float2 screenCoordinate = viewport.CursorScreenPosition;
		Ray mouseRay = viewport.CursorRay;
		float rayStartProgress = 0f;
		float rayEndProgress = 0f;
		if (!upperPlane.Raycast(mouseRay, out rayStartProgress))
		{
			rayStartProgress = 0f;
		}
		if (!lowerPlane.Raycast(mouseRay, out rayEndProgress))
		{
			rayEndProgress = 0f;
		}
		int maxSteps = 250;
		float raymarchInterval = 0.1f;
		GameMap map = player.CurrentMap;
		GlobalTileCoordinate currentTile = new GlobalTileCoordinate(int.MaxValue, int.MaxValue, short.MaxValue);
		Bounds tmpBounds = default(Bounds);
		for (float progress = rayStartProgress; progress <= rayEndProgress; progress += raymarchInterval)
		{
			if (maxSteps-- <= 0)
			{
				break;
			}
			Vector3 point_W = mouseRay.GetPoint(progress);
			GlobalTileCoordinate tile_G = point_W.To_G();
			tile_G.z = (short)math.clamp((int)math.floor(tile_G.z), 0, maxLayer);
			if (tile_G == currentTile || tile_G.z < currentLayer || (tile_G.z > currentLayer && !showUpperLayers))
			{
				continue;
			}
			currentTile = tile_G;
			MapEntity building = map.GetEntityAt_G(in tile_G);
			if (building == null)
			{
				continue;
			}
			MetaBuildingInternalVariant.CollisionBox[] colliders = building.InternalVariant.Colliders;
			foreach (MetaBuildingInternalVariant.CollisionBox collider in colliders)
			{
				tmpBounds.center = building.W_From_L(in collider.Center_L);
				tmpBounds.size = collider.DimensionsByRotation_W[(int)building.Rotation_G];
				if (tmpBounds.IntersectRay(mouseRay))
				{
					entityTileCoordinate = tile_G;
					return building;
				}
			}
		}
		entityTileCoordinate = GlobalTileCoordinate.Origin;
		return null;
	}
}
