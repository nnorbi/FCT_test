#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class MapSuperChunk
{
	public static string FILENAME_CHUNK = "chunks/%x_%y.bin";

	public SuperChunkCoordinate Origin_SC;

	public GlobalChunkCoordinate Origin_GC;

	public Bounds Bounds_W;

	protected GameMap Map;

	public List<ShapeResourceSource> ShapeResources = new List<ShapeResourceSource>();

	public List<FluidResourceSource> FluidResources = new List<FluidResourceSource>();

	private List<SuperChunkShapeResourceCluster> _ShapeResourceClusters = new List<SuperChunkShapeResourceCluster>();

	public Dictionary<GlobalChunkCoordinate, ResourceSource> ResourcesLookup_GC = new Dictionary<GlobalChunkCoordinate, ResourceSource>();

	protected HashSet<Island> _Islands = new HashSet<Island>();

	public IReadOnlyList<SuperChunkShapeResourceCluster> ShapeResourceClusters => _ShapeResourceClusters;

	public IReadOnlyCollection<Island> Islands => _Islands;

	public bool ContainsIslands => _Islands.Count > 0;

	public static MapSuperChunk Deserialize(SavegameBlobReader reader)
	{
		return null;
	}

	public MapSuperChunk(GameMap map, SuperChunkCoordinate origin_SC)
	{
		Map = map;
		Origin_SC = origin_SC;
		Origin_GC = Origin_SC.ToOrigin_GC();
		Bounds_W = Singleton<GameCore>.G.Theme.ComputeSuperChunkBounds(this);
	}

	public ResourceSource GetResourceSource_GC(GlobalChunkCoordinate chunk_GC)
	{
		if (ResourcesLookup_GC.TryGetValue(chunk_GC, out var result))
		{
			return result;
		}
		return null;
	}

	public void RegisterIsland(Island island)
	{
		if (!_Islands.Add(island))
		{
			throw new Exception("Super chunk already contains island " + island.Descriptor.ToString());
		}
	}

	public void UnregisterIsland(Island island)
	{
		if (!_Islands.Remove(island))
		{
			throw new Exception("Super chunk does not contain island " + island.Descriptor.ToString());
		}
	}

	public void InitializeResources()
	{
		ShapeManager shapeManager = Singleton<GameCore>.G.Shapes;
		foreach (IResourceSourceData resourceDescriptor in Map.Generator.Generate(Origin_SC))
		{
			if (resourceDescriptor is ShapeResourceClusterData cluster)
			{
				_ShapeResourceClusters.Add(new SuperChunkShapeResourceCluster((from d in cluster.ShapeResources.Select((ShapeResourceSourceData r) => r.Definition).Distinct()
					select shapeManager.GetDefinitionByHash(d)).ToArray(), cluster.Center_GC));
				Debug.Assert(cluster.Center_GC.To_SC() == Origin_SC);
			}
			ResourceSource source = resourceDescriptor.Create();
			AddResourceSource(source);
		}
	}

	protected void AddResourceSource(ResourceSource resource)
	{
		if (!TryRegisterResource(resource))
		{
			return;
		}
		if (resource is ShapeResourceSource shapeResource)
		{
			ShapeResources.Add(shapeResource);
			return;
		}
		if (resource is FluidResourceSource fluidResource)
		{
			FluidResources.Add(fluidResource);
			return;
		}
		throw new Exception("Unknown resource type");
	}

	protected bool TryRegisterResource(ResourceSource source)
	{
		GlobalChunkCoordinate[] tiles_GC = source.Tiles_GC;
		for (int i = 0; i < tiles_GC.Length; i++)
		{
			GlobalChunkCoordinate tile_GC = tiles_GC[i];
			if (ResourcesLookup_GC.ContainsKey(tile_GC))
			{
				return false;
			}
			SuperChunkCoordinate tile_SC = tile_GC.To_SC();
			if (tile_SC != Origin_SC)
			{
				return false;
			}
		}
		GlobalChunkCoordinate[] tiles_GC2 = source.Tiles_GC;
		foreach (GlobalChunkCoordinate tile_GC2 in tiles_GC2)
		{
			ResourcesLookup_GC.Add(tile_GC2, source);
		}
		return true;
	}

	public void Serialize(SavegameBlobWriter handle)
	{
		if (!ContainsIslands)
		{
			return;
		}
		handle.Write(FILENAME_CHUNK.Replace("%x", Origin_SC.x.ToString() ?? "").Replace("%y", Origin_SC.y.ToString() ?? ""), delegate(BinaryStringLUTSerializationVisitor serializer)
		{
			serializer.WriteInt_4(Origin_SC.x);
			serializer.WriteInt_4(Origin_SC.y);
			serializer.Checkpoint("ShapeResources", always: true);
			serializer.WriteInt_4(ShapeResources.Count);
			foreach (ShapeResourceSource current in ShapeResources)
			{
				serializer.WriteInt_4(current.Definitions.Length);
				ShapeDefinition[] definitions = current.Definitions;
				foreach (ShapeDefinition shapeDefinition in definitions)
				{
					serializer.WriteString_4(shapeDefinition.Hash);
				}
			}
			serializer.Checkpoint("FluidResources", always: true);
			serializer.WriteInt_4(FluidResources.Count);
			foreach (FluidResourceSource current2 in FluidResources)
			{
				serializer.WriteString_4(current2.Fluid.Serialize());
			}
		});
	}

	public void OnGameCleanup()
	{
		foreach (ResourceSource resource in ResourcesLookup_GC.Values)
		{
			resource.OnGameCleanup();
		}
	}

	public void OnGameDraw(FrameDrawOptions options)
	{
		float3 cameraPosition_W = options.CameraPosition_W;
		if (options.DrawShapeResources)
		{
			int shapeResourceCount = ShapeResources.Count;
			for (int i = 0; i < shapeResourceCount; i++)
			{
				ShapeResourceSource source = ShapeResources[i];
				float3 sourcePosition_W = source.Origin_GC.ToCenter_W();
				if (!(math.distancesq(cameraPosition_W, sourcePosition_W) > 30250000f) && GeometryUtility.TestPlanesAABB(options.CameraPlanes, source.Bounds_W))
				{
					source.OnGameDraw(options);
					options.Theme.Draw_ShapeResourceSource(options, source);
				}
			}
		}
		if (!options.DrawFluidResources)
		{
			return;
		}
		int fluidResourceCount = FluidResources.Count;
		for (int j = 0; j < fluidResourceCount; j++)
		{
			FluidResourceSource source2 = FluidResources[j];
			float3 sourcePosition_W2 = source2.Origin_GC.ToCenter_W();
			if (!(math.distancesq(cameraPosition_W, sourcePosition_W2) > 30250000f) && GeometryUtility.TestPlanesAABB(options.CameraPlanes, source2.Bounds_W))
			{
				source2.OnGameDraw(options);
				options.Theme.Draw_FluidResourceSource(options, source2);
			}
		}
	}
}
