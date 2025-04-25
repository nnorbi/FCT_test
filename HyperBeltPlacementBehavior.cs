#define UNITY_ASSERTIONS
using System.Collections.Generic;
using UnityEngine;

public class HyperBeltPlacementBehavior : IslandPlacementBehaviour
{
	private IHyperBeltInputManager HyperBeltHyperBeltInputManager;

	private IHyperBeltSolver HyperBeltSolver;

	private IDrawer<HyperBelt> HyperBeltDrawer;

	private IDrawer<Checkpoint<GlobalChunkCoordinate>> CheckpointsDrawer;

	private IDrawer<(HyperBelt, HyperBeltInput)> HyperBeltIODrawer;

	private IPlacer<HyperBelt> HyperBeltPlacer;

	private HyperBeltMaterializer HyperBeltMaterializer;

	private int ChunkCost;

	public HyperBeltPlacementBehavior(CtorData data)
		: base(data)
	{
		HyperBeltHyperBeltInputManager = new HyperBeltInputManager(Player.Viewport);
		HyperBeltSolver = new HyperBeltSolver();
		HyperBeltMaterializer = new HyperBeltMaterializer();
		HyperBeltPlacementValidator nodeValidator = new HyperBeltPlacementValidator(data.Player, Singleton<GameCore>.G.Research.ChunkLimitManager, HyperBeltMaterializer);
		HyperBeltDrawer = new HyperBeltDrawer(data.Player.CurrentMap, HyperBeltMaterializer, nodeValidator, nodeValidator);
		CheckpointsDrawer = new CheckpointsDrawer<GlobalChunkCoordinate>(3f, 10f);
		HyperBeltIODrawer = new HyperBeltIODrawer();
		HyperBeltPlacer = new HyperBeltPlacer(data.Player, HyperBeltMaterializer, nodeValidator);
	}

	public override UpdateResult Update(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		base.Update(context, drawOptions);
		HyperBeltHyperBeltInputManager.Update(context, out var buildInput);
		HyperBelt hyperBelt = HyperBeltSolver.Solve(buildInput);
		if (TunnelsAreUnlocked())
		{
			hyperBelt = HyperBeltPathOptimizer.OptimizeBeltWithTunnels(hyperBelt);
		}
		hyperBelt = HyperBeltSelfIntersectChecker.InvalidateOverlappingNodes(hyperBelt);
		ChunkCost = hyperBelt.Nodes.Length;
		HyperBeltDrawer.Draw(drawOptions, in hyperBelt);
		CheckpointsDrawer.DrawBatch(drawOptions, HyperBeltHyperBeltInputManager.Checkpoints);
		HyperBeltIODrawer.Draw(drawOptions, (hyperBelt, buildInput));
		if (buildInput.ConfirmPlacement)
		{
			HyperBeltPlacer.Place(in hyperBelt);
			HyperBeltHyperBeltInputManager.Reset();
		}
		hyperBelt.Dispose();
		return UpdateResult.StayInPlacementMode;
	}

	private bool TunnelsAreUnlocked()
	{
		MetaIslandLayout senderLayout = HyperBeltMaterializer.GetLayoutForPart(HyperBeltPart.TunnelSender);
		MetaIslandLayout receiverLayout = HyperBeltMaterializer.GetLayoutForPart(HyperBeltPart.TunnelReceiver);
		bool isTunnelEntranceUnlocked = Singleton<GameCore>.G.Research.Progress.IsUnlocked(senderLayout);
		bool isTunnelExitUnlocked = Singleton<GameCore>.G.Research.Progress.IsUnlocked(receiverLayout);
		Debug.Assert(isTunnelEntranceUnlocked == isTunnelExitUnlocked, "Tunnel entrance and exit unlock mismatch. How can one be unlocked while the other one is not?");
		return isTunnelEntranceUnlocked && isTunnelExitUnlocked;
	}

	public override IEnumerable<HUDSidePanelHotkeyInfoData> GetActions()
	{
		return HyperBeltHyperBeltInputManager.GetActions();
	}

	public override PersistentPlacementData GetPersistentData()
	{
		return default(PersistentPlacementData);
	}

	public override int GetChunkCost()
	{
		return ChunkCost;
	}
}
