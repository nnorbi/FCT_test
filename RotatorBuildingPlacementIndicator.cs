using Unity.Mathematics;

public abstract class RotatorBuildingPlacementIndicator : BuildingPlacementIndicator<MetaBuildingInternalVariant>
{
	protected override void DrawInternal(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate tile_I, GlobalTileCoordinate tile_G, Grid.Direction rotation, MetaBuildingInternalVariant internalVariant)
	{
		float3 tile_W = tile_I.To_W(island);
		float progress = drawOptions.AnimationSimulationTime_G;
		float animationRotation = internalVariant.GetCurve(0, progress % 1f) + (float)(int)progress;
		if (internalVariant.SupportMeshesInternalLOD[1].TryGet(0, out LODBaseMesh.CachedMesh mesh))
		{
			AnalogUI.DrawUIGeneralBuildingPlacementIndicatorMesh(drawOptions, mesh, FastMatrix.TranslateRotateDegrees(tile_W + 0.1f * WorldDirection.Up, animationRotation * GetRotationDegrees()));
		}
	}

	protected abstract float GetRotationDegrees();
}
