using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpaceThemeShapeAsteroidVisualizationBuilder : IShapeAsteroidBuilder
{
	private readonly GlobalChunkCoordinate Origin_GC;

	private readonly ShapeAsteroidVisualization Visualization;

	private Unity.Mathematics.Random CurrentRandom;

	private float3 CurrentPosition_W;

	private char CurrentNorthWestShape;

	private char CurrentNorthEastShape;

	private char CurrentSouthEastShape;

	private char CurrentSouthWestShape;

	private readonly List<(LODBaseMesh, Matrix4x4)> LodMeshes = new List<(LODBaseMesh, Matrix4x4)>();

	public SpaceThemeShapeAsteroidVisualizationBuilder(GlobalChunkCoordinate origin_GC, ShapeAsteroidVisualization visualization)
	{
		Origin_GC = origin_GC;
		Visualization = visualization;
	}

	public void PushResource(ChunkDirection relativeCoordinate, ShapeDefinition shapeDefinition)
	{
		GlobalChunkCoordinate pos_GC = Origin_GC + relativeCoordinate;
		CurrentRandom = MathematicsRandomUtils.SafeRandom(pos_GC);
		CurrentPosition_W = pos_GC.ToCenter_W(Visualization.HeightOffset);
		CurrentNorthWestShape = ShapeResourceSourceUtils.SelectCode(shapeDefinition.Layers[0], 3);
		CurrentNorthEastShape = ShapeResourceSourceUtils.SelectCode(shapeDefinition.Layers[0], 0);
		CurrentSouthEastShape = ShapeResourceSourceUtils.SelectCode(shapeDefinition.Layers[0], 1);
		CurrentSouthWestShape = ShapeResourceSourceUtils.SelectCode(shapeDefinition.Layers[0], 2);
	}

	public void PushPlatform()
	{
		LOD4Mesh mesh = Visualization.Platform.RandomChoice(ref CurrentRandom);
		LodMeshes.Add((mesh, FastMatrix.Translate(in CurrentPosition_W)));
	}

	public void PushFillerEdge(Grid.Direction direction)
	{
		LOD4Mesh mesh = Visualization.FillerEdge.RandomChoice(ref CurrentRandom);
		LodMeshes.Add((mesh, FastMatrix.TranslateRotate(in CurrentPosition_W, direction)));
	}

	public void PushFillerCrossing()
	{
		LOD4Mesh mesh = Visualization.FillerCrossing.RandomChoice(ref CurrentRandom);
		LodMeshes.Add((mesh, FastMatrix.Translate(in CurrentPosition_W)));
	}

	public void PushPart(OutlineCardinal outlineCardinal, ShapeAsteroidDecorationPart decorationPart, Grid.Direction direction = Grid.Direction.Right)
	{
		float3 position_W = CurrentPosition_W;
		ShapeAsteroidVisualization.Layer[] layers = Visualization.Layers;
		foreach (ShapeAsteroidVisualization.Layer layer in layers)
		{
			ShapeAsteroidVisualization.Outline currentNorthWestOutline = layer.FindOutline(CurrentNorthWestShape, allowFallback: true);
			ShapeAsteroidVisualization.Outline currentNorthEastOutline = layer.FindOutline(CurrentNorthEastShape, allowFallback: true);
			ShapeAsteroidVisualization.Outline currentSouthEastOutline = layer.FindOutline(CurrentSouthEastShape, allowFallback: true);
			ShapeAsteroidVisualization.Outline currentSouthWestOutline = layer.FindOutline(CurrentSouthWestShape, allowFallback: true);
			position_W += (float3)(WorldDirection.Up * layer.HeightOffset);
			if (1 == 0)
			{
			}
			ShapeAsteroidVisualization.Outline outline = outlineCardinal switch
			{
				OutlineCardinal.NorthWest => currentNorthWestOutline, 
				OutlineCardinal.NorthEast => currentNorthEastOutline, 
				OutlineCardinal.SouthEast => currentSouthEastOutline, 
				OutlineCardinal.SouthWest => currentSouthWestOutline, 
				_ => throw new ArgumentOutOfRangeException("outlineCardinal", outlineCardinal, null), 
			};
			if (1 == 0)
			{
			}
			ShapeAsteroidVisualization.Outline outline2 = outline;
			if (1 == 0)
			{
			}
			LOD4Mesh[] array = decorationPart switch
			{
				ShapeAsteroidDecorationPart.ConcaveLeft => outline2.ConcaveL, 
				ShapeAsteroidDecorationPart.ConcaveRight => outline2.ConcaveR, 
				ShapeAsteroidDecorationPart.StraightLeft => outline2.StraightL, 
				ShapeAsteroidDecorationPart.StraightRight => outline2.StraightR, 
				ShapeAsteroidDecorationPart.Convex => outline2.Convex, 
				_ => throw new ArgumentOutOfRangeException("decorationPart", decorationPart, null), 
			};
			if (1 == 0)
			{
			}
			LOD4Mesh[] mesh = array;
			LodMeshes.Add((mesh.RandomChoice(ref CurrentRandom), FastMatrix.TranslateRotate(in position_W, direction)));
		}
	}

	public ISpaceThemeShapeAsteroidVisualization Generate(bool combine)
	{
		ISpaceThemeShapeAsteroidVisualization visualization = ((!combine) ? ((ISpaceThemeShapeAsteroidVisualization)new SpaceThemeShapeAsteroidInstancedVisualization(Origin_GC, Visualization.Material, Visualization.LOD1Distance, Visualization.LOD2Distance, Visualization.LOD3Distance)) : ((ISpaceThemeShapeAsteroidVisualization)new SpaceThemeShapeAsteroidCombinedVisualization(Origin_GC, Visualization.Material, Visualization.LOD1Distance, Visualization.LOD2Distance, Visualization.LOD3Distance)));
		foreach (var (mesh, transform) in LodMeshes)
		{
			visualization.Add(mesh, transform);
		}
		return visualization;
	}
}
