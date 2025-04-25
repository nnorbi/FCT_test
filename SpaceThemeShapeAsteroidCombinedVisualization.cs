using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpaceThemeShapeAsteroidCombinedVisualization : ISpaceThemeShapeAsteroidVisualization
{
	private readonly float[] LODDistances;

	private readonly GlobalChunkCoordinate Pos_GC;

	private readonly List<(LODBaseMesh, Matrix4x4)> Instances = new List<(LODBaseMesh, Matrix4x4)>();

	private Material SharedMaterial;

	private bool Generated = false;

	private ExpiringCombinedMesh[] Meshes;

	public SpaceThemeShapeAsteroidCombinedVisualization(GlobalChunkCoordinate pos_GC, Material sharedMaterial, params float[] lodDistances)
	{
		if (lodDistances.Length == 0)
		{
			throw new ArgumentException("At least 1 LOD distance required.", "lodDistances");
		}
		SharedMaterial = sharedMaterial;
		LODDistances = lodDistances;
		Meshes = new ExpiringCombinedMesh[lodDistances.Length + 1];
		Pos_GC = pos_GC;
		for (int i = 0; i < Meshes.Length; i++)
		{
			Meshes[i] = new ExpiringCombinedMesh();
		}
	}

	public void Add(LODBaseMesh mesh, Matrix4x4 transform)
	{
		if (Generated)
		{
			throw new Exception("Can't add meshes dynamically after generation already (not supported)");
		}
		Instances.Add((mesh, transform));
	}

	public void Draw(FrameDrawOptions options)
	{
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
			GraphicsBackgroundDetails.Minimum => Meshes.Length, 
			GraphicsBackgroundDetails.Low => 2, 
			GraphicsBackgroundDetails.Medium => 1, 
			GraphicsBackgroundDetails.High => 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		int lodOffset = num;
		lod = math.min(lod + lodOffset, Meshes.Length - 1);
		CombinedMesh mesh = Generate(lod).GetMeshAndMarkUsed();
		mesh.Draw(options, SharedMaterial, RenderCategory.ShapeMapResources, null, castShadows: false, (GraphicsShadowQuality)Globals.Settings.Graphics.ShadowQuality >= GraphicsShadowQuality.Extreme);
	}

	private ExpiringCombinedMesh Generate(int lod)
	{
		ExpiringCombinedMesh mesh = Meshes[lod];
		if (mesh.HasMesh)
		{
			return mesh;
		}
		MeshBuilder generator = new MeshBuilder(lod);
		foreach (var instance in Instances)
		{
			var (lodMesh, transform) = instance;
			generator.AddTRS(lodMesh, in transform);
		}
		CombinedMesh target = null;
		generator.Generate(ref target);
		mesh.SetMesh(target);
		Generated = true;
		return mesh;
	}
}
