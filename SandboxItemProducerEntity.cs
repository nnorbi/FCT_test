using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SandboxItemProducerEntity : MapEntity<MetaBuildingInternalVariant>
{
	public const string DEFAULT_RESOURCE_ITEM = "CuCuCuCu";

	protected BeltLane OutputLane;

	public BeltItem ResourceItem = null;

	public SandboxItemProducerEntity(CtorArgs payload)
		: base(payload)
	{
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0]);
		ResourceItem = Singleton<GameCore>.G.Shapes.GetItemByHash("CuCuCuCu");
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	protected override void Hook_SyncConfig(ISerializationVisitor visitor)
	{
		BeltItem.Sync(visitor, ref ResourceItem);
	}

	protected void HUD_ShowConfigureDialog()
	{
		HUDDialogSimpleInput dialog = Singleton<GameCore>.G.HUD.DialogStack.ShowUIDialog<HUDDialogSimpleInput>();
		dialog.InitDialogContents("sandbox-item-producer.dialog-title".tr(), "sandbox-item-producer.dialog-description".tr() + "\n\nShape Prefix: shape: \nShape Crate Prefix: shapecrate: \nFluid Crate Prefix: fluidcrate:", "global.btn-confirm".tr(), (ResourceItem as ShapeItem)?.Definition.Hash ?? "CuCuCuCu");
		dialog.OnConfirmed.AddListener(delegate(string text)
		{
			text = text.Trim();
			if (string.IsNullOrEmpty(text))
			{
				ResourceItem = null;
				return;
			}
			if (!text.StartsWith("shape:") && !text.StartsWith("fluidcrate:") && !text.StartsWith("shapecrate:"))
			{
				text = "shape:" + text;
			}
			try
			{
				BeltItem resourceItem = BeltItem.Deserialize(text);
				ResourceItem = resourceItem;
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Not a valid shape code: " + ex);
			}
		});
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[1]
		{
			new HUDSidePanelModuleGenericButton("global.btn-configure".tr(), HUD_ShowConfigureDialog)
		};
	}

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		if (ResourceItem != null)
		{
			if (OutputLane.Item == null)
			{
				OutputLane.Item = ResourceItem;
				OutputLane.Progress_T = 0;
			}
			if (BeltSimulation.UpdateLane(options, OutputLane))
			{
				OutputLane.Item = ResourceItem;
				OutputLane.Progress_T = math.min(OutputLane.Progress_T + options.DeltaTicks_T, OutputLane.MaxTickClamped_T);
			}
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, OutputLane);
		if (ResourceItem != null)
		{
			options.RegularRenderer.DrawMesh(ResourceItem.GetMesh(), FastMatrix.TranslateScale(W_From_L(new float3(0f, 0f, 0.66f)), new float3(2f)), ResourceItem.GetMaterial(), RenderCategory.Shapes);
		}
	}
}
