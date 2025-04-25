using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct TunnelLane
{
	public static int TUNNEL_THROUGHPUT_MULTIPLIER = 5;

	public static int TUNNEL_EFFECTIVE_LENGTH = 20;

	public static int TUNNEL_LENGTH_S = TUNNEL_EFFECTIVE_LENGTH * 100000;

	public int Index;

	public BeltLane InputLane;

	public BeltPathLogic Path;

	public TunnelEntranceEntity Parent;

	public TunnelLane(TunnelEntranceEntity parent, int index, BeltLaneDefinition definition)
	{
		Parent = parent;
		Index = index;
		Path = new BeltPathLogic(TUNNEL_LENGTH_S);
		InputLane = new BeltLane(definition);
		InputLane.PreAcceptHook = InputPreAcceptHook;
		InputLane.PostAcceptHook = InputPostAcceptHook;
		InputLane.MaxStep_S = -1;
	}

	public TunnelExitEntity FindExitEntity_Slow()
	{
		return (TunnelExitEntity)(((TunnelEntranceIsland)Parent.Island).CachedExit?.Buildings.Buildings[0]);
	}

	public BeltLane FindOutputLane_Slow()
	{
		return FindExitEntity_Slow().Lanes[Index];
	}

	public BeltItem GetRepresentativeItem()
	{
		if (Path.Items.Count > 0)
		{
			return Path.Items[0].Item;
		}
		return null;
	}

	public BeltItem InputPreAcceptHook(BeltItem item)
	{
		TunnelExitIsland exit = ((TunnelEntranceIsland)Parent.Island).CachedExit;
		if (exit == null)
		{
			return null;
		}
		return item;
	}

	public void InputPostAcceptHook(BeltLane inputLane, ref int excessTicks_T)
	{
		int steps_S = InputLane.Definition.S_From_T(excessTicks_T);
		if (Path.AcceptItem(inputLane.Item, steps_S, Path_ComputeMaxProgress_S()))
		{
			inputLane.ClearLaneRaw_UNSAFE();
			UpdateInputLaneMaxStep();
			return;
		}
		throw new Exception("Path logic didn't accept item - but there is no reason to. (Max=" + Path_ComputeMaxProgress_S() + ")");
	}

	public void UpdateInputLaneMaxStep()
	{
		if (InputLane.HasItem)
		{
			Debug.LogError("DEV ERROR: Conversion lane on tunnel entrance has item after update");
			InputLane.ClearLaneRaw_UNSAFE();
		}
		if (Path.Items.Count >= 30)
		{
			InputLane.MaxStep_S = -1;
		}
		else
		{
			InputLane.MaxStep_S = Path.FirstItemDistance_S - 50000;
		}
	}

	public int Path_ComputeMaxProgress_S()
	{
		return Path.Length_S + FindOutputLane_Slow().MaxStep_S;
	}

	public int Path_S_From_T(int steps_T)
	{
		return InputLane.Definition.S_From_T(steps_T) * TUNNEL_THROUGHPUT_MULTIPLIER;
	}

	public int Path_T_From_S(int steps_S)
	{
		if (steps_S % TUNNEL_THROUGHPUT_MULTIPLIER != 0)
		{
			Debug.LogWarning("Tunnel bad steps_T: " + steps_S + " mult = " + TUNNEL_THROUGHPUT_MULTIPLIER);
		}
		return InputLane.Definition.T_From_S(steps_S / TUNNEL_THROUGHPUT_MULTIPLIER);
	}

	public bool Path_ItemTransferHandler(BeltItem item, int excessPathSteps_S)
	{
		int excessTicks_T = Path_T_From_S(excessPathSteps_S);
		BeltLane outputLane = FindOutputLane_Slow();
		return BeltSimulation.TransferToLane(item, outputLane, excessTicks_T);
	}

	public int Path_MinStepsHandler_S()
	{
		return -FindOutputLane_Slow().MaxStep_S;
	}

	public void Update(TickOptions options)
	{
		TunnelExitIsland exit = ((TunnelEntranceIsland)Parent.Island).CachedExit;
		if (exit == null)
		{
			Path.ClearItems();
			return;
		}
		int pathSteps_S = Path_S_From_T(options.DeltaTicks_T);
		Path.Update(options, pathSteps_S, endIsConnected: true, Path_ItemTransferHandler, Path_MinStepsHandler_S);
		UpdateInputLaneMaxStep();
	}

	public void Draw(FrameDrawOptions options)
	{
		TunnelExitIsland exit = ((TunnelEntranceIsland)Parent.Island).CachedExit;
		if (exit == null)
		{
			return;
		}
		ChunkDirection vectorToEntrance_GC = Parent.Island.Origin_GC - exit.Origin_GC;
		int distanceToEntrance_GC = math.abs(vectorToEntrance_GC.x) + math.abs(vectorToEntrance_GC.y);
		int pathLength_G = 25 + (distanceToEntrance_GC - 1) * 20;
		int progress_S = Path.FirstItemDistance_S;
		Bounds bounds = new Bounds(Vector3.zero, new Vector3(1f, 1f, 1f));
		List<BeltPathLogic.ItemOnBelt> items = Path.Items;
		for (int i = 0; i < items.Count; i++)
		{
			BeltPathLogic.ItemOnBelt entry = items[i];
			BeltItem item = entry.Item;
			float progress = (float)progress_S / (float)Path.Length_S;
			float worldProgress = progress * (float)pathLength_G;
			float height = Globals.Resources.BeltShapeHeight;
			float offset = (float)Index - 2f;
			float3 scale = new float3(1f);
			if (worldProgress > 5f && worldProgress < (float)pathLength_G - 5f)
			{
				worldProgress += (float)Index;
				float angle = worldProgress / 15f + (float)Index * 1f;
				height = -11.3f + math.sin(angle) * 1f;
				offset = 0.5f + math.cos(angle) * 1f;
				scale = 2.5f;
			}
			float3 pos_L = new float3(worldProgress, offset, height);
			float3 pos_W = Parent.W_From_L(in pos_L);
			bounds.center = pos_W;
			progress_S += entry.NextItemDistance_S;
			if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, bounds))
			{
				options.ShapeInstanceManager.AddInstance(item.GetDefaultInstancingKey(), item.GetMesh(), item.GetMaterial(), FastMatrix.TranslateScale(in pos_W, in scale));
			}
		}
	}
}
