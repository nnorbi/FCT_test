#define UNITY_ASSERTIONS
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class HyperBeltIODrawer : IDrawer<(HyperBelt, HyperBeltInput)>
{
	private const float HEIGHT = 2.5f;

	private const float CENTER_OFFSET = 8f;

	private const float SCALE = 8f;

	private static void DrawInput(FrameDrawOptions draw, GlobalChunkCoordinate coord, Grid.Direction dir)
	{
		Material material = Singleton<GameCore>.G.Theme.BaseResources.UXBuildingBeltInputConnectedMaterial;
		float3 position = coord.ToCenter_W() - 8f * WorldDirection.ByDirection(dir) + 2.5f * WorldDirection.Up;
		Quaternion rotation = FastMatrix.RotateY(Grid.OppositeDirection(dir));
		draw.Draw3DPlaneWithMaterial(material, Matrix4x4.TRS(position, rotation, 8f * Vector3.one));
	}

	private static void DrawOutput(FrameDrawOptions draw, GlobalChunkCoordinate coord, Grid.Direction dir)
	{
		Material material = Singleton<GameCore>.G.Theme.BaseResources.UXBuildingBeltOutputConnectedMaterial;
		float3 position = coord.ToCenter_W() + 8f * WorldDirection.ByDirection(dir) + 2.5f * WorldDirection.Up;
		Quaternion rotation = FastMatrix.RotateY(Grid.OppositeDirection(dir));
		draw.Draw3DPlaneWithMaterial(material, Matrix4x4.TRS(position, rotation, 8f * Vector3.one));
	}

	private static void Draw(FrameDrawOptions draw, in HyperBelt belt, in HyperBeltInput input)
	{
		Debug.Assert(belt.Nodes.Length > 0);
		DrawInput(draw, belt.Nodes[0].Position, belt.Nodes[0].Direction);
		if (input.StartedPlacement)
		{
			NativeSlice<HyperBeltNode> nodes = belt.Nodes;
			HyperBeltNode last = nodes[nodes.Length - 1];
			DrawOutput(draw, last.Position, GetOutputDirection(last));
		}
	}

	private static Grid.Direction GetOutputDirection(HyperBeltNode node)
	{
		HyperBeltPart part = node.Part;
		if (1 == 0)
		{
		}
		Grid.Direction direction = part switch
		{
			HyperBeltPart.TunnelSender => Grid.Direction.Right, 
			HyperBeltPart.TunnelReceiver => Grid.Direction.Right, 
			HyperBeltPart.LeftTurn => Grid.Direction.Top, 
			HyperBeltPart.RightTurn => Grid.Direction.Bottom, 
			HyperBeltPart.Forward => Grid.Direction.Right, 
			HyperBeltPart.Invalid => Grid.Direction.Right, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		Grid.Direction offsetRotation = direction;
		return Grid.RotateDirection(node.Direction, offsetRotation);
	}

	public void Draw(FrameDrawOptions draw, in (HyperBelt, HyperBeltInput) data)
	{
		Draw(draw, in data.Item1, in data.Item2);
	}

	void IDrawer<(HyperBelt, HyperBeltInput)>.Draw(FrameDrawOptions draw, in (HyperBelt, HyperBeltInput) data)
	{
		Draw(draw, in data);
	}
}
