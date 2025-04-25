using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DrawManager
{
	[Serializable]
	public class SceneReferences
	{
		public Camera MainCamera;

		public Camera OverlayTransparentCamera;

		public Camera UICamera;

		public Canvas MainCanvas;

		public Transform HUDRoot;

		public GameObject LoadingOverlay;

		public UniversalRenderPipelineAsset URPAsset;
	}

	private static int TRANSPARENT_LAYER = LayerMask.NameToLayer("TransparentFX");

	protected Player Player;

	public FrameDrawOptions DrawOptions;

	public readonly SceneReferences References;

	public DrawHooks Hooks = new DrawHooks();

	private List<InstancedMeshManager> InstancingManagers = new List<InstancedMeshManager>();

	public int FrameIndex { get; protected set; } = 0;

	public DrawManager(Player player, SceneReferences references)
	{
		Player = player;
		References = references;
	}

	public void Init(VisualTheme theme)
	{
		Camera cam = References.MainCamera;
		int layer = 0;
		Camera transparentCam = References.OverlayTransparentCamera;
		Player.Viewport.MainCamera = cam;
		Player.Viewport.TransparentCamera = transparentCam;
		DrawOptions = new FrameDrawOptions
		{
			Player = Player,
			Theme = theme,
			Hooks = Hooks,
			MiscInstanceManager = MakeManager(cam, layer, RenderCategory.Misc),
			ShapeInstanceManager = MakeManager(cam, layer, RenderCategory.Shapes),
			BackgroundInstanceManager = MakeManager(cam, layer, RenderCategory.Background),
			AnalogUIInstanceManager = MakeManager(transparentCam, TRANSPARENT_LAYER, RenderCategory.AnalogUI),
			FluidsInstanceManager = MakeManager(cam, layer, RenderCategory.Fluids),
			DynamicBuildingsInstanceManager = MakeManager(cam, layer, RenderCategory.BuildingsDynamic),
			StaticBuildingsInstanceManager = MakeManager(cam, layer, RenderCategory.BuildingsStatic),
			GlassBuildingsInstanceManager = MakeManager(cam, layer, RenderCategory.BuildingsGlass),
			IslandInstanceManager = MakeManager(cam, layer, RenderCategory.Islands),
			PlayingfieldInstanceManager = MakeManager(cam, layer, RenderCategory.Playingfield),
			FluidMapResourcesInstanceManager = MakeManager(cam, layer, RenderCategory.FluidMapResources),
			ShapeMapResourcesInstanceManager = MakeManager(cam, layer, RenderCategory.ShapeMapResources),
			TrainsInstanceManager = MakeManager(cam, layer, RenderCategory.Trains),
			TunnelsInstanceManager = MakeManager(cam, layer, RenderCategory.Tunnels),
			EffectsInstanceManager = MakeManager(cam, layer, RenderCategory.Effects)
		};
		DrawOptions.RegularRenderer = new RegularMeshRenderer(DrawOptions.RenderStats, cam, layer);
		DrawOptions.AnalogUIRenderer = new RegularMeshRenderer(DrawOptions.RenderStats, transparentCam, TRANSPARENT_LAYER);
		bool headless = Singleton<GameCore>.G.SavegameCoordinator.Headless;
		References.UICamera.gameObject.SetActiveSelfExt(!headless);
		References.MainCanvas.gameObject.SetActiveSelfExt(!headless);
		UpdateCameraOptions();
		Globals.Settings.Graphics.Changed.AddListener(OnGraphicSettingsChanged);
		OnGraphicSettingsChanged();
		HUDAssignTranslucentImage.OnGameCameraCreated(References.MainCamera);
	}

	private InstancedMeshManager MakeManager(Camera camera, int layer, RenderCategory category)
	{
		InstancedMeshManager manager = new InstancedMeshManager(camera, layer, category);
		InstancingManagers.Add(manager);
		return manager;
	}

	public void RegisterCommands(DebugConsole console)
	{
	}

	protected void UpdateCameraOptions()
	{
		LODManager.CalculateLODDistances();
	}

	public void BeginFrame(InputDownstreamContext context)
	{
		if (Application.isEditor)
		{
			LODManager.CalculateLODDistances();
		}
		int frameIndex = FrameIndex + 1;
		FrameIndex = frameIndex;
		DrawOptions.OnBeforeFrameDraw();
		DrawOptions.Viewport = DrawOptions.Player.Viewport;
		DrawOptions.CameraPosition_W = DrawOptions.Viewport.MainCamera.transform.position;
		DrawOptions.SimulationTime_G = Singleton<GameCore>.G.SimulationSpeed.SimulationTime_G;
		DrawOptions.AnimationSimulationTime_G = (float)(DrawOptions.SimulationTime_G % 2048.0);
		DrawOptions.FrameIndex = FrameIndex;
	}

	public void ContinueFrameAfterHUDTick(InputDownstreamContext context)
	{
		DrawOptions.RenderStats.Reset();
		DrawOptions.CameraPosition_W = DrawOptions.Viewport.MainCamera.transform.position;
		GeometryUtility.CalculateFrustumPlanes(DrawOptions.Viewport.MainCamera, DrawOptions.CameraPlanes);
	}

	protected void OnGraphicSettingsChanged()
	{
		Globals.Settings.Graphics.Apply(Player.Viewport.MainCamera, References.URPAsset);
	}

	public void PrepareDrawOptionsFromContext(InputDownstreamContext context)
	{
		DrawOptions.DrawBackground = context.ConsumeToken("visualtheme:background");
		DrawOptions.DrawShapeResources = context.ConsumeToken("visualtheme:shape_resources");
		DrawOptions.DrawFluidResources = context.ConsumeToken("visualtheme:fluid_resources");
		DrawOptions.DrawIslands = context.ConsumeToken("visualtheme:islands");
		DrawOptions.DrawTrains = context.ConsumeToken("trains::render-trains");
		DrawOptions.DrawRails = context.ConsumeToken("trains::render-rails");
	}

	public void ScheduleCameraDependentJobs()
	{
		DrawOptions.Theme.Draw_ScheduleCameraDependentJobs(DrawOptions);
	}

	public void ScheduleSimulationDependentJobs()
	{
	}

	public void DrawMap()
	{
		Player.CurrentMap.OnGameDraw(DrawOptions);
	}

	public void DrawBackground()
	{
		if (DrawOptions.DrawBackground)
		{
			DrawOptions.Theme.Draw_Background(DrawOptions);
		}
	}

	public void FinalizeFrame()
	{
		foreach (InstancedMeshManager manager in InstancingManagers)
		{
			manager.DrawAll(DrawOptions);
		}
	}

	public void GarbageCollect()
	{
		foreach (InstancedMeshManager manager in InstancingManagers)
		{
			manager.GarbageCollect();
		}
	}

	public void OnGameCleanup()
	{
		Globals.Settings.Graphics.Changed.RemoveListener(OnGraphicSettingsChanged);
	}
}
