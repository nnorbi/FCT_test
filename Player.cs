using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Events;

public class Player : IInputSourceProvider
{
	public class SerializedData
	{
		public PlayerViewport.SerializedData Viewport = new PlayerViewport.SerializedData();

		public Dictionary<string, int> StoredShapes = new Dictionary<string, int>();

		public PlayerWaypoints.SerializedData Waypoints = new PlayerWaypoints.SerializedData();

		public PlayerPins.SerializedData Pins = new PlayerPins.SerializedData();

		public TutorialStateSerializedData TutorialState = default(TutorialStateSerializedData);

		public PlayerWikiManager.SerializedData Wiki = default(PlayerWikiManager.SerializedData);

		public float TotalPlaytime = 0f;
	}

	public enum PlayerRole
	{
		LocalHost,
		Local,
		GameInternal
	}

	public const string DATA_FILENAME = "local-player.json";

	public GameMap CurrentMap;

	public UnityEvent<GameMap> MapChanged = new UnityEvent<GameMap>();

	public PlayerRole Role;

	public UnityEvent<GameInputModeType> InputModeChanged = new UnityEvent<GameInputModeType>();

	public float TotalPlaytime = 0f;

	public Dictionary<string, int> StoredShapes = new Dictionary<string, int>();

	public PlayerSelectionManager<MapEntity> BuildingSelection = new PlayerSelectionManager<MapEntity>();

	public PlayerSelectionManager<Island> IslandSelection = new PlayerSelectionManager<Island>();

	public Observable<IBlueprint> CurrentBlueprint = new Observable<IBlueprint>(null);

	public PlayerWaypoints Waypoints;

	public PlayerPins Pins;

	public Observable<MetaBuildingVariant> SelectedBuildingVariant = new Observable<MetaBuildingVariant>(null);

	public Observable<MetaIslandLayout> SelectedIslandLayout = new Observable<MetaIslandLayout>(null);

	public PlayerWikiManager WikiManager = new PlayerWikiManager();

	public ITutorialState TutorialState = new TutorialState();

	public BlueprintLibrary BlueprintLibrary = new BlueprintLibrary();

	public PlayerViewport Viewport { get; } = new PlayerViewport();

	public GameInputModeType InputMode { get; protected set; } = GameInputModeType.KeyboardMouse;

	public Player(PlayerRole role)
	{
		Role = role;
		Waypoints = new PlayerWaypoints(this);
		Pins = new PlayerPins();
	}

	public void ChangeInputMode(GameInputModeType mode)
	{
		InputMode = mode;
		InputModeChanged.Invoke(mode);
	}

	public void Serialize(SavegameBlobWriter writer)
	{
		writer.WriteObjectAsJson("local-player.json", new SerializedData
		{
			Viewport = Viewport.Serialize(),
			StoredShapes = StoredShapes,
			Waypoints = Waypoints.Serialize(),
			Pins = Pins.Serialize(),
			TotalPlaytime = TotalPlaytime,
			TutorialState = TutorialState.Serialize(),
			Wiki = WikiManager.Serialize()
		});
	}

	public void Deserialize(SavegameBlobReader reader)
	{
		SerializedData data = reader.ReadObjectFromJson<SerializedData>("local-player.json");
		TotalPlaytime = data.TotalPlaytime;
		Viewport.Deserialize(data.Viewport);
		Waypoints.Deserialize(data.Waypoints);
		Pins.Deserialize(data.Pins);
		StoredShapes = data.StoredShapes;
		TutorialState.Deserialize(data.TutorialState);
		BuildingSelection.Clear();
		IslandSelection.Clear();
		WikiManager.Deserialize(data.Wiki);
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("selection.info", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output($"{BuildingSelection.Count} buildings selected");
			int3 @int = int.MaxValue;
			int3 int2 = int.MinValue;
			foreach (MapEntity current in BuildingSelection.Selection)
			{
				GlobalTileCoordinate globalTileCoordinate = TileDirection.Zero.To_G(current);
				IslandTileCoordinate tile_I = current.Tile_I;
				ctx.Output($"{current.InternalVariant.Variant.Title} at ({globalTileCoordinate} | {tile_I})");
				@int = math.min(@int, (int3)globalTileCoordinate);
				int2 = math.max(int2, (int3)globalTileCoordinate);
			}
			int3 int3 = (@int + int2) / 2;
			ctx.Output($"Selected objects centered at {int3}. Min: {@int}. Max {int2}");
		});
	}
}
