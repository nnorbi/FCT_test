using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HUDOverviewModeRailsRenderer
{
	private struct CacheEntry
	{
		public readonly int InstancingId;

		public readonly MaterialPropertyBlock PropertyBlock;

		public CacheEntry(TrainSubPath.TrainPathType pathType, MetaShapeColor color)
		{
			InstancingId = Shader.PropertyToID("hud-overview-rails-renderer::" + color.Code + "//" + pathType);
			PropertyBlock = new MaterialPropertyBlock();
			PropertyBlock.SetColor(MaterialPropertyHelpers.SHADER_ID_BaseColor, color.Color);
		}
	}

	private Dictionary<(TrainSubPath.TrainPathType, MetaShapeColor), CacheEntry> Cache = new Dictionary<(TrainSubPath.TrainPathType, MetaShapeColor), CacheEntry>();

	private CacheEntry GetRenderCacheEntry((TrainSubPath.TrainPathType, MetaShapeColor) key)
	{
		if (Cache.TryGetValue(key, out var entry))
		{
			return entry;
		}
		return Cache[key] = new CacheEntry(key.Item1, key.Item2);
	}

	public void Draw(FrameDrawOptions options, float alpha)
	{
		VisualThemeBaseResources resources = options.Theme.BaseResources;
		float3 offset_W = new float3(0f, 0f, 0f);
		Material materialForward = resources.UXOverviewModeRailForwardMaterial;
		Material materialRight = resources.UXOverviewModeRailRightMaterial;
		Material materialLeft = resources.UXOverviewModeRailLeftMaterial;
		foreach (KeyValuePair<int2, TrainRailNode> entry in options.Player.CurrentMap.Trains.RailLookup_TG)
		{
			if (!GeometryUtility.TestPlanesAABB(options.CameraPlanes, entry.Value.Bounds))
			{
				continue;
			}
			TrainRailNode railNode = entry.Value;
			foreach (TrainSubPath connection in railNode.Connections)
			{
				if (!railNode.HasAuthorityOver(connection.To))
				{
					continue;
				}
				TrainSubPath.PathDescriptor descriptor = connection.Descriptor;
				TrainSubPath.TrainPathType pathType = descriptor.Type;
				Grid.Direction pathDirection = descriptor.Direction;
				int2 from_TG = connection.From.Position_TG;
				int2 to_TG = connection.To.Position_TG;
				foreach (MetaShapeColor color in connection.Colors)
				{
					CacheEntry cacheEntry = GetRenderCacheEntry((pathType, color));
					float3 position_W;
					Material material;
					switch (pathType)
					{
					case TrainSubPath.TrainPathType.Forward:
						position_W = TrainRailNode.W_From_TG((float2)(from_TG + to_TG) / 2f);
						material = materialForward;
						break;
					case TrainSubPath.TrainPathType.TurnLeft:
						position_W = TrainRailNode.W_From_TG(from_TG + Grid.Rotate(new int2(1, 0), pathDirection));
						material = materialLeft;
						pathDirection = Grid.OppositeDirection(pathDirection);
						break;
					case TrainSubPath.TrainPathType.TurnRight:
						position_W = TrainRailNode.W_From_TG(from_TG + Grid.Rotate(new int2(1, 0), pathDirection));
						material = materialRight;
						pathDirection = Grid.OppositeDirection(pathDirection);
						break;
					default:
						throw new Exception("Bad path type");
					}
					options.Draw3DPlaneWithMaterialInstanced(cacheEntry.InstancingId, material, Matrix4x4.TRS(position_W + offset_W, s: Vector3.one * TrainManager.RAIL_GRID_SIZE_G * 2f * alpha, q: FastMatrix.RotateY(pathDirection)), cacheEntry.PropertyBlock);
				}
			}
		}
	}
}
