#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Collections.Scoped;
using Unity.Mathematics;
using UnityEngine;

public class DefaultMapGenerator : IMapGenerator
{
	private static readonly ChunkDirection[] Directions = new ChunkDirection[4]
	{
		ChunkDirection.North,
		ChunkDirection.East,
		ChunkDirection.South,
		ChunkDirection.West
	};

	protected GameModeConfig Config;

	protected DefaultMapGeneratorData Data;

	protected static DefaultMapGeneratorData.LinkedSubShape PickRandomShape(IReadOnlyList<DefaultMapGeneratorData.LinkedSubShape> availableShapes, ConsistentRandom rng)
	{
		int sum = availableShapes.Sum((DefaultMapGeneratorData.LinkedSubShape s) => s.RarityScore);
		int index = rng.Next(0, sum);
		foreach (DefaultMapGeneratorData.LinkedSubShape shape in availableShapes)
		{
			index -= shape.RarityScore;
			if (index < 0)
			{
				return shape;
			}
		}
		Debug.LogError("Missing shape score @ " + index);
		return availableShapes[0];
	}

	protected static ShapeResourceClusterData GenerateShapeCluster(IEnumerable<string> clusterData, GlobalChunkCoordinate clusterPosition_GC, int ring, ConsistentRandom rng, Func<GlobalChunkCoordinate, bool> isAvailable)
	{
		if (!isAvailable(clusterPosition_GC))
		{
			throw new ArgumentException("Initial cluster position must be available!");
		}
		ScopedList<ShapeResourceSourceData> resources = ScopedList<ShapeResourceSourceData>.Get();
		HashSet<ChunkDirection> usedPositions;
		try
		{
			usedPositions = new HashSet<ChunkDirection>();
			using ScopedList<string> hashes = ScopedList<string>.Get(clusterData);
			hashes.Reverse();
			if (1 == 0)
			{
			}
			int num = ring switch
			{
				0 => 2, 
				1 => 3, 
				2 => 5, 
				3 => 8, 
				_ => 13, 
			};
			if (1 == 0)
			{
			}
			int minChunkCount = num;
			if (1 == 0)
			{
			}
			num = ring switch
			{
				0 => 5, 
				1 => 8, 
				2 => 13, 
				3 => 21, 
				_ => 34, 
			};
			if (1 == 0)
			{
			}
			int maxChunkCount = num;
			int chunkCount = rng.Next(minChunkCount, maxChunkCount + 1);
			int slotCount = (1 << hashes.Count) - 1;
			int hashIndex = 0;
			for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
			{
				int slotIndex = chunkIndex * slotCount / chunkCount;
				Debug.Assert(slotIndex < slotCount);
				if (slotIndex + 1 == 2 << hashIndex)
				{
					hashIndex++;
				}
				ChunkDirection pos_LC = ((slotCount > 1 && slotIndex == slotCount - 1) ? FindDisconnectedChunkPosition() : FindAdjacentChunkPosition());
				usedPositions.Add(pos_LC);
				resources.Add(new ShapeResourceSourceData(pos_LC, hashes[hashIndex]));
			}
			Debug.Assert(hashIndex == hashes.Count - 1);
			Debug.Assert(usedPositions.Count == chunkCount);
			Debug.Assert(resources.Count == chunkCount);
			return new ShapeResourceClusterData(resources, clusterPosition_GC);
		}
		finally
		{
			if (resources != null)
			{
				((IDisposable)resources).Dispose();
			}
		}
		ChunkDirection FindAdjacentChunkPosition()
		{
			if (resources.Count == 0)
			{
				return ChunkDirection.Zero;
			}
			int tries = 0;
			ChunkDirection chunkPosition_LC;
			do
			{
				ScopedList<ShapeResourceSourceData> scopedList = resources;
				chunkPosition_LC = scopedList[scopedList.Count - 1].Offset_LC;
				do
				{
					chunkPosition_LC += Directions[rng.Next(0, Directions.Length)];
				}
				while (usedPositions.Contains(chunkPosition_LC));
			}
			while (++tries <= 16 && (HasTooFewNeighbours(chunkPosition_LC, usedPositions, 2) || CreatesExceedingEdgeLength(chunkPosition_LC, usedPositions, 2) || !isAvailable(clusterPosition_GC + chunkPosition_LC)));
			return chunkPosition_LC;
		}
		ChunkDirection FindDisconnectedChunkPosition()
		{
			Debug.Assert(resources.Count > 0);
			int tries = 0;
			ChunkDirection chunkPosition_LC;
			do
			{
				chunkPosition_LC = resources[rng.Next(0, resources.Count)].Offset_LC;
				do
				{
					chunkPosition_LC += 2 * Directions[rng.Next(0, Directions.Length)];
					chunkPosition_LC += Directions[rng.Next(0, Directions.Length)];
				}
				while (usedPositions.Contains(chunkPosition_LC));
				if (++tries > 16)
				{
					Debug.LogWarning($"Failed to find valid disconnected patch chunk position in {16} tries.");
					break;
				}
			}
			while (HasNeighbours(chunkPosition_LC, usedPositions) || !isAvailable(clusterPosition_GC + chunkPosition_LC));
			return chunkPosition_LC;
		}
	}

