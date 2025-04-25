using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpaceThemeShapeAsteroidInstancedVisualization : ISpaceThemeShapeAsteroidVisualization
{
	private readonly float[] LODDistances;

	private readonly GlobalChunkCoordinate Pos_GC;

	private readonly List<(LODBaseMesh, Matrix4x4)> Instances = new List<(LODBaseMesh, Matrix4x4)>();

	private Material SharedMaterial;

	public SpaceThemeShapeAsteroidInstancedVisualization(GlobalChunkCoordinate pos_GC, Material sharedMaterial, params float[] lodDistances)
	{
		SharedMaterial = sharedMaterial;
		LODDistances = lodDistances ?? throw new ArgumentNullException("lodDistances");
		Pos_GC = pos_GC;
		if (lodDistances.Length == 0)
		{
			throw new ArgumentException("At least 1 LOD distance required.", "lodDistances");
		}
	}

	public void Add(LODBaseMesh mesh, Matrix4x4 transform)
	{
		Instances.Add((mesh, transform));
	}

	public void Draw(FrameDrawOptions options)
	{
		InstancedMeshManager instancedMeshManager = options.ShapeMapResourcesInstanceManager;
		float3 resourcePosition_W = Pos_GC.ToCenter_W();
		float cameraDistanceSq = math.distancesq(resourcePosition_W, options.CameraPosition_W);
		int lod = LODDistances.Length;
		while (!(cameraDistanceSq > LODDistances[lod - 1] * LODDistances[lod - 1]) && --lod > 0)
		{
		}
		GraphicsBackgroundDetails value = Globals.Settings.Graphics.BackgroundDetails.Value;
		if (1 == 0)
		{
		}
		int num = value switch
		{
			GraphicsBackgroundDetails.Minimum => LODDistances.Length + 1, 
			GraphicsBackgroundDetails.Low => 2, 
			GraphicsBackgroundDetails.Medium => 1, 
			GraphicsBackgroundDetails.High => 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		int lodOffset = num;
		lod = math.min(lod + lodOffset, LODDistances.Length);
		foreach (var instance in Instances)
		{
			var (lodMesh, transform) = instance;
			if (lodMesh.TryGet(lod, out LODBaseMesh.CachedMesh cachedMesh))
			{
				instancedMeshManager.AddInstance(cachedMesh, SharedMaterial, in transform);
			}
		}
	}
}
