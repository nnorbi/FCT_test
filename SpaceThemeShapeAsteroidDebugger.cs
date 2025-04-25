using System;
using Drawing;
using Unity.Mathematics;

public class SpaceThemeShapeAsteroidDebugger : IShapeAsteroidBuilder
{
	private const float HeightBetweenText = 2f;

	private const float TextFontSize = 0.5f;

	private GlobalChunkCoordinate CurrentCoordinate;

	private char CurrentNorthWestShape;

	private char CurrentNorthEastShape;

	private char CurrentSouthEastShape;

	private char CurrentSouthWestShape;

	private int CurrentLayer;

	private int[] CurrentDisplayedPerDirection;

	private readonly GlobalChunkCoordinate Origin_GC;

	private CommandBuilder DrawCommandBuilder;

	private readonly float BaseOffset;

	private static string ShortCardinal(OutlineCardinal cardinal)
	{
		if (1 == 0)
		{
		}
		string result = cardinal switch
		{
			OutlineCardinal.NorthWest => "NW", 
			OutlineCardinal.NorthEast => "NE", 
			OutlineCardinal.SouthEast => "SE", 
			OutlineCardinal.SouthWest => "SW", 
			_ => throw new ArgumentOutOfRangeException("cardinal", cardinal, null), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static string ShortDecoration(ShapeAsteroidDecorationPart decoration)
	{
		if (1 == 0)
		{
		}
		string result = decoration switch
		{
			ShapeAsteroidDecorationPart.ConcaveLeft => "Concave_R", 
			ShapeAsteroidDecorationPart.ConcaveRight => "Concave_L", 
			ShapeAsteroidDecorationPart.StraightLeft => "Straight_L", 
			ShapeAsteroidDecorationPart.StraightRight => "Straight_R", 
			ShapeAsteroidDecorationPart.Convex => "Convex", 
			_ => throw new ArgumentOutOfRangeException("decoration", decoration, null), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public SpaceThemeShapeAsteroidDebugger(GlobalChunkCoordinate origin_GC, CommandBuilder drawCommandBuilder, float baseOffset)
	{
		Origin_GC = origin_GC;
		DrawCommandBuilder = drawCommandBuilder;
		BaseOffset = baseOffset;
		CurrentDisplayedPerDirection = new int[5];
	}

	public void PushResource(ChunkDirection relativeCoordinate, ShapeDefinition shapeDefinition)
	{
		CurrentCoordinate = Origin_GC + relativeCoordinate;
		CurrentNorthWestShape = ShapeResourceSourceUtils.SelectCode(shapeDefinition.Layers[0], 3);
		CurrentNorthEastShape = ShapeResourceSourceUtils.SelectCode(shapeDefinition.Layers[0], 0);
		CurrentSouthEastShape = ShapeResourceSourceUtils.SelectCode(shapeDefinition.Layers[0], 1);
		CurrentSouthWestShape = ShapeResourceSourceUtils.SelectCode(shapeDefinition.Layers[0], 2);
		for (int i = 0; i < CurrentDisplayedPerDirection.Length; i++)
		{
			CurrentDisplayedPerDirection[i] = 0;
		}
	}

	public void PushLayer(int index)
	{
		for (int i = 0; i < 5; i++)
		{
			CurrentDisplayedPerDirection[i] = 0;
		}
		CurrentLayer = index;
	}

	public void PushPlatform()
	{
		DisplayData("Platform");
	}

	public void PushFillerEdge(Grid.Direction direction)
	{
		DisplayData("Filler edge", direction);
	}

	public void PushFillerCrossing()
	{
		DisplayData("Filler crossing");
	}

	public void PushPart(OutlineCardinal outlineCardinal, ShapeAsteroidDecorationPart decorationPart, Grid.Direction direction = Grid.Direction.Right)
	{
		if (1 == 0)
		{
		}
		char c = outlineCardinal switch
		{
			OutlineCardinal.NorthWest => CurrentNorthWestShape, 
			OutlineCardinal.NorthEast => CurrentNorthEastShape, 
			OutlineCardinal.SouthEast => CurrentSouthEastShape, 
			OutlineCardinal.SouthWest => CurrentSouthWestShape, 
			_ => throw new ArgumentOutOfRangeException("outlineCardinal", outlineCardinal, null), 
		};
		if (1 == 0)
		{
		}
		char code = c;
		DisplayData($"[{CurrentLayer}:{ShortCardinal(outlineCardinal)}] {code} -- {ShortDecoration(decorationPart)}", direction);
	}

	private void DisplayData(string text)
	{
		float height = 2f * (float)CurrentDisplayedPerDirection[^1];
		float3 pos = CurrentCoordinate.ToCenter_W(BaseOffset + height);
		DrawCommandBuilder.Label3D(pos, quaternion.identity, text, 0.5f);
		CurrentDisplayedPerDirection[^1]++;
	}

	private void DisplayData(string text, Grid.Direction direction)
	{
		float height = 2f * (float)CurrentDisplayedPerDirection[(int)direction]++;
		float3 pos = CurrentCoordinate.ToCenter_W(BaseOffset + height);
		pos += (float3)((WorldDirection)direction * 9.5f);
		Grid.Direction rotation = Grid.RotateDirection(direction, Grid.Direction.Top);
		DrawCommandBuilder.Label3D(pos, FastMatrix.RotateY(rotation), text, 0.5f);
	}
}
