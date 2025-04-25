using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public static class FastMatrix
{
	private static Matrix4x4[] ROTATION_MATRICES_3D_PRECACHED = new Matrix4x4[4]
	{
		Matrix4x4.Rotate(RotateYAngle(0f)),
		Matrix4x4.Rotate(RotateYAngle(90f)),
		Matrix4x4.Rotate(RotateYAngle(180f)),
		Matrix4x4.Rotate(RotateYAngle(270f))
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix4x4 Translate(in float3 translation)
	{
		return new Matrix4x4
		{
			m00 = 1f,
			m01 = 0f,
			m02 = 0f,
			m03 = translation.x,
			m10 = 0f,
			m11 = 1f,
			m12 = 0f,
			m13 = translation.y,
			m20 = 0f,
			m21 = 0f,
			m22 = 1f,
			m23 = translation.z,
			m30 = 0f,
			m31 = 0f,
			m32 = 0f,
			m33 = 1f
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix4x4 TranslateRotate(in float3 translation, Grid.Direction direction)
	{
		if (direction == Grid.Direction.Right)
		{
			return new Matrix4x4
			{
				m00 = 1f,
				m01 = 0f,
				m02 = 0f,
				m03 = translation.x,
				m10 = 0f,
				m11 = 1f,
				m12 = 0f,
				m13 = translation.y,
				m20 = 0f,
				m21 = 0f,
				m22 = 1f,
				m23 = translation.z,
				m30 = 0f,
				m31 = 0f,
				m32 = 0f,
				m33 = 1f
			};
		}
		ref Matrix4x4 rotationMatrix = ref ROTATION_MATRICES_3D_PRECACHED[(int)direction];
		return new Matrix4x4
		{
			m00 = rotationMatrix.m00,
			m01 = rotationMatrix.m01,
			m02 = rotationMatrix.m02,
			m03 = translation.x,
			m10 = rotationMatrix.m10,
			m11 = rotationMatrix.m11,
			m12 = rotationMatrix.m12,
			m13 = translation.y,
			m20 = rotationMatrix.m20,
			m21 = rotationMatrix.m21,
			m22 = rotationMatrix.m22,
			m23 = translation.z,
			m30 = 0f,
			m31 = 0f,
			m32 = 0f,
			m33 = 1f
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix4x4 TranslateRotateScale(in float3 translation, Grid.Direction direction, in float3 scale)
	{
		if (direction == Grid.Direction.Right)
		{
			return new Matrix4x4
			{
				m00 = scale.x,
				m01 = 0f,
				m02 = 0f,
				m03 = translation.x,
				m10 = 0f,
				m11 = scale.y,
				m12 = 0f,
				m13 = translation.y,
				m20 = 0f,
				m21 = 0f,
				m22 = scale.z,
				m23 = translation.z,
				m30 = 0f,
				m31 = 0f,
				m32 = 0f,
				m33 = 1f
			};
		}
		ref Matrix4x4 rotationMatrix = ref ROTATION_MATRICES_3D_PRECACHED[(int)direction];
		return new Matrix4x4
		{
			m00 = rotationMatrix.m00,
			m01 = rotationMatrix.m01,
			m02 = rotationMatrix.m02,
			m03 = translation.x,
			m10 = rotationMatrix.m10,
			m11 = rotationMatrix.m11,
			m12 = rotationMatrix.m12,
			m13 = translation.y,
			m20 = rotationMatrix.m20,
			m21 = rotationMatrix.m21,
			m22 = rotationMatrix.m22,
			m23 = translation.z,
			m30 = 0f,
			m31 = 0f,
			m32 = 0f,
			m33 = 1f
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix4x4 TranslateRotateDegrees(in float3 translation, float rotationDegreesY)
	{
		if (rotationDegreesY < 0.05f && rotationDegreesY > -0.05f)
		{
			return new Matrix4x4
			{
				m00 = 1f,
				m01 = 0f,
				m02 = 0f,
				m03 = translation.x,
				m10 = 0f,
				m11 = 1f,
				m12 = 0f,
				m13 = translation.y,
				m20 = 0f,
				m21 = 0f,
				m22 = 1f,
				m23 = translation.z,
				m30 = 0f,
				m31 = 0f,
				m32 = 0f,
				m33 = 1f
			};
		}
		Matrix4x4 rotationMatrix = Matrix4x4.Rotate(RotateYAngle(rotationDegreesY));
		return new Matrix4x4
		{
			m00 = rotationMatrix.m00,
			m01 = rotationMatrix.m01,
			m02 = rotationMatrix.m02,
			m03 = translation.x,
			m10 = rotationMatrix.m10,
			m11 = rotationMatrix.m11,
			m12 = rotationMatrix.m12,
			m13 = translation.y,
			m20 = rotationMatrix.m20,
			m21 = rotationMatrix.m21,
			m22 = rotationMatrix.m22,
			m23 = translation.z,
			m30 = 0f,
			m31 = 0f,
			m32 = 0f,
			m33 = 1f
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix4x4 TranslateScale(in float3 translation, in float3 scale)
	{
		return new Matrix4x4
		{
			m00 = scale.x,
			m01 = 0f,
			m02 = 0f,
			m03 = translation.x,
			m10 = 0f,
			m11 = scale.y,
			m12 = 0f,
			m13 = translation.y,
			m20 = 0f,
			m21 = 0f,
			m22 = scale.z,
			m23 = translation.z,
			m30 = 0f,
			m31 = 0f,
			m32 = 0f,
			m33 = 1f
		};
	}

	public static float4x4 TranslateScale_math(in float3 translation, in float3 scale)
	{
		return new float4x4(new float4(scale.x, 0f, 0f, 0f), new float4(0f, scale.y, 0f, 0f), new float4(0f, 0f, scale.z, 0f), new float4(translation, 1f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Quaternion RotateYAngle(float rotationDegrees)
	{
		if (rotationDegrees == 0f)
		{
			return Quaternion.identity;
		}
		float rad = 0.5f * math.radians(rotationDegrees);
		return new Quaternion(0f, math.sin(rad), 0f, math.cos(rad));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static quaternion RotateYAngle_math(float rotationDegrees)
	{
		if (rotationDegrees == 0f)
		{
			return quaternion.identity;
		}
		float rad = 0.5f * math.radians(rotationDegrees);
		return new quaternion(0f, math.sin(rad), 0f, math.cos(rad));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Quaternion RotateY(Grid.Direction direction)
	{
		if (direction == Grid.Direction.Right)
		{
			return Quaternion.identity;
		}
		float rad = 0.5f * math.radians((float)direction * 90f);
		return new Quaternion(0f, math.sin(rad), 0f, math.cos(rad));
	}
}
