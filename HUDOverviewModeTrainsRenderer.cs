using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HUDOverviewModeTrainsRenderer
{
	private struct CacheEntry
	{
		public readonly int InstancingId;

		public readonly MaterialPropertyBlock PropertyBlock;

		public CacheEntry(MetaShapeColor color)
		{
			InstancingId = Shader.PropertyToID("hud-overview-mode::trains::" + color.Code);
			PropertyBlock = new MaterialPropertyBlock();
			PropertyBlock.SetColor(MaterialPropertyHelpers.SHADER_ID_BaseColor, color.Color);
		}
	}

	private Dictionary<MetaShapeColor, CacheEntry> Cache = new Dictionary<MetaShapeColor, CacheEntry>();

	private CacheEntry GetRenderCacheEntry(MetaShapeColor color)
	{
		if (Cache.TryGetValue(color, out var entry))
		{
			return entry;
		}
		return Cache[color] = new CacheEntry(color);
	}

	public void Draw(FrameDrawOptions options, float alpha)
	{
		foreach (Train train in options.Player.CurrentMap.Trains.Trains)
		{
			DrawTrain(options, train, alpha);
		}
	}

	protected void DrawTrain(FrameDrawOptions options, Train train, float alpha)
	{
		float scale = 10f * alpha;
		CacheEntry entry = GetRenderCacheEntry(train.Color);
		float3 pos_W = train.CurrentPosition_W;
		options.Draw3DPlaneWithMaterialInstanced(entry.InstancingId, options.Theme.BaseResources.UXOverviewModeTrain, Matrix4x4.TRS(new float3(pos_W.x, 1f, pos_W.z), Quaternion.Euler(0f, train.CurrentAngle, 0f), Vector3.one * scale), entry.PropertyBlock);
	}
}
