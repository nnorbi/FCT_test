using System.Collections.Generic;

public abstract class IslandPlacementBehaviour
{
	public struct PersistentPlacementData
	{
		public Grid.Direction Rotation;
	}

	public struct CtorData
	{
		public MetaIslandLayout Layout;

		public PersistentPlacementData PersistentData;

		public Player Player;
	}

	public enum UpdateResult
	{
		StayInPlacementMode,
		Stop
	}

	protected MetaIslandLayout Layout;

	protected MouseTileTracker_GC TileTracker_GC;

	protected GameMap Map;

	protected Player Player;

	protected IslandPlacementBehaviour(CtorData data)
	{
		Player = data.Player;
		TileTracker_GC = new MouseTileTracker_GC(Player);
		Layout = data.Layout;
		Map = Player.CurrentMap;
	}

	public virtual void Cleanup()
	{
	}

	public virtual IEnumerable<HUDSidePanelHotkeyInfoData> GetActions()
	{
		return new HUDSidePanelHotkeyInfoData[0];
	}

	public abstract PersistentPlacementData GetPersistentData();

	public virtual int GetChunkCost()
	{
		return Layout.ChunkCount;
	}

	public virtual UpdateResult Update(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		TileTracker_GC.OnGameUpdate();
		return UpdateResult.StayInPlacementMode;
	}
}
