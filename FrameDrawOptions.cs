using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class FrameDrawOptions
{
	public RegularMeshRenderer RegularRenderer;

	public RegularMeshRenderer AnalogUIRenderer;

	public InstancedMeshManager MiscInstanceManager;

	public InstancedMeshManager DynamicBuildingsInstanceManager;

	public InstancedMeshManager StaticBuildingsInstanceManager;

	public InstancedMeshManager GlassBuildingsInstanceManager;

	public InstancedMeshManager IslandInstanceManager;

	public InstancedMeshManager PlayingfieldInstanceManager;

	public InstancedMeshManager FluidsInstanceManager;

	public InstancedMeshManager ShapeInstanceManager;

	public InstancedMeshManager BackgroundInstanceManager;

	public InstancedMeshManager AnalogUIInstanceManager;

	public InstancedMeshManager TrainsInstanceManager;

	public InstancedMeshManager FluidMapResourcesInstanceManager;

	public InstancedMeshManager ShapeMapResourcesInstanceManager;

	public InstancedMeshManager TunnelsInstanceManager;

	public InstancedMeshManager EffectsInstanceManager;

	public Player Player;

	public PlayerViewport Viewport;

	public float3 CameraPosition_W;

	public VisualTheme Theme;

	public Plane[] CameraPlanes = new Plane[6];

	public FrameRenderStats RenderStats = new FrameRenderStats();

	protected FrameBudgetKeeper BudgetKeeper = new FrameBudgetKeeper();

	public double SimulationTime_G;

	public float AnimationSimulationTime_G;

	public int FrameIndex;

	public DrawHooks Hooks;

	public bool DrawShapeResources = true;

	public bool DrawFluidResources = true;

	public bool DrawIslands = true;

	public bool DrawBackground = true;

	public bool DrawTrains = true;

	public bool DrawRails = true;

	public int BuildingsLOD = -1;

	public int IslandLOD = -1;

	public bool ShouldDrawBuildingsMinimalMode => LODManager.ShouldDrawBuildingsMinimalMode(IslandLOD);

	public bool ShouldCombineUpperIslandFrameMesh => IslandLOD > 3;

	public bool ShouldCombineLowerIslandFrameMesh => IslandLOD > 3;

	public bool ShouldCombineContentsMesh => BuildingsLOD >= 2;

	public bool ShouldCombineGlassMesh => BuildingsLOD >= 2;

	public void OnBeforeFrameDraw()
	{
		BudgetKeeper.RemainingBudget = 25;
	}

	public bool ConsumeLargeOperation()
	{
		if (BudgetKeeper.RemainingBudget > 0)
		{
			BudgetKeeper.RemainingBudget--;
			return true;
		}
		return false;
	}

	public CommandBuilder GetDebugDrawManager()
	{
		CommandBuilder draw = DrawingManager.GetBuilder(renderInGame: true);
		draw.cameraTargets = new Camera[1] { Viewport.MainCamera };
		return draw;
	}
}
