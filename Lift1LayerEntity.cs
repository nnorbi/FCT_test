using System.Collections.Generic;
using Unity.Mathematics;

public abstract class Lift1LayerEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected class VariantConfig
	{
		public float3 Paddle0_Pos_L;

		public float3 Paddle1_Pos_L;

		public float Paddle0_BaseRotation;

		public float Paddle1_BaseRotation;

		public float Paddle0_AngleScale;

		public float Paddle1_AngleScale;

		public float Paddle0_Length;

		public float Paddle1_Length;

		public bool RotateShapeWithPaddle;

		public bool EqualAnimations;

		public bool InvertAnimations;
	}

	protected BeltLane InputLane;

	protected BeltLane VerticalLane;

	protected BeltLane OutputLane;

	protected int CurrentAnimationSide = 0;

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	public Lift1LayerEntity(CtorArgs payload)
		: base(payload)
	{
		BeltLaneDefinition[] definitions = InternalVariant.BeltLaneDefinitions;
		OutputLane = new BeltLane(definitions[2]);
		VerticalLane = new BeltLane(definitions[1], OutputLane);
		InputLane = new BeltLane(definitions[0], VerticalLane);
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(VerticalLane);
	}

	protected abstract VariantConfig GetVariantConfig();

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, InputLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { InputLane, VerticalLane, OutputLane })
		};
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, OutputLane);
		if (BeltSimulation.UpdateLane(options, VerticalLane))
		{
			CurrentAnimationSide = 1 - CurrentAnimationSide;
		}
		BeltSimulation.UpdateLane(options, InputLane);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, OutputLane);
		DrawDynamic_BeltLane(options, InputLane);
		VariantConfig config = GetVariantConfig();
		BeltLaneDefinition verticalDefinition = VerticalLane.Definition;
		float startHeight = verticalDefinition.ItemStartPos_L.z;
		float endHeight = verticalDefinition.ItemEndPos_L.z;
		float progress = (VerticalLane.HasItem ? VerticalLane.Progress : 0f);
		float adjustedProgress = progress + (float)CurrentAnimationSide;
		if (config.InvertAnimations)
		{
			adjustedProgress = 2f - adjustedProgress;
		}
		float paddle0Angle = InternalVariant.GetCurve(0, adjustedProgress) * config.Paddle0_AngleScale + config.Paddle0_BaseRotation;
		float paddle0Height = math.lerp(startHeight, endHeight, InternalVariant.GetCurve(1, adjustedProgress));
		float paddle1Angle = InternalVariant.GetCurve((!config.EqualAnimations) ? 2 : 0, (adjustedProgress + 1f) % 2f) * config.Paddle1_AngleScale + config.Paddle1_BaseRotation;
		float paddle1Height = math.lerp(startHeight, endHeight, InternalVariant.GetCurve(config.EqualAnimations ? 1 : 3, (adjustedProgress + 1f) % 2f));
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], config.Paddle0_Pos_L + new float3(0f, 0f, paddle0Height + Globals.Resources.BeltShapeHeight - 0.05f), paddle0Angle);
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], config.Paddle1_Pos_L + new float3(0f, 0f, paddle1Height + Globals.Resources.BeltShapeHeight - 0.05f), paddle1Angle);
		if (VerticalLane.HasItem)
		{
			float height = paddle0Height;
			float angle = paddle0Angle;
			float shapeAngle = paddle0Angle - config.Paddle0_BaseRotation;
			float3 paddlePos = config.Paddle0_Pos_L;
			float paddleLength = config.Paddle0_Length;
			if (CurrentAnimationSide == 1)
			{
				height = paddle1Height;
				angle = paddle1Angle;
				shapeAngle = paddle1Angle - config.Paddle1_BaseRotation;
				paddlePos = config.Paddle1_Pos_L;
				paddleLength = config.Paddle1_Length;
			}
			float2 itemPos = Grid.Rotate(new float2(0f, paddleLength), angle) + paddlePos.xy;
			DrawDynamic_BeltItem(options, VerticalLane.Item, new float3(itemPos, height), config.RotateShapeWithPaddle ? shapeAngle : 0f);
		}
	}
}
