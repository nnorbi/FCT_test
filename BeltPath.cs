using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class BeltPath
{
	public static int BELT_LENGTH_S = 100000;

	protected static int NEXT_UID = 1;

	public BeltLane FakeInputLane = null;

	public BeltLane FakeOutputLane = null;

	[NonSerialized]
	public List<BeltEntity> Belts;

	protected int LastDrawnIndex = 0;

	protected BeltPathLogic Logic;

	protected int UID = ++NEXT_UID;

	public int StepsPerTick_S => FakeInputLane.Definition.StepsPerTick_S;

	public BeltItem FirstItem => Logic.FirstItem;

	public BeltItem LastItem => Logic.LastItem;

	public BeltPath(List<BeltEntity> belts)
	{
		if (belts.Count < 1)
		{
			throw new Exception("Empty belt path not possible in ctor");
		}
		Belts = belts;
		Logic = new BeltPathLogic(Belts.Count * BELT_LENGTH_S);
		LaneLinks_Init();
		LinkBeltsToPath();
	}

	public void Serialize(ISerializationVisitor visitor)
	{
		Logic.Serialize(visitor);
	}

	public void Deserialize(ISerializationVisitor visitor)
	{
		Logic.Deserialize(visitor);
	}

	public List<BeltItem> GetItemsAtBeltIndex(int beltIndex)
	{
		return Logic.GetItemsAtRange_S(beltIndex * BELT_LENGTH_S, (beltIndex + 1) * BELT_LENGTH_S - 1);
	}

	public void ClearItems()
	{
		Logic.ClearItems();
		LaneLinks_UpdateInputMaxStep();
	}

	public void ClearItemsAtBelt(int beltIndex)
	{
		Logic.ClearItemsBetween(beltIndex * BELT_LENGTH_S, (beltIndex + 1) * BELT_LENGTH_S - 1);
		LaneLinks_UpdateInputMaxStep();
	}

	protected void LinkBeltsToPath()
	{
		for (int i = 0; i < Belts.Count; i++)
		{
			BeltEntity belt = Belts[i];
			belt.IndexInPath = i;
			belt.Path = this;
		}
	}

	protected void DestroyPath()
	{
		Logic.ClearItems();
		Belts.Clear();
	}

	public void Update(TickOptions options)
	{
		Logic.Update(options, FakeInputLane.Definition.S_From_T(options.DeltaTicks_T), FakeOutputLane.NextLane != null, HandleTransfer, HandleGetMinStepsToEnd_S);
		LaneLinks_UpdateInputMaxStep();
	}

	protected bool HandleTransfer(BeltItem item, int excessSteps_S)
	{
		int excessTicks_T = FakeInputLane.Definition.T_From_S(excessSteps_S);
		return BeltSimulation.TransferToLane(item, FakeOutputLane.NextLane, excessTicks_T);
	}

	protected int HandleGetMinStepsToEnd_S()
	{
		if (FakeOutputLane.NextLane == null)
		{
			return 25000;
		}
		return -FakeOutputLane.NextLane.MaxStep_S;
	}

	protected void Debug_PrintBelt(string name = "Belt")
	{
		string output = "";
		output = output + "-- BELT PATH <" + name + ">: Belts = " + Belts.Count + " | Items = " + Logic.Items.Count + " | First item @ " + Logic.FirstItemDistance_S + "\n";
		int pos_S = Logic.FirstItemDistance_S;
		for (int i = 0; i < Logic.Items.Count; i++)
		{
			BeltPathLogic.ItemOnBelt item = Logic.Items[i];
			output = output + "--  [Item " + i + "] is at " + pos_S + " with next distance = " + item.NextItemDistance_S + "\n";
			pos_S += item.NextItemDistance_S;
		}
		output = output + "--> Total belt path length = " + pos_S + " (should: " + Logic.Length_S + ")\n";
		UnityEngine.Debug.LogWarning(output);
	}

	[Conditional("UNITY_EDITOR")]
	protected void Debug_SanityChecks(bool checkLinks = true)
	{
	}

	public void Draw(FrameDrawOptions options)
	{
		if (LastDrawnIndex == options.FrameIndex)
		{
			return;
		}
		LastDrawnIndex = options.FrameIndex;
		BeltPathLogic logic = Logic;
		int itemCount = logic.Items.Count;
		if (itemCount == 0)
		{
			return;
		}
		Island island = Belts[0].Island;
		int pos_S = logic.FirstItemDistance_S;
		float beltHeight = Globals.Resources.BeltShapeHeight;
		DebugViewManager debugViews = Singleton<GameCore>.G.DebugViews;
		float3 islandPos_W = island.Origin_GC.ToOrigin_W();
		islandPos_W += (float3)(beltHeight * WorldDirection.Up);
		List<BeltPathLogic.ItemOnBelt> items = logic.Items;
		for (int i = 0; i < itemCount; i++)
		{
			if (pos_S < 0 || pos_S >= logic.Length_S)
			{
				UnityEngine.Debug.LogError("DEV ISSUE: Bad progress at " + i + " : d=" + pos_S + " -> (int)d=" + pos_S + " count=" + Belts.Count + " Length_S=" + logic.Length_S + " firstitem=" + logic.FirstItemDistance_S);
			}
			BeltPathLogic.ItemOnBelt item = items[i];
			int index = math.clamp((int)BeltLaneDefinition.StepsToWorld_UNSAFE(pos_S), 0, Belts.Count - 1);
			BeltEntity belt = Belts[index];
			float progress = BeltLaneDefinition.StepsToWorld_UNSAFE(pos_S % 100000);
			float2 itemPos_Rotated = Grid.Rotate(belt.GetItemPosition_L(progress).xy, belt.Rotation_G);
			IslandTileCoordinate basePos_W = belt.Tile_I;
			BeltItem beltItem = item.Item;
			options.ShapeInstanceManager.AddInstance(beltItem.GetDefaultInstancingKey(), beltItem.GetMesh(), beltItem.GetMaterial(), FastMatrix.Translate(new float3(islandPos_W.x + (itemPos_Rotated.x + (float)basePos_W.x), islandPos_W.y + (float)basePos_W.z, islandPos_W.z - (itemPos_Rotated.y + (float)basePos_W.y))));
			pos_S += item.NextItemDistance_S;
		}
	}

	protected void LaneLinks_Init()
	{
		MetaBuildingInternalVariant fakeBelt = Singleton<GameCore>.G.Mode.GetBuildingInternalVariant("BeltDefaultForwardInternalVariant");
		FakeInputLane = new BeltLane(fakeBelt.BeltLaneDefinitions[0]);
		FakeOutputLane = new BeltLane(fakeBelt.BeltLaneDefinitions[1]);
		FakeInputLane.PostAcceptHook = LaneLinks_InputPostAcceptHook;
	}

	protected void LaneLinks_RemoveInputLink()
	{
		MapEntity.Belts_LinkedEntity connectedInput = Belts[0].Belts_GetInputConnections()[0];
		if (connectedInput.Entity != null)
		{
			connectedInput.Entity.Belts_GetLaneForOutput(connectedInput.SlotIndex).NextLane = null;
		}
	}

	protected void LaneLinks_LinkOutput()
	{
		List<BeltEntity> belts = Belts;
		MapEntity.Belts_LinkedEntity connectedOutput = belts[belts.Count - 1].Belts_GetOutputConnections()[0];
		FakeOutputLane.NextLane = null;
		if (connectedOutput.Entity != null)
		{
			FakeOutputLane.NextLane = connectedOutput.Entity.Belts_GetLaneForInput(connectedOutput.SlotIndex);
		}
	}

	protected void LaneLinks_LinkInput()
	{
		MapEntity.Belts_LinkedEntity connectedInput = Belts[0].Belts_GetInputConnections()[0];
		if (connectedInput.Entity != null)
		{
			connectedInput.Entity.Belts_GetLaneForOutput(connectedInput.SlotIndex).NextLane = FakeInputLane;
		}
	}

	protected void LaneLinks_LinkInputAndOutput()
	{
		LaneLinks_LinkInput();
		LaneLinks_LinkOutput();
	}

	protected void LaneLinks_UpdateInputMaxStep()
	{
		if (FakeInputLane.HasItem)
		{
			string[] obj = new string[11]
			{
				"Fake input lane on belt path at ",
				Belts[0].Island.Descriptor.ToString(),
				" (tile ",
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};
			IslandTileCoordinate tile_I = Belts[0].Tile_I;
			obj[3] = tile_I.ToString();
			obj[4] = ") has item ";
			obj[5] = FakeInputLane.Item.Serialize();
			obj[6] = " at Progress_T=";
			obj[7] = FakeInputLane.Progress_T.ToString();
			obj[8] = " MaxStep_S=";
			obj[9] = FakeInputLane.MaxStep_S.ToString();
			obj[10] = " AFTER update -> Clearing item";
			UnityEngine.Debug.LogError(string.Concat(obj));
			FakeInputLane.ClearLaneRaw_UNSAFE();
		}
		FakeInputLane.MaxStep_S = Logic.FirstItemDistance_S - 50000;
	}

	protected void LaneLinks_InputPostAcceptHook(BeltLane lane, ref int remainingTicks_T)
	{
		int steps_S = remainingTicks_T * StepsPerTick_S;
		if (!Logic.AcceptItem(lane.Item, steps_S, LaneLinks_GetMaxProgress_S()))
		{
			UnityEngine.Debug.LogError("Item accept of " + lane.Item.Serialize() + " failed on belt path, but should not happen. remainingTicks_T = " + remainingTicks_T + ", maxProgress_S = " + LaneLinks_GetMaxProgress_S() + " maxStep_S =" + FakeInputLane.MaxStep_S + " -> item will be lost");
		}
		lane.ClearLaneRaw_UNSAFE();
		LaneLinks_UpdateInputMaxStep();
	}

	protected int LaneLinks_GetMaxProgress_S()
	{
		if (FakeOutputLane.NextLane == null)
		{
			return Logic.Length_S - 25000;
		}
		return Logic.Length_S + FakeOutputLane.MaxStep_S;
	}

	public static void Modify_IntegrateBeltIntoPath(BeltEntity belt, BeltPath sourcePath, BeltPath destinationPath)
	{
		if (sourcePath != null && destinationPath != null)
		{
			sourcePath.Modify_MergeWith(belt, destinationPath);
			return;
		}
		if (sourcePath != null)
		{
			sourcePath.Modify_AppendBelt(belt);
			return;
		}
		if (destinationPath != null)
		{
			destinationPath.Modify_PrependBelt(belt);
			return;
		}
		BeltPath path = new BeltPath(new List<BeltEntity> { belt });
		path.LaneLinks_LinkInputAndOutput();
	}

	public static void Modify_RemoveBeltFromPath(BeltEntity belt)
	{
		belt.Path.Modify_DeleteBelt(belt);
	}

	protected void Modify_AppendBelt(BeltEntity entity)
	{
		Belts.Add(entity);
		Logic.ExtendPathOnEndBy(BELT_LENGTH_S);
		LinkBeltsToPath();
		LaneLinks_LinkOutput();
	}

	protected void Modify_DeleteBelt(BeltEntity entity)
	{
		int removalIndex = Belts.IndexOf(entity);
		if (removalIndex < 0)
		{
			throw new Exception("Belt path: delete: entity not contained (" + removalIndex + ")");
		}
		if (Belts.Count == 1)
		{
			LaneLinks_RemoveInputLink();
			DestroyPath();
			return;
		}
		if (removalIndex == 0)
		{
			Logic.ShrinkPathFromBeginningBy(BELT_LENGTH_S);
			LaneLinks_RemoveInputLink();
			entity.Path = null;
			entity.IndexInPath = -1;
			Belts.Remove(entity);
			LinkBeltsToPath();
			LaneLinks_LinkInput();
			return;
		}
		if (removalIndex == Belts.Count - 1)
		{
			Logic.ShrinkPathFromEndBy(BELT_LENGTH_S);
			entity.Path = null;
			entity.IndexInPath = -1;
			Belts.Remove(entity);
			LaneLinks_LinkOutput();
			return;
		}
		for (int i = 0; i < Belts.Count; i++)
		{
			Belts[i].Path = null;
			Belts[i].IndexInPath = -1;
		}
		List<BeltEntity> secondBelts = Belts.GetRange(removalIndex + 1, Belts.Count - removalIndex - 1);
		Belts.RemoveRange(removalIndex, Belts.Count - removalIndex);
		LinkBeltsToPath();
		int removalStart_S = removalIndex * 100000;
		int removalEnd_S = removalStart_S + 100000;
		BeltPath secondPath = new BeltPath(secondBelts);
		Logic.SplitPath(removalStart_S, removalEnd_S, secondPath.Logic);
		LaneLinks_LinkOutput();
		secondPath.LaneLinks_LinkOutput();
	}

	protected void Modify_PrependBelt(BeltEntity entity)
	{
		Belts.Insert(0, entity);
		Logic.ExtendPathOnBeginningBy(100000);
		LinkBeltsToPath();
		LaneLinks_LinkInput();
	}

	protected void Modify_MergeWith(BeltEntity connector, BeltPath other)
	{
		if (other == this)
		{
			BeltPath path = new BeltPath(new List<BeltEntity> { connector });
			path.LaneLinks_LinkInputAndOutput();
			return;
		}
		Modify_AppendBelt(connector);
		if (!Modify_IsCircularDependency(other.Belts[other.Belts.Count - 1]))
		{
			Modify_AppendOtherPath(other);
			return;
		}
		other.LaneLinks_LinkInputAndOutput();
		LaneLinks_LinkInputAndOutput();
	}

	protected void Modify_AppendOtherPath(BeltPath other)
	{
		Logic.AppendOtherPath(other.Logic);
		Belts.AddRange(other.Belts);
		other.DestroyPath();
		LinkBeltsToPath();
		LaneLinks_LinkOutput();
	}

	protected bool Modify_IsCircularDependency(BeltEntity entity)
	{
		MapEntity.Belts_LinkedEntity target = entity.Belts_GetOutputConnections()[0];
		if (target.Entity != null && target.Entity is BeltEntity)
		{
			BeltPath path = ((BeltEntity)target.Entity).Path;
			return path == this;
		}
		return false;
	}
}
