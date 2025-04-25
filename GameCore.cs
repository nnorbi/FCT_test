using System;
using System.Collections;
using Core.Dependency;
using Core.Events;
using Core.Logging;
using DG.Tweening;
using Unity.Core.Logging;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-35)]
public class GameCore : Singleton<GameCore>, IDisposable
{
	[Space(30f)]
	public HUDCrashOverlay UICrashOverlayPrefab;

	public MusicManager Music;

	public DrawManager.SceneReferences DrawSceneReferences;

	public HUDConfiguration HUDConfiguration;

	public EventSystem MainEventSystem;

	[NonSerialized]
	private EventQueue PassiveEventBus = new EventQueue();

	private DependencyContainer DependencyContainer;

	[NonSerialized]
	public HUD HUD;

	[NonSerialized]
	public VisualTheme Theme = null;

	[NonSerialized]
	public Player LocalPlayer;

	[NonSerialized]
	public Player SystemPlayer;

	[NonSerialized]
	public DebugConsole Console;

	[NonSerialized]
	public Savegame Savegame;

	[NonSerialized]
	public SavegameCoordinator SavegameCoordinator;

	[NonSerialized]
	public GameModeHandle Mode;

	[NonSerialized]
	public GameCoreHooks Hooks;

	[NonSerialized]
	public ShapeManager Shapes;

	[NonSerialized]
	public CrateItemManager CrateItems;

	[NonSerialized]
	public GameInputManager Input;

	[NonSerialized]
	public DebugViewManager DebugViews;

	[NonSerialized]
	public SimulationSpeedManager SimulationSpeed;

	[NonSerialized]
	public ResearchManager Research;

	[NonSerialized]
	public MapManager Maps;

	[NonSerialized]
	public PlayerActionManager PlayerActions;

	[NonSerialized]
	public LayerManager Layers;

	[NonSerialized]
	public ExpiringResourceManager ExpiringResources;

	[NonSerialized]
	public DrawManager Draw;

	[NonSerialized]
	public TutorialManager Tutorial;

	[NonSerialized]
	public UnityEvent OnGameInitialized = new UnityEvent();

	protected bool Crashed = false;

	private bool IsDisposed;

	public bool Initialized => OnGameInitialized == null;

	public void Start()
	{
		StartCoroutine("Init");
	}

	public void Update()
	{
		if (!IsDisposed)
		{
			PerformTick(Time.deltaTime);
		}
	}

	private void OnApplicationQuit()
	{
		Dispose();
	}

