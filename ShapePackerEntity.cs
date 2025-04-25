using System;
using Unity.Mathematics;

public class ShapePackerEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane LowerInputLane;

	protected BeltLane UpperInputLane;

	protected BeltLane OutputLane;

	protected ShapeDefinition CurrentItem;

	protected int CurrentItemCount;

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1]
		{
			new HUDSidePanelModuleStatShapeCrateSize(ShapeCrateItem.SIZE)
		};
	}

	public ShapePackerEntity(CtorArgs payload)
		: base(payload)
	{
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[2]);
		LowerInputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0]);
		UpperInputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1]);
		LowerInputLane.PreAcceptHook = InputPreAcceptHook;
		UpperInputLane.PreAcceptHook = InputPreAcceptHook;
		LowerInputLane.PostAcceptHook = InputPostAcceptHook;
		LowerInputLane.MaxStep_S = 400000;
		UpperInputLane.PostAcceptHook = InputPostAcceptHook;
		UpperInputLane.MaxStep_S = 400000;
	}

	protected BeltItem InputPreAcceptHook(BeltItem item)
	{
		if (CurrentItem == null)
		{
			return item;
		}
		if (OutputLane.HasItem)
		{
			return null;
		}
		ShapeDefinition definition = ((ShapeItem)item).Definition;
		ShapeManager shapeManager = Singleton<GameCore>.G.Shapes;
		for (int rotation = 0; rotation < definition.PartCount; rotation++)
		{
			if (definition == CurrentItem)
			{
				return item;
			}
			definition = shapeManager.GetDefinitionByHash(shapeManager.Op_Rotate.Execute(new ShapeOperationRotatePayload
			{
				Shape = definition,
				AmountClockwise = 1
			}));
		}
		return null;
	}

	protected void InputPostAcceptHook(BeltLane lane, ref int remainingTicks_T)
	{
		if (CurrentItem == null)
		{
			CurrentItem = ((ShapeItem)lane.Item).Definition;
		}
		CurrentItemCount++;
		lane.ClearLaneRaw_UNSAFE();
		lane.MaxStep_S = 400000;
		if (CurrentItemCount >= ShapeCrateItem.SIZE)
		{
			if (OutputLane.HasItem)
			{
				throw new Exception("Output lane must not have item while packaging shape");
			}
			OutputLane.Item = Singleton<GameCore>.G.CrateItems.GetShapeCrateByHash(CurrentItem.Hash);
			OutputLane.Progress_T = 0;
			CurrentItem = null;
			CurrentItemCount = 0;
		}
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return (index == 0) ? LowerInputLane : UpperInputLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, OutputLane);
		BeltSimulation.UpdateLane(options, LowerInputLane);
		BeltSimulation.UpdateLane(options, UpperInputLane);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, OutputLane);
		float progress = 0f;
		if (CurrentItem != null)
		{
			progress = 0.05f + 0.95f * math.saturate((float)CurrentItemCount / (float)ShapeCrateItem.SIZE);
		}
		if (!(progress <= 0.01f))
		{
			DrawDynamic_RawShape(options, CurrentItem, new float3(0.1191f, 0f, 0.905f));
			DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], new float3(-0.24514f, -0.2428f, 0.89397f), new float3(1f, progress, 1f));
		}
	}
}
