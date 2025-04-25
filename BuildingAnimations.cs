using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BuildingAnimations
{
	protected class BaseAnimation
	{
		public MetaBuildingInternalVariant InternalVariant;

		public float3 Pos_W;

		public Grid.Direction Direction;

		public float StartTime;

		public float AnimationDurationFactor = 1f;
	}

	protected class PlacementAnimation : BaseAnimation
	{
		public bool BaseMeshOnly = false;
	}

	protected class DeletionAnimation : BaseAnimation
	{
	}

	public const int MAX_ANIMATION_STACK = 300;

	public static int ID_Alpha = Shader.PropertyToID("_Alpha");

	public static int ID_BaseColor = Shader.PropertyToID("_BaseColor");

	protected static MaterialPropertyBlock PLACEMENT_BLOCK;

	protected static MaterialPropertyBlock DELETION_BLOCK;

	protected static float PlacementAnimationDuration = 0.25f;

	protected static float DeletionAnimationDuration = 0.15f;

	protected List<PlacementAnimation> PlacementAnimations = new List<PlacementAnimation>();

	protected List<DeletionAnimation> DeletionAnimations = new List<DeletionAnimation>();

	public static void InitMaterialBlocks(GameResources resources)
	{
		PLACEMENT_BLOCK = new MaterialPropertyBlock();
		DELETION_BLOCK = new MaterialPropertyBlock();
		PLACEMENT_BLOCK.SetVector(ID_BaseColor, resources.ThemePrimary.Color);
		DELETION_BLOCK.SetVector(ID_BaseColor, resources.ThemeErrorOrDelete.Color);
	}

	public void PlayPlace(MetaBuildingInternalVariant internalVariant, in float3 pos_W, Grid.Direction direction, float animationDurationFactor = 1f, bool baseMeshOnly = false)
	{
		if (PlacementAnimations.Count <= 300)
		{
			PlacementAnimations.Add(new PlacementAnimation
			{
				InternalVariant = internalVariant,
				Pos_W = pos_W,
				Direction = direction,
				StartTime = Time.time,
				AnimationDurationFactor = math.clamp(animationDurationFactor, 0.2f, 2f),
				BaseMeshOnly = baseMeshOnly
			});
		}
	}

	public void PlayDelete(MetaBuildingInternalVariant internalVariant, in float3 pos_W, Grid.Direction direction, float animationDurationFactor = 1f)
	{
		if (DeletionAnimations.Count <= 300)
		{
			DeletionAnimations.Add(new DeletionAnimation
			{
				InternalVariant = internalVariant,
				Pos_W = pos_W,
				Direction = direction,
				StartTime = Time.time,
				AnimationDurationFactor = math.clamp(animationDurationFactor, 0.2f, 2f)
			});
		}
	}

	public void DrawAndUpdate(FrameDrawOptions options)
	{
		float now = Time.time;
		GameResources resources = Globals.Resources;
		for (int i = PlacementAnimations.Count - 1; i >= 0; i--)
		{
			PlacementAnimation animation = PlacementAnimations[i];
			float progress = (now - animation.StartTime) / (PlacementAnimationDuration * animation.AnimationDurationFactor);
			if (progress >= 1f)
			{
				PlacementAnimations.RemoveAt(i);
			}
			else
			{
				float scale = resources.PlacementAnimationScaleFactor.Evaluate(progress);
				float alpha = resources.PlacementAnimationAlpha.Evaluate(progress);
				PLACEMENT_BLOCK.SetFloat(ID_Alpha, alpha);
				Matrix4x4 trs = Matrix4x4.TRS(animation.Pos_W + new float3(0f, scale - 1f, 0f), FastMatrix.RotateY(animation.Direction), new Vector3(scale, scale, scale));
				if (animation.BaseMeshOnly)
				{
					options.RegularRenderer.DrawMesh(animation.InternalVariant.BlueprintMeshBase, in trs, resources.BuildingPlaceAnimationMaterial, RenderCategory.Effects, PLACEMENT_BLOCK);
				}
				else
				{
					animation.InternalVariant.CombinedBlueprintMesh.Draw(options, resources.BuildingPlaceAnimationMaterial, in trs, RenderCategory.Effects, PLACEMENT_BLOCK);
				}
			}
		}
		for (int i2 = DeletionAnimations.Count - 1; i2 >= 0; i2--)
		{
			DeletionAnimation animation2 = DeletionAnimations[i2];
			float progress2 = (now - animation2.StartTime) / (DeletionAnimationDuration * animation2.AnimationDurationFactor);
			if (progress2 >= 1f)
			{
				DeletionAnimations.RemoveAt(i2);
			}
			else
			{
				float scale2 = resources.DeletionAnimationScaleFactor.Evaluate(progress2);
				float alpha2 = resources.DeletionAnimationAlpha.Evaluate(progress2);
				DELETION_BLOCK.SetFloat(ID_Alpha, alpha2);
				Matrix4x4 trs2 = Matrix4x4.TRS(animation2.Pos_W + new float3(0f, math.max(0f, scale2 - 1f), 0f), FastMatrix.RotateY(animation2.Direction), new Vector3(scale2, scale2, scale2));
				animation2.InternalVariant.CombinedBlueprintMesh.Draw(options, resources.BuildingPlaceAnimationMaterial, in trs2, RenderCategory.Effects, DELETION_BLOCK);
			}
		}
	}
}