	private static bool HasNeighbours(ChunkDirection newPosition, HashSet<ChunkDirection> existingPositions)
	{
		Debug.Assert(!existingPositions.Contains(newPosition));
		return existingPositions.Select((ChunkDirection existingPosition) => existingPosition - newPosition).Any((ChunkDirection delta) => math.abs(delta.x) <= 1 && math.abs(delta.y) <= 1);
	}

	private static bool HasTooFewNeighbours(ChunkDirection newPosition, HashSet<ChunkDirection> existingPositions, int minNeighbourCount)
	{
		Debug.Assert(!existingPositions.Contains(newPosition));
		if (existingPositions.Count < minNeighbourCount)
		{
			return false;
		}
		int neighbourCount = existingPositions.Select((ChunkDirection existingPosition) => existingPosition - newPosition).Count((ChunkDirection delta) => math.abs(delta.x) <= 1 && math.abs(delta.y) <= 1);
		return neighbourCount < minNeighbourCount;
	}

	private static bool CreatesExceedingEdgeLength(ChunkDirection newPosition, HashSet<ChunkDirection> existingPositions, int maxEdgeLength)
	{
		Debug.Assert(!existingPositions.Contains(newPosition));
		if (existingPositions.Count == 4 && LocalChunkBounds.From(existingPositions).Dimensions == new ChunkDimensions(2, 2))
		{
			return false;
		}
		if (CreatesExceedingEdgeLengthInDirection(newPosition, existingPositions, ChunkDirection.East, ChunkDirection.North, maxEdgeLength))
		{
			return true;
		}
		if (CreatesExceedingEdgeLengthInDirection(newPosition, existingPositions, ChunkDirection.West, ChunkDirection.South, maxEdgeLength))
		{
			return true;
		}
		if (CreatesExceedingEdgeLengthInDirection(newPosition, existingPositions, ChunkDirection.North, ChunkDirection.West, maxEdgeLength))
		{
			return true;
		}
		if (CreatesExceedingEdgeLengthInDirection(newPosition, existingPositions, ChunkDirection.South, ChunkDirection.East, maxEdgeLength))
		{
			return true;
		}
		return false;
	}

	private static bool CreatesExceedingEdgeLengthInDirection(ChunkDirection newPosition, HashSet<ChunkDirection> existingPositions, ChunkDirection walkDirection, ChunkDirection edgeDirection, int maxEdgeLength)
	{
		Debug.Assert(!existingPositions.Contains(newPosition));
		if (existingPositions.Contains(newPosition + edgeDirection))
		{
			return false;
		}
		int edgeLength = 1;
		ChunkDirection position = newPosition;
		while (existingPositions.Contains(position += walkDirection) && !existingPositions.Contains(position + edgeDirection))
		{
			if (++edgeLength > maxEdgeLength)
			{
				return true;
			}
		}
		position = newPosition;
		while (existingPositions.Contains(position -= walkDirection) && !existingPositions.Contains(position + edgeDirection))
		{
			if (++edgeLength > maxEdgeLength)
			{
				return true;
			}
		}
		return false;
	}