	protected override void OnDestroy()
	{
		if (!IsDisposed)
		{
			Debug.LogError("GameCore is not disposed OnDestroy.");
		}
		base.OnDestroy();
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			IsDisposed = true;
			Debug.Log("Core:: Disposing ...");
			PlayerActions.OnGameCleanup();
			Console.OnGameCleanup();
			Theme.OnGameCleanup();
			HUD?.Dispose();
			HUD = null;
			Tutorial.Dispose();
			Maps.OnGameCleanup();
			Shapes.OnGameCleanup();
			CrateItems.OnGameCleanup();
			ExpiringResources.OnGameCleanup();
			Music.OnGameCleanup();
			Draw.OnGameCleanup();
			PassiveEventBus?.Dispose();
			DOTween.KillAll();
			DependencyContainer?.Dispose();
		}
	}

	private IEnumerator Init()
	{
		foreach (string item in Globals.Init())
		{
			yield return item;
		}
		try
		{
			OnGlobalsInitialized();
		}
		catch (Exception ex)
		{
			CrashWithFatalError("Failed to initialize game:\n" + ex);
			throw;
		}
	}

	protected void OnGlobalsInitialized()
	{
		Debug.Log("Core:: OnGlobalsInitialized");
		Globals.Settings.General.PreloadIntroShown.SetValue(value: true);
		Globals.Settings.Display.InitDisplay();
		LocalPlayer = new Player(Player.PlayerRole.LocalHost);
		SystemPlayer = new Player(Player.PlayerRole.GameInternal);
		SavegameCoordinator = new SavegameCoordinator(Globals.CurrentGameStartOptionsPassOver, Globals.Savegames);
		Globals.CurrentGameStartOptionsPassOver = null;
		DependencyContainer = CreateDependencyContainer();
		Theme.OnGameInitialize();
		SavegameCoordinator.InitAfterCoreLoad();
		DependencyContainer.Bind<GameModeHandle>().To(Mode);
		Draw.Init(Theme);
		Tutorial.Init();
		HUD = new HUD(DependencyContainer, DrawSceneReferences.HUDRoot);
		if (SavegameCoordinator.Headless)
		{
			HUD.Initialize(new EmptyHUDConfiguration());
		}
		else
		{
			HUD.Initialize(HUDConfiguration);
		}
		UnityEngine.Object.Destroy(Singleton<GameCore>.G.DrawSceneReferences.LoadingOverlay);
		MainEventSystem.gameObject.SetActiveSelfExt(!SavegameCoordinator.Headless);
		RegisterConsoleCommands();
		GC.Collect();
		OnGameInitialized.InvokeAndClear();
		OnGameInitialized = null;
		InitGarbageCollection();
		Music.OnGameInitialized();
	}

	private DependencyContainer CreateDependencyContainer()
	{
		Console = new DebugConsole();
		Draw = new DrawManager(LocalPlayer, DrawSceneReferences);
		PlayerActions = new PlayerActionManager(LocalPlayer, PassiveEventBus);
		DebugViews = new DebugViewManager();
		SimulationSpeed = new SimulationSpeedManager();
		Maps = new MapManager();
		Shapes = new ShapeManager();
		CrateItems = new CrateItemManager();
		Input = new GameInputManager(LocalPlayer, Globals.Keybindings);
		Research = new ResearchManager(LocalPlayer);
		Layers = new LayerManager();
		ExpiringResources = new ExpiringResourceManager();
		Hooks = new GameCoreHooks();
		Theme = new SpaceTheme();
		Tutorial = new TutorialManager(LocalPlayer, Research, PassiveEventBus);
		DependencyContainer container = new DependencyContainer();
		container.Bind<Core.Logging.ILogger>().To(new UnityLogger());
		container.Bind<IPrefabViewInstanceProvider>().To<PrefabViewInstanceConstructor>();
		container.Bind<IHUDConfiguration>().To(HUDConfiguration);
		container.Bind<DebugConsole>().To(Console);
		container.Bind<DrawManager>().To(Draw);
		container.Bind<PlayerActionManager>().To(PlayerActions);
		container.Bind<DebugViewManager>().To(DebugViews);
		container.Bind<SimulationSpeedManager>().To(SimulationSpeed);
		container.Bind<MapManager>().To(Maps);
		container.Bind<ShapeManager>().To(Shapes);
		container.Bind<CrateItemManager>().To(CrateItems);
		container.Bind<GameInputManager>().To(Input);
		container.Bind<ResearchManager>().To(Research);
		container.Bind<LayerManager>().To(Layers);
		container.Bind<ExpiringResourceManager>().To(ExpiringResources);
		container.Bind<VisualTheme>().To(Theme);
		container.Bind<IEventSender>().To(PassiveEventBus);
		container.Bind<SavegameCoordinator>().To(SavegameCoordinator);
		container.Bind<CameraGameSettings>().To(Globals.Settings.Camera);
		container.Bind<Player>().To(LocalPlayer);
		container.Bind<ITutorialStateReadAccess>().To(LocalPlayer.TutorialState);
		container.Bind<ITutorialStateWriteAccess>().To(LocalPlayer.TutorialState);
		container.Bind<ITutorialProvider>().To(Tutorial);
		container.Bind<ITutorialHighlightProvider>().To(Tutorial);
		container.Bind<IBlueprintLibraryAccess>().To(LocalPlayer.BlueprintLibrary);
		container.Bind<IPlayerWikiManager>().To(LocalPlayer.WikiManager);
		return container;
	}

	protected void RegisterConsoleCommands()
	{
		GeneralCommands.RegisterCommands(Console);
		Globals.Keybindings.RegisterCommands(Console);
		Globals.Settings.RegisterCommands(Console);
		PlayerActions.RegisterCommands(Console);
		Research.RegisterCommands(Console);
		Shapes.RegisterCommands(Console);
		SavegameCoordinator.RegisterCommands(Console);
		LocalPlayer.RegisterCommands(Console);
		ExpiringResources.RegisterCommands(Console);
		SimulationSpeed.RegisterCommands(Console);
		LazyCombinedMesh.RegisterCommands(Console);
		Draw.RegisterCommands(Console);
		Theme.RegisterCommands(Console);
		Tutorial.RegisterCommands(Console);
	}

	public void PerformTick(float delta)
	{
		if (Initialized && !Crashed)
		{
			SimulationSpeed.PerformTick(delta);
			Input.OnGameUpdate();
			InputDownstreamContext inputs = Input.DownstreamContext;
			Draw.BeginFrame(inputs);
			DebugViews.HandleInput(inputs);
			Layers.SyncViewportHeight();
			HUD.OnGameUpdate(inputs, Draw.DrawOptions);
			while (PassiveEventBus.TryDequeue())
			{
			}
			Tutorial.Update();
			Hooks.AfterInputUpdate?.Invoke();
			bool rendering = inputs.ConsumeToken("HUDPart$render_3d");
			Draw.PrepareDrawOptionsFromContext(inputs);
			Draw.ScheduleCameraDependentJobs();
			if (inputs.ConsumeToken("HUDPart$advance_playtime"))
			{
				LocalPlayer.TotalPlaytime += delta;
			}
			Draw.ContinueFrameAfterHUDTick(inputs);
			Maps.OnGameUpdate(delta, rendering);
			Draw.ScheduleSimulationDependentJobs();
			Music.OnGameUpdate();
			if (rendering)
			{
				Draw.DrawMap();
				Draw.DrawBackground();
				DebugViews.OnGameDraw();
			}
			Cursor.lockState = (((bool)Globals.Settings.Camera.ConfineMouseCursor && inputs.IsTokenAvailable("HUDPart$confine_cursor") && !SavegameCoordinator.Headless) ? CursorLockMode.Confined : CursorLockMode.None);
			Draw.FinalizeFrame();
		}
	}

	public void ReturnToMenu()
	{
		AsyncOperation operation = SceneManager.LoadSceneAsync("GameUnloading", LoadSceneMode.Additive);
		operation.completed += delegate
		{
			Dispose();
		};
	}

	protected void InitGarbageCollection()
	{
		int garbargeCollectInterval = 10;
		InvokeRepeating("GC_Shapes", 0.1f, garbargeCollectInterval);
		InvokeRepeating("GC_Drawing", 0.2f, garbargeCollectInterval);
		InvokeRepeating("GC_ExpiringResources", 0.3f, garbargeCollectInterval);
		InvokeRepeating("GC_VisualTheme", 0.4f, garbargeCollectInterval);
	}

	protected void GC_Drawing()
	{
		Draw.GarbageCollect();
	}

	protected void GC_Shapes()
	{
		Shapes.GarbageCollect();
	}

	protected void GC_VisualTheme()
	{
		Theme.GarbageCollect();
	}

	protected void GC_ExpiringResources()
	{
		ExpiringResources.GarbageCollect();
	}

	public void CrashWithFatalError(string message)
	{
		if (Crashed)
		{
			Debug.LogWarning("Duplicate error after crash: " + message);
			return;
		}
		Crashed = true;
		HUDCrashOverlay overlay = UnityEngine.Object.Instantiate(UICrashOverlayPrefab, DrawSceneReferences.MainCanvas.transform);
		overlay.Setup(message);
		HUD?.Dispose();
		HUD = null;
		if (EventSystem.current == null)
		{
			MainEventSystem.gameObject.SetActive(value: true);
		}
	}
}
