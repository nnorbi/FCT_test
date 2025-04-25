using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class HalfCutterMetaBuildingInternalVariant : MetaBuildingInternalVariant
{
	[Header("Overall")]
	public AnimationCurve ShapeHalvesOffset;

	public float ShapeHalvesMaxOffset = 0.025f;

	[Header("Cut")]
	public float LaserCutTime = 0.1f;

	public LOD2Mesh LaserBladeMesh;

	public AnimationCurve LaserBladeHeight;

	[Header("Dissolve")]
	public float DissolveTime = 0.9f;

	public AnimationCurve WasteAlpha;

	public AnimationCurve CollapseProgress;

	public AnimationCurve GlassShieldHeight;

	public LOD2Mesh GlassShieldMesh;

	[Header("Robot")]
	public HalfCutterRobotJoint[] RobotJoints;

	public float3 ShootPositionOffset = new float3(0f, -0.07f, 0.07f);

	private void AppendJointsToAdditionalBlueprintMeshes()
	{
		Grid.Direction direction = Grid.Direction.Bottom;
		float4x4 stackedMatrix = float4x4.RotateY(math.radians(Grid.DirectionToDegrees(direction)));
		Quaternion currentBaseRotation = FastMatrix.RotateY(direction);
		List<AdditionalBlueprintMesh> additionalBlueprintMeshes = AdditionalBlueprintMeshes.ToList();
		HalfCutterRobotJoint[] robotJoints = RobotJoints;
		for (int i = 0; i < robotJoints.Length; i++)
		{
			HalfCutterRobotJoint joint = robotJoints[i];
			currentBaseRotation = math.mul(currentBaseRotation, joint.RestRotation);
			float4x4 trs = float4x4.TRS(joint.Position, joint.RestRotation, new float3(1f, 1f, 1f));
			stackedMatrix = math.mul(stackedMatrix, trs);
			if (joint.Mesh.TryGet(0, out LODBaseMesh.CachedMesh jointMesh))
			{
				additionalBlueprintMeshes.Add(new AdditionalBlueprintMesh
				{
					Mesh = jointMesh,
					Pos = math.mul(stackedMatrix, new float4(0f, 0f, 0f, 1f)).xyz,
					Rotate = currentBaseRotation,
					Scale = new float3(1f, 1f, 1f)
				});
			}
		}
		AdditionalBlueprintMeshes = additionalBlueprintMeshes.ToArray();
	}
}
