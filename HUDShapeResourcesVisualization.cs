using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HUDShapeResourcesVisualization : HUDVisualization, IDisposable
{
	private static int INSTANCING_ID_SHAPE_UNDERLAY = Shader.PropertyToID("shape-resources-visualization::underlay");

	private readonly DrawManager DrawManager;

	public HUDShapeResourcesVisualization(DrawManager drawManager)
	{
		DrawManager = drawManager;
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawSuperChunk = (DrawHooks.DrawSuperChunkDelegate)Delegate.Combine(hooks.OnDrawSuperChunk, new DrawHooks.DrawSuperChunkDelegate(DrawSuperChunk));
	}

	public void Dispose()
	{
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawSuperChunk = (DrawHooks.DrawSuperChunkDelegate)Delegate.Remove(hooks.OnDrawSuperChunk, new DrawHooks.DrawSuperChunkDelegate(DrawSuperChunk));
	}

	public override string GetGlobalIconId()
	{
		return "visualization-shape-resources";
	}

	public override string GetTitle()
	{
		return "visualizations.shape-resources.title".tr();
	}

	public override bool IsAvailable()
	{
		return Player.Viewport.Scope == GameScope.Islands || Player.Viewport.Scope == GameScope.Overview;
	}

	protected void DrawSuperChunk(FrameDrawOptions options, MapSuperChunk chunk)
	{
		if (Alpha < 0.001f)
		{
			return;
		}
		float zoom = options.Player.Viewport.Zoom;
		float scale = 15f + math.min(zoom, 12000f) / 20f;
		scale *= Alpha;
		if (scale < 0.001f)
		{
			return;
		}
		IReadOnlyList<SuperChunkShapeResourceCluster> resources = chunk.ShapeResourceClusters;
		float spacing = scale * 0.4f;
		int maxDefinitions = 1;
		if (zoom < 2000f)
		{
			maxDefinitions = 3;
		}
		else if (zoom < 5000f)
		{
			maxDefinitions = 2;
		}
		Material shapeMaterial = Globals.Resources.ShapeMaterial;
		for (int i = 0; i < resources.Count; i++)
		{
			SuperChunkShapeResourceCluster cluster = resources[i];
			int definitionCount = math.min(maxDefinitions, cluster.Definitions.Length);
			for (int j = 0; j < definitionCount; j++)
			{
				ShapeDefinition definition = cluster.Definitions[j];
				float3 tile_W = cluster.Center_GC.ToCenter_W() + new float3(((float)j - ((float)definitionCount - 1f) / 2f) * spacing, 4f, 0f);
				options.ShapeInstanceManager.AddInstanceSlow(definition.GetMesh(), shapeMaterial, FastMatrix.TranslateScale(in tile_W, new float3(scale)));
				options.Draw3DPlaneWithMaterialInstanced(INSTANCING_ID_SHAPE_UNDERLAY, options.Theme.BaseResources.UXShapeResourceVisualizationUnderlayMaterial, FastMatrix.TranslateScale(in tile_W, (float3)(scale * 0.5f)));
			}
		}
	}
}