	private static bool CreatesHole(ChunkDirection newPosition, HashSet<ChunkDirection> existingPositions)
	{
		return CreatesHole(newPosition, existingPositions, ChunkDirection.North + ChunkDirection.East) || CreatesHole(newPosition, existingPositions, ChunkDirection.East + ChunkDirection.South) || CreatesHole(newPosition, existingPositions, ChunkDirection.South + ChunkDirection.West) || CreatesHole(newPosition, existingPositions, ChunkDirection.West + ChunkDirection.North);
	}

	private static bool CreatesHole(ChunkDirection newPosition, HashSet<ChunkDirection> existingPositions, ChunkDirection bridgeDirection)
	{
		if (!existingPositions.Contains(newPosition + bridgeDirection))
		{
			return false;
		}
		if (existingPositions.Contains(newPosition + new ChunkDirection(bridgeDirection.x, 0)))
		{
			return false;
		}
		if (existingPositions.Contains(newPosition + new ChunkDirection(0, bridgeDirection.y)))
		{
			return false;
		}
		return true;
	}

	public DefaultMapGenerator(MapGeneratorData data, GameModeConfig config)
	{
		Data = (DefaultMapGeneratorData)data;
		Config = config;
	}

	public IEnumerable<IResourceSourceData> Generate(SuperChunkCoordinate origin_SC)
	{
		ConsistentRandom rng = new ConsistentRandom(Config.Seed + "/" + origin_SC.x + "/" + origin_SC.y);
		GlobalChunkBounds bounds_GC = SuperChunkBounds.From(origin_SC, new ChunkDimensions(1, 1)).To_GC();
		DefaultMapGeneratorData.ChunkOverride overrideData = Data.ChunkOverrides.Get(origin_SC);
		if (overrideData != null)
		{
			foreach (IResourceSourceData item in GenerateFromOverride(origin_SC, overrideData, rng))
			{
				yield return item;
			}
			if (!overrideData.Additional)
			{
				yield break;
			}
		}
		int distanceToOrigin = (int)math.length((int2)origin_SC);
		List<DefaultMapGeneratorData.LinkedSubShape> availableShapes = Data.Shapes.Where((DefaultMapGeneratorData.LinkedSubShape s) => distanceToOrigin >= s.MinimumDistanceToHub).ToList();
		if (availableShapes.Count == 0)
		{
			throw new Exception("No available shapes for chunk with distance " + distanceToOrigin);
		}
		int shapePatchCount = math.clamp(distanceToOrigin / 3, 2, 3);
		int ring = distanceToOrigin / 2;
		ScopedList<GlobalChunkCoordinate> usedPositions = ScopedList<GlobalChunkCoordinate>.Get();
		try
		{
			int fluidPatchCount = 1;
			int i = 0;
			while (i < fluidPatchCount)
			{
				DefaultMapGeneratorData.LinkedColorFluid fluid = rng.Choice(Data.ColorFluids);
				FluidResourceSourceData resource = new FluidResourceSourceData
				{
					Position_GC = GeneratePatchLocation(),
					Tiles_LC = GetLayoutFromSize(fluid.Size),
					FluidResource = ColorFluid.ForColor(fluid.Color)
				};
				usedPositions.AddRange(resource.Tiles_LC.Select((ChunkDirection pos_LC) => resource.Position_GC + pos_LC));
				yield return resource;
				int num = i + 1;
				i = num;
			}
			int i2 = 0;
			while (i2 < shapePatchCount)
			{
				ShapeResourceClusterData cluster = GenerateShapeCluster(GenerateClusterData(new MetaShapeSubPart[4]
				{
					PickRandomShape(availableShapes, rng).Part,
					PickRandomShape(availableShapes, rng).Part,
					PickRandomShape(availableShapes, rng).Part,
					PickRandomShape(availableShapes, rng).Part
				}, rng, ring), GeneratePatchLocation(), ring, rng, IsAvailable);
				usedPositions.AddRange(cluster.ShapeResources.Select((ShapeResourceSourceData r) => cluster.Center_GC + r.Offset_LC));
				yield return cluster;
				int num = i2 + 1;
				i2 = num;
			}
		}
		finally
		{
			if (usedPositions != null)
			{
				((IDisposable)usedPositions).Dispose();
			}
		}
		GlobalChunkCoordinate GeneratePatchLocation(int padding = 3)
		{
			int halfChunk = 32;
			int range = halfChunk - padding;
			GlobalChunkCoordinate randomLocation_GC;
			do
			{
				randomLocation_GC = origin_SC.ToOrigin_GC() + new ChunkDirection(halfChunk, halfChunk) + new ChunkDirection(rng.Next(-range, range), rng.Next(-range, range));
			}
			while (!IsAvailable(randomLocation_GC));
			return randomLocation_GC;
		}
		bool IsAvailable(GlobalChunkCoordinate position)
		{
			return bounds_GC.Includes(position) && usedPositions.Select((GlobalChunkCoordinate usedPosition) => usedPosition - position).All((ChunkDirection delta) => math.abs(delta.x) > 0 || math.abs(delta.y) > 0);
		}
	}

