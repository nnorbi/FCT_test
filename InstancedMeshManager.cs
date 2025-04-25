using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InstancedMeshManager
{
	protected struct RendererReference
	{
		public object Mesh;

		public Material Material;

		public string PropertyBlockKey;

		public bool CastsShadows;

		public bool ReceiveShadows;

		public InstancedMeshRenderer Renderer;
	}

	protected const int INITIAL_CAPACITY = 500;

	protected const int CAPACITY_INCREASE_ON_EXCEED = 100;

	protected const bool CHECKS_ENABLED = false;

	protected int Renderers_ListCount = 0;

	protected RendererReference[] Renderers_ListRaw = new RendererReference[500];

	protected Dictionary<int, RendererReference> Renderers_Dictionary = new Dictionary<int, RendererReference>();

	private Camera Camera;

	private int Layer;

	private RenderCategory Category;

	public InstancedMeshManager(Camera camera, int layer, RenderCategory category)
	{
		Camera = camera;
		Layer = layer;
		Category = category;
	}

	public void AddInstanceSlow(Mesh mesh, Material material, in Matrix4x4 transform, string propertyBlockKey = null, MaterialPropertyBlock propertyBlock = null, bool castShadows = false, bool receiveShadows = false)
	{
		bool flag = false;
		for (int i = 0; i < Renderers_ListCount; i++)
		{
			RendererReference rendererRef = Renderers_ListRaw[i];
			if (rendererRef.Mesh == mesh && (object)rendererRef.Material == material && string.Equals(rendererRef.PropertyBlockKey, propertyBlockKey) && rendererRef.CastsShadows == castShadows && rendererRef.ReceiveShadows == receiveShadows)
			{
				InstancedMeshRenderer renderer = rendererRef.Renderer;
				bool flag2 = false;
				renderer.AddInstance(in transform, mesh, material, castShadows, receiveShadows);
				return;
			}
		}
		InstancedMeshRenderer newRenderer = new InstancedMeshRenderer(mesh, material, propertyBlock, Camera, Layer, castShadows, receiveShadows, Category);
		Renderers_ListCount++;
		if (Renderers_ListCount > Renderers_ListRaw.Length)
		{
			Array.Resize(ref Renderers_ListRaw, Renderers_ListRaw.Length + 100);
		}
		Renderers_ListRaw[Renderers_ListCount - 1] = new RendererReference
		{
			Renderer = newRenderer,
			PropertyBlockKey = propertyBlockKey,
			Mesh = mesh,
			Material = material,
			CastsShadows = castShadows,
			ReceiveShadows = receiveShadows
		};
		newRenderer.AddInstance(in transform, mesh, material, castShadows, receiveShadows);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddInstance(LODBaseMesh.CachedMesh mesh, Material material, in Matrix4x4 transform, MaterialPropertyBlock propertyBlock = null, bool castShadows = false, bool receiveShadows = false)
	{
		AddInstance(mesh.InstancingID, mesh.Mesh, material, in transform, propertyBlock, castShadows, receiveShadows);
	}

	public void AddInstance(int key, Mesh mesh, Material material, in Matrix4x4 transform, MaterialPropertyBlock propertyBlock = null, bool castShadows = false, bool receiveShadows = false)
	{
		bool flag = false;
		if (Renderers_Dictionary.TryGetValue(key, out var rendererReference))
		{
			InstancedMeshRenderer renderer = rendererReference.Renderer;
			bool flag2 = false;
		}
		else
		{
			rendererReference = new RendererReference
			{
				Renderer = new InstancedMeshRenderer(mesh, material, propertyBlock, Camera, Layer, castShadows, receiveShadows, Category),
				Mesh = mesh,
				Material = material,
				CastsShadows = castShadows,
				ReceiveShadows = receiveShadows
			};
			Renderers_Dictionary.Add(key, rendererReference);
		}
		rendererReference.Renderer.AddInstance(in transform, mesh, material, castShadows, receiveShadows);
	}

	public void DrawAll(FrameDrawOptions options)
	{
		for (int i = 0; i < Renderers_ListCount; i++)
		{
			Renderers_ListRaw[i].Renderer.Draw(options);
		}
		foreach (KeyValuePair<int, RendererReference> item in Renderers_Dictionary)
		{
			item.Value.Renderer.Draw(options);
		}
	}

	public void GarbageCollect()
	{
		for (int i = Renderers_ListCount - 1; i >= 0; i--)
		{
			RendererReference renderer = Renderers_ListRaw[i];
			renderer.Renderer.GarbageCollect();
			if (renderer.Renderer.BlockCount == 0 || renderer.Mesh == null || renderer.Material == null || renderer.Renderer.Material == null || renderer.Renderer.Mesh == null)
			{
				if (i == Renderers_ListCount - 1)
				{
					Renderers_ListCount--;
				}
				else
				{
					Renderers_ListRaw[i] = Renderers_ListRaw[Renderers_ListCount - 1];
					Renderers_ListCount--;
				}
			}
		}
		List<int> deleteIndices = new List<int>();
		foreach (KeyValuePair<int, RendererReference> entry in Renderers_Dictionary)
		{
			RendererReference renderer2 = entry.Value;
			renderer2.Renderer.GarbageCollect();
			if (renderer2.Renderer.BlockCount == 0 || renderer2.Mesh == null || renderer2.Material == null || renderer2.Renderer.Material == null || renderer2.Renderer.Mesh == null)
			{
				deleteIndices.Add(entry.Key);
			}
		}
		foreach (int key in deleteIndices)
		{
			Renderers_Dictionary.Remove(key);
		}
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("instancing.stats", delegate(DebugConsole.CommandContext ctx)
		{
			int num = 0;
			ctx.Output("There are " + Renderers_ListCount + " instanced renderers");
			for (int i = 0; i < Renderers_ListCount; i++)
			{
				RendererReference rendererReference = Renderers_ListRaw[i];
				int num2 = rendererReference.Renderer.ComputeMaxInstanceHistoryCount();
				ctx.Output("   " + num2 + " x " + ((Mesh)rendererReference.Mesh).name + " ( " + rendererReference.PropertyBlockKey + " )  : " + rendererReference.Renderer.BlockCount + " blocks");
				num += num2;
			}
			ctx.Output("There are " + Renderers_Dictionary.Count + " indexed instanced renderers");
			foreach (KeyValuePair<int, RendererReference> current in Renderers_Dictionary)
			{
				RendererReference value = current.Value;
				int num3 = value.Renderer.ComputeMaxInstanceHistoryCount();
				ctx.Output("   [#" + current.Key + "] " + num3 + " x " + ((Mesh)value.Mesh).name + " ( " + value.PropertyBlockKey + " )  : " + value.Renderer.BlockCount + " blocks");
				num += num3;
			}
			ctx.Output("");
			ctx.Output("Total instances drawn: " + num);
		});
		console.Register("instancing.set-max-size", new DebugConsole.IntOption("size", 1, 1023), delegate(DebugConsole.CommandContext ctx)
		{
			int num = (InstancedMeshRenderer.MAX_SIZE = ctx.GetInt(0));
			Renderers_ListCount = 0;
			ctx.Output("Instance size set to " + num);
		});
		console.Register("instancing.set-threshold", new DebugConsole.IntOption("threshold", 1, 1023), delegate(DebugConsole.CommandContext ctx)
		{
			int num = (InstancedMeshRenderer.THRESHOLD_RENDER_INSTANCED = ctx.GetInt(0));
			Renderers_ListCount = 0;
			ctx.Output("Instance threshold set to " + num + " instances");
		});
	}
}
