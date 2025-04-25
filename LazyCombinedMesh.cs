using System;
using System.Collections.Generic;
using UnityEngine;

public class LazyCombinedMesh : IExpiringResource
{
	protected static bool SHOW_UNCOMBINED;

	public static int STATS_MESHES_COMBINED;

	protected List<CombineInstance> SourceInstances = null;

	protected CombinedMesh ResultMesh = new CombinedMesh();

	protected bool ResultGenerated = false;

	protected bool AllowCombine;

	public bool NeedsGeneration => SourceInstances == null;

	public double LastUsed { get; protected set; } = -1000.0;

	public float ExpireAfter => 30f;

	public string Name
	{
		get
		{
			if (SourceInstances == null)
			{
				return "lazy(not-generated)";
			}
			if (SourceInstances.Count == 0)
			{
				return "lazy(empty)";
			}
			return "lazy(" + SourceInstances[0].mesh.name + ", +" + (SourceInstances.Count - 1) + ")";
		}
	}

	public static void RegisterCommands(DebugConsole console)
	{
		console.Register("combined-mesh.set-visualize-uncombined", new DebugConsole.BoolOption("enabled"), delegate(DebugConsole.CommandContext ctx)
		{
			SHOW_UNCOMBINED = ctx.GetBool(0);
			ctx.Output("Visualize uncombined meshes enabled: " + SHOW_UNCOMBINED);
		});
	}

	public LazyCombinedMesh(bool allowCombine = true)
	{
		AllowCombine = allowCombine;
	}

	public void Hook_OnExpire()
	{
		Clear(unregister: false);
	}

	public void ClearAndSetAllowCombine(bool allowCombine)
	{
		Clear();
		AllowCombine = allowCombine;
	}

	public void Clear(bool unregister = true)
	{
		if (SourceInstances == null)
		{
			return;
		}
		if (SourceInstances.Count == 0)
		{
			SourceInstances = null;
			return;
		}
		if (ResultGenerated && unregister)
		{
			Singleton<GameCore>.G.ExpiringResources.Unregister(this);
		}
		ResultGenerated = false;
		ResultMesh.Clear();
		SourceInstances.Clear();
		SourceInstances = null;
	}

	public void SetSourceInstances(List<CombineInstance> sourceInstances)
	{
		Clear();
		SourceInstances = sourceInstances;
	}

	protected void PrepareMeshForDraw(FrameDrawOptions options)
	{
		if (!ResultGenerated && AllowCombine && options.ConsumeLargeOperation())
		{
			Generate();
		}
		if (!AllowCombine && ResultGenerated)
		{
			throw new Exception("Not allowed to change combine mode after creation");
		}
	}

	public void Draw(FrameDrawOptions options, Material material, RenderCategory category, InstancedMeshManager fallbackInstanceManager, MaterialPropertyBlock propertyBlock = null, bool castShadows = false, bool receiveShadows = false)
	{
		LastUsed = Singleton<GameCore>.G.SimulationSpeed.CurrentRealtime;
		if (SourceInstances == null || SourceInstances.Count == 0)
		{
			return;
		}
		PrepareMeshForDraw(options);
		if (ResultGenerated)
		{
			CombinedMesh resultMesh = ResultMesh;
			Material material2 = (SHOW_UNCOMBINED ? Globals.Resources.DebugIndicatorMeshCombinedShader : material);
			Matrix4x4 transform = Matrix4x4.identity;
			bool castShadows2 = castShadows;
			bool receiveShadows2 = receiveShadows;
			resultMesh.Draw(options, material2, in transform, category, propertyBlock, castShadows2, receiveShadows2);
			return;
		}
		int meshCount = SourceInstances.Count;
		for (int i = 0; i < meshCount; i++)
		{
			Mesh mesh = SourceInstances[i].mesh;
			Material material3 = (SHOW_UNCOMBINED ? Globals.Resources.DebugIndicatorMeshNotCombinedShader : material);
			Matrix4x4 transform = SourceInstances[i].transform;
			bool receiveShadows2 = receiveShadows;
			bool castShadows2 = castShadows;
			fallbackInstanceManager.AddInstanceSlow(mesh, material3, in transform, null, propertyBlock, castShadows2, receiveShadows2);
		}
	}

	public void DrawSlow(FrameDrawOptions options, Material material, in Matrix4x4 transform, RenderCategory category, InstancedMeshManager fallbackInstanceManager, MaterialPropertyBlock propertyBlock = null, bool castShadows = false, bool receiveShadows = false)
	{
		LastUsed = Singleton<GameCore>.G.SimulationSpeed.CurrentRealtime;
		if (SourceInstances == null || SourceInstances.Count == 0)
		{
			return;
		}
		PrepareMeshForDraw(options);
		if (ResultGenerated)
		{
			ResultMesh.Draw(options, SHOW_UNCOMBINED ? Globals.Resources.DebugIndicatorMeshCombinedShader : material, in transform, category, propertyBlock, castShadows, receiveShadows);
			return;
		}
		int meshCount = SourceInstances.Count;
		for (int i = 0; i < meshCount; i++)
		{
			Mesh mesh = SourceInstances[i].mesh;
			Material material2 = (SHOW_UNCOMBINED ? Globals.Resources.DebugIndicatorMeshNotCombinedShader : material);
			Matrix4x4 transform2 = transform * SourceInstances[i].transform;
			bool receiveShadows2 = receiveShadows;
			fallbackInstanceManager.AddInstanceSlow(mesh, material2, in transform2, null, null, castShadows, receiveShadows2);
		}
	}

	protected void Generate()
	{
		if (ResultGenerated)
		{
			throw new Exception("Combined mesh already generated");
		}
		if (SourceInstances == null || SourceInstances.Count == 0)
		{
			throw new Exception("Combined mesh empty");
		}
		ResultMesh.Clear();
		int currentVertexCount = 0;
		int totalVertexCount = 0;
		List<CombineInstance> currentPayload = new List<CombineInstance>();
		int instanceCount = SourceInstances.Count;
		for (int i = 0; i < instanceCount; i++)
		{
			CombineInstance instance = SourceInstances[i];
			if (instance.mesh == null)
			{
				Debug.LogError("Got empty mesh in lazy combine instance @" + i + ": " + (instance.mesh == null));
				continue;
			}
			int vertexCount = instance.mesh.vertexCount;
			totalVertexCount += vertexCount;
			if (currentVertexCount + vertexCount > 65535)
			{
				Mesh mesh = new Mesh();
				mesh.CombineMeshes(currentPayload.ToArray(), mergeSubMeshes: true, useMatrices: true, hasLightmapData: false);
				ResultMesh.Add(mesh);
				currentVertexCount = 0;
				currentPayload.Clear();
			}
			currentPayload.Add(instance);
			currentVertexCount += vertexCount;
		}
		if (currentPayload.Count > 0)
		{
			Mesh mesh2 = new Mesh();
			mesh2.CombineMeshes(currentPayload.ToArray(), mergeSubMeshes: true, useMatrices: true, hasLightmapData: false);
			ResultMesh.Add(mesh2);
			STATS_MESHES_COMBINED++;
		}
		ResultGenerated = true;
		Singleton<GameCore>.G.ExpiringResources.Register(this);
	}
}
