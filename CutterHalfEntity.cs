using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class CutterHalfEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane InputLane;

	protected BeltLane ProcessingLane;

	protected BeltLane OutputLane;

	protected ShapeCollapseResult CurrentWaste;

	protected ShapeCollapseResult CurrentCollapseResult;

	protected bool ProducingEmptyShape = false;

	protected new HalfCutterMetaBuildingInternalVariant InternalVariant;

	public CutterHalfEntity(CtorArgs payload)
		: base(payload)
	{
		InternalVariant = (HalfCutterMetaBuildingInternalVariant)base.InternalVariant;
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[2]);
		ProcessingLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1], OutputLane);
		InputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0], ProcessingLane);
		ProcessingLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			ShapeDefinition definition = (lane.Item as ShapeItem).Definition;
			ShapeCutResult shapeCutResult = Singleton<GameCore>.G.Shapes.Op_Cut.Execute(definition);
			CurrentWaste = shapeCutResult.LeftSide;
			CurrentCollapseResult = shapeCutResult.RightSide;
			ShapeItem itemByHash = Singleton<GameCore>.G.Shapes.GetItemByHash(shapeCutResult.RightSide?.ResultDefinition);
			if (itemByHash != null)
			{
				lane.Item = itemByHash;
				ProducingEmptyShape = false;
			}
			else
			{
				ProducingEmptyShape = true;
			}
		};
		OutputLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			if (ProducingEmptyShape)
			{
				lane.ClearLane();
			}
		};
	}

	protected override void Hook_SyncAdditionalContents(ISerializationVisitor visitor)
	{
		ShapeCollapseResult.Sync(visitor, ref CurrentWaste);
		ShapeCollapseResult.Sync(visitor, ref CurrentCollapseResult);
		visitor.SyncBool_1(ref ProducingEmptyShape);
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(ProcessingLane);
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, InputLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { InputLane, ProcessingLane, OutputLane })
		};
	}

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration + internalVariant.BeltLaneDefinitions[1].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, OutputLane);
		BeltSimulation.UpdateLane(options, ProcessingLane);
		BeltSimulation.UpdateLane(options, InputLane);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, InputLane);
		DrawDynamic_BeltLane(options, OutputLane);
		float3? wastePosition = null;
		DrawPlatform(options);
		if (ProcessingLane.HasItem)
		{
			DrawProcessing(options);
			DrawWaste(options, out wastePosition);
		}
		DrawRobot(options, out var shootPoint);
		if (AreJointsInPosition() && CurrentWaste?.ResultDefinition != null)
		{
			DrawLaserBeam(options, wastePosition, shootPoint);
		}
	}

	private bool AreJointsInPosition()
	{
		float progress = ProcessingLane.Progress;
		HalfCutterRobotJoint[] robotJoints = InternalVariant.RobotJoints;
		for (int i = 0; i < robotJoints.Length; i++)
		{
			HalfCutterRobotJoint joint = robotJoints[i];
			if (joint.RotationInterpolation.Evaluate(progress) < 0.95f)
			{
				return false;
			}
		}
		return true;
	}

	private void DrawLaserBeam(FrameDrawOptions options, float3? wastePosition, float3 shootPosition_W)
	{
		if (wastePosition.HasValue && InternalVariant.SupportMeshesInternalLOD[5].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh beamMesh))
		{
			float3 shapeCenter = CalculateShapeCollapseCenter(CurrentWaste);
			float dissolveProgress = DissolveProgress();
			float wasteOpacity = InternalVariant.WasteAlpha.Evaluate(dissolveProgress);
			if (dissolveProgress > 0f && dissolveProgress < 1f && wasteOpacity > 0.4f)
			{
				float3 pos_W = W_From_L(in shapeCenter);
				quaternion rotation = quaternion.LookRotation(math.normalize(shootPosition_W - pos_W), new float3(0f, 1f, 0f));
				float3 center = (shootPosition_W + pos_W) * 0.5f;
				float length = math.distance(shootPosition_W, pos_W);
				float3 scale = new float3(1f, 1f, length);
				options.DynamicBuildingsInstanceManager.AddInstance(beamMesh, options.Theme.BaseResources.OpaqueBuildingLaserMaterial, (Matrix4x4)float4x4.TRS(center, rotation, scale));
			}
		}
	}

	private float3 CalculateShapeCollapseCenter(ShapeCollapseResult shapeCollapseResult)
	{
		ShapeDefinition def = Singleton<GameCore>.G.Shapes.GetItemByHash(shapeCollapseResult.ResultDefinition).Definition;
		ShapeLayer firstLayer = def.Layers[0];
		int count = 0;
		float2 center = float2.zero;
		for (int i = 0; i < firstLayer.Parts.Length; i++)
		{
			ShapePart part = firstLayer.Parts[i];
			if (!part.IsEmpty)
			{
				float halfPartSize = 180f / (float)firstLayer.Parts.Length;
				float rotationDegrees = (float)i / (float)firstLayer.Parts.Length * 360f;
				float rotationRad = math.radians(rotationDegrees + halfPartSize);
				float halfSize = Globals.Resources.ShapeDimensions2D * 0.5f;
				float2 pos = new float2(math.cos(rotationRad), math.sin(rotationRad)) * halfSize;
				Grid.Direction rotation = Grid.RotateDirection(Rotation_G, Grid.Direction.Bottom);
				pos = Grid.Rotate(in pos, Grid.InvertDirection(rotation));
				center += pos;
				count++;
			}
		}
		center /= (float)count;
		return new float3(center, Globals.Resources.BeltShapeHeight);
	}

	private void DrawPlatform(FrameDrawOptions options)
	{
		DrawDynamic_MeshGlobalRotation(options, InternalVariant.SupportMeshesInternalLOD[0], new float3(0f, 0f, 0f), Grid.Direction.Bottom);
		DrawDynamic_MeshGlobalRotation(options, InternalVariant.SupportMeshesInternalLOD[4], new float3(0f, 0f, 0f), Grid.Direction.Bottom);
	}

	private void DrawProcessing(FrameDrawOptions draw)
	{
		float progress = ProcessingLane.Progress;
		if (InternalVariant.LaserBladeMesh.TryGet(draw.BuildingsLOD, out LODBaseMesh.CachedMesh laserBladeMesh))
		{
			Material laserMaterial = draw.Theme.BaseResources.BuildingLaserMaterial;
			float bladeHeight = InternalVariant.LaserBladeHeight.Evaluate(CutProgress());
			DrawSplitter(draw, bladeHeight, laserBladeMesh, laserMaterial);
		}
		if (InternalVariant.GlassShieldMesh.TryGet(draw.BuildingsLOD, out LODBaseMesh.CachedMesh shieldMesh))
		{
			Material glassMaterial = draw.Theme.BaseResources.BuildingsGlassMaterial;
			float shieldHeight = InternalVariant.GlassShieldHeight.Evaluate(DissolveProgress());
			DrawSplitter(draw, shieldHeight, shieldMesh, glassMaterial);
		}
		float collapseProgress = math.saturate(InternalVariant.CollapseProgress.Evaluate(progress));
		if (CurrentCollapseResult != null)
		{
			DrawDynamic_ShapeCollapseResult(draw, CurrentCollapseResult, new float3(ShapeHalvesOffset(), Globals.Resources.BeltShapeHeight + Globals.Resources.ShapeSupportHeight), collapseProgress, collapseProgress, collapseProgress);
		}
		float alpha = (ProducingEmptyShape ? (1f - progress) : 1f);
		DrawDynamic_LeftShapeSupportMesh(draw, new float3(0f, 0f, Globals.Resources.BeltShapeHeight), alpha);
		DrawDynamic_RightShapeSupportMesh(draw, new float3(0f, 0f, Globals.Resources.BeltShapeHeight), alpha);
	}

	private void DrawSplitter(FrameDrawOptions draw, float height, LODBaseMesh.CachedMesh mesh, Material material)
	{
		if (height > 0.01f)
		{
			draw.DynamicBuildingsInstanceManager.AddInstance(mesh, material, FastMatrix.TranslateScale(W_From_L(new float3(0f, 0f, 0.19f)), Grid.Scale_W_From_G(new float3(1f, 1f, 2.5f * height))));
		}
	}

	private void DrawWaste(FrameDrawOptions options, out float3? wastePosition)
	{
		if (CurrentWaste == null)
		{
			wastePosition = null;
			return;
		}
		float progress = ProcessingLane.Progress;
		GameResources resources = Globals.Resources;
		float wasteOpacity = InternalVariant.WasteAlpha.Evaluate(DissolveProgress());
		float height = resources.BeltShapeHeight + resources.ShapeSupportHeight;
		wastePosition = new float3(-ShapeHalvesOffset(), height);
		float collapseProgress = math.saturate(InternalVariant.CollapseProgress.Evaluate(progress));
		if (wasteOpacity > 0f)
		{
			DrawDynamic_ShapeCollapseResult(options, CurrentWaste, wastePosition.Value, collapseProgress, collapseProgress, collapseProgress, CustomDissolveShapeDrawer.WithOpacity(wasteOpacity));
		}
	}

	private void DrawRobot(FrameDrawOptions options, out float3 shootPosition_W)
	{
		float progress = ProcessingLane.Progress;
		if (CurrentWaste?.ResultDefinition == null)
		{
			progress = 0f;
		}
		Grid.Direction robotDirection = Grid.RotateDirection(Rotation_G, Grid.Direction.Bottom);
		float4x4 stackedMatrix = float4x4.TRS(W_From_L(in float3.zero), FastMatrix.RotateY(robotDirection), Vector3.one);
		HalfCutterRobotJoint[] robotJoints = InternalVariant.RobotJoints;
		for (int i = 0; i < robotJoints.Length; i++)
		{
			HalfCutterRobotJoint joint = robotJoints[i];
			if (joint.Mesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh lodMesh))
			{
				float rotationProgress = joint.RotationInterpolation.Evaluate(progress);
				quaternion currentRotation = math.slerp(joint.RestRotation, joint.AlignedRotation, rotationProgress);
				float4x4 trs = float4x4.TRS(joint.Position, currentRotation, new float3(1f, 1f, 1f));
				stackedMatrix = math.mul(stackedMatrix, trs);
				options.DynamicBuildingsInstanceManager.AddInstance(lodMesh.InstancingID, lodMesh, options.Theme.BaseResources.BuildingMaterial, (Matrix4x4)stackedMatrix);
			}
		}
		shootPosition_W = math.mul(stackedMatrix, new float4(InternalVariant.ShootPositionOffset, 1f)).xyz;
	}

	private float CutProgress()
	{
		return RemapProcessingProgress(0f, InternalVariant.LaserCutTime);
	}

	private float DissolveProgress()
	{
		return RemapProcessingProgress(InternalVariant.LaserCutTime, InternalVariant.DissolveTime);
	}

	private float RemapProcessingProgress(float start, float duration)
	{
		return math.saturate((ProcessingLane.Progress - start) / duration);
	}

	private float PlatformHeight()
	{
		return 0f;
	}

	private float2 ShapeHalvesOffset()
	{
		float distance = InternalVariant.ShapeHalvesOffset.Evaluate(ProcessingLane.Progress);
		float offset = distance * InternalVariant.ShapeHalvesMaxOffset;
		return Grid.Rotate(new float2(distance * offset, 0f), Grid.InvertDirection(Rotation_G));
	}
}
