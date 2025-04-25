using UnityEngine;

public abstract class BuildingPlacementIndicator<TInternalVariant> : IBuildingPlacementIndicator where TInternalVariant : MetaBuildingInternalVariant
{
	public void Draw(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate tile_I, GlobalTileCoordinate tile_G, Grid.Direction rotation, MetaBuildingInternalVariant internalVariant)
	{
		if (!(internalVariant is TInternalVariant typedInternalVariant))
		{
			Debug.LogError($"Wrong argument type passed. Expected {typeof(TInternalVariant)} but received {internalVariant.GetType()}");
		}
		else
		{
			DrawInternal(drawOptions, island, tile_I, tile_G, rotation, typedInternalVariant);
		}
	}

	protected abstract void DrawInternal(FrameDrawOptions drawOptions, Island island, IslandTileCoordinate tile_I, GlobalTileCoordinate tile_G, Grid.Direction rotation, TInternalVariant internalVariant);
}
