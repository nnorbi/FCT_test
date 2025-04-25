using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TrashEntity : MapEntity<MetaBuildingInternalVariant>
{
	[Serializable]
	protected class OngoingAnimation
	{
		[SerializeReference]
		public BeltItem Item;

		public float Progress;

		public Grid.Direction Direction_I;
	}

	protected static float3[] DIRECTION_TO_FALL_ROTATION = new float3[4]
	{
		new float3(0f, 0f, 1f),
		new float3(1f, 0f, 0f),
		new float3(0f, 0f, -1f),
		new float3(-1f, 0f, 0f)
	};

	protected List<OngoingAnimation> Animations = new List<OngoingAnimation>();

	protected BeltLane[] Lanes;

	protected float AnimationDuration => 2f * Lanes[0].Definition.ScaledDuration_NonDeterministic;

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	public TrashEntity(CtorArgs payload)
		: base(payload)
	{
		BeltLaneDefinition definition = InternalVariant.BeltLaneDefinitions[0];
		Lanes = new BeltLane[4]
		{
			new BeltLane(definition),
			new BeltLane(definition),
			new BeltLane(definition),
			new BeltLane(definition)
		};
		for (int i = 0; i < Lanes.Length; i++)
		{
			int index = i;
			BeltLane lane = Lanes[index];
			lane.MaxStep_S = 400000;
			lane.PostAcceptHook = delegate(BeltLane beltLane, ref int remainingTicks_T)
			{
				Grid.Direction direction_L = InternalVariant.BeltInputs[index].Direction_L;
				Animations.Add(new OngoingAnimation
				{
					Item = beltLane.Item,
					Progress = beltLane.Definition.TicksToSeconds_UNSAFE(beltLane.Progress_T) / AnimationDuration,
					Direction_I = I_From_L_Direction(direction_L)
				});
				beltLane.ClearLaneRaw_UNSAFE();
				beltLane.MaxStep_S = 400000;
			};
		}
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return Lanes[index];
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		for (int i = Animations.Count - 1; i >= 0; i--)
		{
			OngoingAnimation animation = Animations[i];
			animation.Progress += options.DeltaTime / AnimationDuration;
			if (animation.Progress > 1f)
			{
				Animations.RemoveAt(i);
			}
		}
	}

	protected override void DrawStatic_EndCap(MeshBuilder builder, MetaBuildingInternalVariant.BaseIO io, LODBaseMesh mesh)
	{
		IslandTileCoordinate tile_I = GetIOTargetTile_I(io);
		MapEntity contents = Island.GetEntity_I(in tile_I);
		if (!(contents is TrashEntity))
		{
			base.DrawStatic_EndCap(builder, io, mesh);
		}
	}

	protected override void DrawStatic_BaseMesh(MeshBuilder builder)
	{
		for (int i = 0; i < 4; i++)
		{
			IslandTileCoordinate tile_I = TileDirection.ByDirection((Grid.Direction)i).To_I(this);
			bool renderSide = true;
			MapEntity contents = Island.GetEntity_I(in tile_I);
			if (contents != null && contents.InternalVariant == InternalVariant)
			{
				renderSide = false;
			}
			if (renderSide)
			{
				int capLOD = math.min(builder.TargetLOD, 1);
				if (InternalVariant.SupportMeshesInternalLOD[0].TryGet(capLOD, out LODBaseMesh.CachedMesh capMesh))
				{
					builder.AddTranslateRotate(capMesh, W_From_L(new float3(0)), Grid.RotateDirection((Grid.Direction)i, Rotation_G));
				}
			}
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		for (int i = 0; i < Animations.Count; i++)
		{
			OngoingAnimation animation = Animations[i];
			float2 start = Grid.Rotate(new float2(0.5f, 0f), animation.Direction_I);
			float2 end = -start * 0.9f;
			float3 interpolated = new float3(math.lerp(start, end, animation.Progress), InternalVariant.GetCurve(1, animation.Progress) + Globals.Resources.BeltShapeHeight);
			float3 shapeRotation = DIRECTION_TO_FALL_ROTATION[(int)animation.Direction_I] * InternalVariant.GetCurve(0, animation.Progress) * 90f;
			options.ShapeInstanceManager.AddInstance(animation.Item.GetDefaultInstancingKey(), animation.Item.GetMesh(), animation.Item.GetMaterial(), Matrix4x4.TRS(Island.W_From_I(interpolated + (int3)Tile_I), Quaternion.Euler(shapeRotation), new Vector3(1f, 1f, 1f)));
		}
	}
}
