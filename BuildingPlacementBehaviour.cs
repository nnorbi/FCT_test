using System.Collections.Generic;
using Core.Events;

public abstract class BuildingPlacementBehaviour
{
	public struct PersistentPlacementData
	{
		public Grid.Direction Rotation;
	}

	public struct CtorData
	{
		public MetaBuildingVariant Variant;

		public PersistentPlacementData PersistentData;

		public Player Player;

		public IEventSender PassiveEventBus;
	}

	public enum UpdateResult
	{
		StayInPlacementMode,
		Stop
	}

	protected MetaBuildingVariant BuildingVariant;

	protected MouseTileTracker_G TileTracker_G;

	protected GameMap Map;

	protected Player Player;

	protected IEventSender PassiveEventBus;

	protected SimulationPredictionManager SimulationPrediction;

	protected short CurrentLayer => Player.Viewport.Layer;

	protected BuildingPlacementBehaviour(CtorData data)
	{
		Player = data.Player;
		TileTracker_G = new MouseTileTracker_G(Player);
		BuildingVariant = data.Variant;
		Map = Player.CurrentMap;
		PassiveEventBus = data.PassiveEventBus;
		SimulationPrediction = new SimulationPredictionManager(data.Player.CurrentMap);
	}

	public virtual IEnumerable<HUDSidePanelHotkeyInfoData> GetActions()
	{
		return new HUDSidePanelHotkeyInfoData[0];
	}

	public abstract PersistentPlacementData GetPersistentData();

	public abstract void RequestSpecificInternalVariant(MetaBuildingInternalVariant internalVariant);

	public virtual UpdateResult Update(InputDownstreamContext context, FrameDrawOptions drawOptions, HUDCursorInfo cursorInfo)
	{
		TileTracker_G.OnGameUpdate();
		return UpdateResult.StayInPlacementMode;
	}

	protected void OnPlacementSuccess()
	{
		SimulationPrediction.Invalidate();
	}
}