	protected IEnumerable<IResourceSourceData> GenerateFromOverride(SuperChunkCoordinate position_SC, DefaultMapGeneratorData.ChunkOverride overrideData, ConsistentRandom rng)
	{
		GlobalChunkCoordinate origin_GC = position_SC.ToOrigin_GC();
		int fluidPatchCount = overrideData.ColorFluids.Length;
		int i = 0;
		while (i < fluidPatchCount)
		{
			DefaultMapGeneratorData.LinkedColorFluid fluid = overrideData.ColorFluids[i];
			yield return new FluidResourceSourceData
			{
				Position_GC = origin_GC + fluid.Location_LC,
				Tiles_LC = GetLayoutFromSize(fluid.Size),
				FluidResource = ColorFluid.ForColor(fluid.Color)
			};
			int num = i + 1;
			i = num;
		}
		DefaultMapGeneratorData.ShapeClusterOverride[] clusters = overrideData.Clusters;
		for (int j = 0; j < clusters.Length; j++)
		{
			DefaultMapGeneratorData.ShapeClusterOverride cluster = clusters[j];
			yield return new ShapeResourceClusterData(cluster.Patches.Select((DefaultMapGeneratorData.ShapeClusterTile tile) => new ShapeResourceSourceData(tile.Offset, tile.Hash)), origin_GC + cluster.Location_LC);
		}
	}

	protected ChunkDirection[] GetLayoutFromSize(int size)
	{
		if (1 == 0)
		{
		}
		ChunkDirection[] result = size switch
		{
			0 => ResourceSource.LAYOUT_SMALL_3, 
			1 => ResourceSource.LAYOUT_SMALL_6, 
			2 => ResourceSource.LAYOUT_MEDIUM_13, 
			_ => ResourceSource.LAYOUT_SMALL_3, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	protected string GenerateOneLayerShapeHash(IEnumerable<MetaShapeSubPart> parts)
	{
		ShapeLayer layer = new ShapeLayer(parts.Select((MetaShapeSubPart p) => (p == null) ? ShapePart.EMPTY : new ShapePart
		{
			Shape = p,
			Color = Data.ShapeColor
		}).ToArray());
		return new ShapeDefinition(new ShapeLayer[1] { layer }).Hash;
	}

	protected IEnumerable<string> GenerateClusterData(MetaShapeSubPart[] parts, ConsistentRandom rng, int ring)
	{
		if (ring < 0)
		{
			throw new ArgumentOutOfRangeException("ring");
		}
		MetaShapeSubPart[] pureShape = (MetaShapeSubPart[])parts.Clone();
		MetaShapeSubPart[] reducedShape1 = (MetaShapeSubPart[])parts.Clone();
		int reduction1 = rng.Next(0, reducedShape1.Length);
		reducedShape1[reduction1] = null;
		MetaShapeSubPart[] reducedShape2 = (MetaShapeSubPart[])parts.Clone();
		int reduction2;
		for (reduction2 = reduction1; reduction2 == reduction1; reduction2 = rng.Next(0, reducedShape1.Length))
		{
		}
		reducedShape2[reduction1] = null;
		reducedShape2[reduction2] = null;
		yield return GenerateOneLayerShapeHash(reducedShape2);
		if (ring >= 2)
		{
			yield return GenerateOneLayerShapeHash(reducedShape1);
		}
		if (ring >= 3)
		{
			yield return GenerateOneLayerShapeHash(pureShape);
		}
	}
}
