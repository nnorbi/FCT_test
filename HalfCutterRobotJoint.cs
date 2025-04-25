using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct HalfCutterRobotJoint
{
	public LOD2Mesh Mesh;

	public float3 Position;

	public Quaternion RestRotation;

	public Quaternion AlignedRotation;

	public AnimationCurve RotationInterpolation;
}
