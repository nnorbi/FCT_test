using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class TrainRailNode
{
	protected static int INSTANCING_ID_RAIL_FORWARD_UX = Shader.PropertyToID("train-rail::ux-rail-forward");

	protected static int INSTANCING_ID_RAIL_LEFT_UX = Shader.PropertyToID("train-rail::ux-rail-left");

	protected static int INSTANCING_ID_RAIL_RIGHT_UX = Shader.PropertyToID("train-rail::ux-rail-right");

	public int2 Position_TG;

	public List<TrainSubPath> Connections = new List<TrainSubPath>();

	public Bounds Bounds { get; }

	public static float2 TG_From_W(Vector3 pos_W)
	{
		return new float2((pos_W.x + 0.5f) / (float)TrainManager.RAIL_GRID_SIZE_G, (0f - pos_W.z + 0.5f) / (float)TrainManager.RAIL_GRID_SIZE_G);
	}

	public static int2 TG_TruncSnapped_From_TG(float2 pos_TG)
	{
		int2 coord_TG = new int2((int)math.round(pos_TG.x), (int)math.round(pos_TG.y));
		if (IsValidCoordinate_TG(in coord_TG))
		{
			return coord_TG;
		}
		float2 fract = pos_TG - coord_TG;
		if (math.abs(fract.x) > math.abs(fract.y))
		{
			return coord_TG + ((fract.x > 0f) ? new int2(1, 0) : new int2(-1, 0));
		}
		return coord_TG + ((fract.y > 0f) ? new int2(0, 1) : new int2(0, -1));
	}

	public static float3 W_From_TG(in int2 pos_TG)
	{
		return new float3((float)(pos_TG.x * TrainManager.RAIL_GRID_SIZE_G) - 0.5f, TrainManager.RAIL_HEIGHT_W, (float)(-pos_TG.y * TrainManager.RAIL_GRID_SIZE_G) + 0.5f);
	}

	public static GlobalTileCoordinate G_From_TG(in int2 pos_TG, short layer = 0)
	{
		return new GlobalTileCoordinate(pos_TG.x * TrainManager.RAIL_GRID_SIZE_G, pos_TG.y * TrainManager.RAIL_GRID_SIZE_G, layer);
	}

	public static float2 G_From_TG(in float2 pos_TG)
	{
		return new float2(pos_TG.x * (float)TrainManager.RAIL_GRID_SIZE_G - 0.5f, pos_TG.y * (float)TrainManager.RAIL_GRID_SIZE_G - 0.5f);
	}

	public static float3 W_From_TG(in float2 pos_TG)
	{
		return new float3(pos_TG.x * (float)TrainManager.RAIL_GRID_SIZE_G - 0.5f, TrainManager.RAIL_HEIGHT_W, (0f - pos_TG.y) * (float)TrainManager.RAIL_GRID_SIZE_G + 0.5f);
	}

	public static bool IsValidCoordinate_TG(in int2 position_TG)
	{
		return FastMath.SafeMod(position_TG.x + position_TG.y, 2) == 1;
	}

	public static bool IsVertical_TG(in int2 position_TG)
	{
		return position_TG.x % 2 == 0;
	}

	public static bool IsConnectionPossible(in int2 pA, in int2 pB)
	{
		if (math.abs(pA.x - pB.x) == 1 && math.abs(pA.y - pB.y) == 1)
		{
			return true;
		}
		bool verticalA = IsVertical_TG(in pA);
		bool verticalB = IsVertical_TG(in pB);
		if (verticalA != verticalB)
		{
			return false;
		}
		if (verticalA)
		{
			if (pA.x == pB.x && math.abs(pA.y - pB.y) == 2)
			{
				return true;
			}
		}
		else if (pA.y == pB.y && math.abs(pA.x - pB.x) == 2)
		{
			return true;
		}
		return false;
	}

	public static void DrawRailOverviewUX(FrameDrawOptions options, int2 from_TG, int2 to_TG, TrainSubPath.TrainPathType pathType, Grid.Direction pathDirection, Color color, bool isPending = false, float alpha = 1f)
	{
		MaterialPropertyBlock propertyBlock = MaterialPropertyHelpers.CreateBaseColorBlock(color);
		propertyBlock.SetFloat(MaterialPropertyHelpers.SHADER_ID_Alpha, alpha);
		float3 offset_W = new float3(0f, 0f, 0f);
		VisualThemeBaseResources resources = options.Theme.BaseResources;
		switch (pathType)
		{
		case TrainSubPath.TrainPathType.Forward:
			options.Draw3DPlaneWithMaterial(isPending ? resources.UXRailPlacementForwardMaterial : resources.UXRailForwardMaterial, Matrix4x4.TRS(W_From_TG((float2)(from_TG + to_TG) / 2f) + offset_W, s: Vector3.one * TrainManager.RAIL_GRID_SIZE_G * 2f, q: FastMatrix.RotateY(pathDirection)), propertyBlock);
			break;
		case TrainSubPath.TrainPathType.TurnLeft:
			options.Draw3DPlaneWithMaterial(isPending ? resources.UXRailPlacementLeftMaterial : resources.UXRailLeftMaterial, Matrix4x4.TRS(W_From_TG(from_TG + Grid.Rotate(new int2(1, 0), pathDirection)) + offset_W, s: Vector3.one * TrainManager.RAIL_GRID_SIZE_G * 2f, q: FastMatrix.RotateY(Grid.OppositeDirection(pathDirection))), propertyBlock);
			break;
		case TrainSubPath.TrainPathType.TurnRight:
			options.Draw3DPlaneWithMaterial(isPending ? resources.UXRailPlacementRightMaterial : resources.UXRailRightMaterial, Matrix4x4.TRS(W_From_TG(from_TG + Grid.Rotate(new int2(1, 0), pathDirection)) + offset_W, s: Vector3.one * TrainManager.RAIL_GRID_SIZE_G * 2f, q: FastMatrix.RotateY(Grid.OppositeDirection(pathDirection))), propertyBlock);
			break;
		}
	}

	public TrainRailNode(int2 position_TG)
	{
		Position_TG = position_TG;
		Bounds = new Bounds(W_From_TG(in position_TG), new Vector3(TrainManager.RAIL_GRID_SIZE_G * 4, 5f, TrainManager.RAIL_GRID_SIZE_G * 4));
	}

	public bool HasAuthorityOver(TrainRailNode other)
	{
		if (Position_TG.x == other.Position_TG.x)
		{
			return Position_TG.y > other.Position_TG.y;
		}
		return Position_TG.x > other.Position_TG.x;
	}

	public bool IsConnectedTo(TrainRailNode other, MetaShapeColor color)
	{
		return GetConnectionTo(other, color) != null;
	}

	public TrainSubPath GetConnectionTo(TrainRailNode other, MetaShapeColor color)
	{
		return Connections.FirstOrDefault((TrainSubPath conn) => conn.To == other && conn.Colors.Contains(color));
	}

	public void ConnectTo(TrainRailNode other, MetaShapeColor color)
	{
		foreach (TrainSubPath connection in Connections)
		{
			if (connection.To != other)
			{
				continue;
			}
			if (connection.Colors.Contains(color))
			{
				int2 position_TG = other.Position_TG;
				throw new Exception("Can not create double connection with " + position_TG.ToString() + " and color " + color);
			}
			connection.Colors.Add(color);
			return;
		}
		TrainSubPath subPath = new TrainSubPath(this, other);
		subPath.Colors.Add(color);
		Connections.Add(subPath);
	}

	public void DisconnectFrom(TrainRailNode other, MetaShapeColor color)
	{
		TrainSubPath connection = GetConnectionTo(other, color);
		if (connection == null)
		{
			int2 position_TG = other.Position_TG;
			throw new Exception("Connection does not contain color to remove from " + position_TG.ToString() + " and color " + color);
		}
		connection.Colors.Remove(color);
	}

	public void OnGameDraw(FrameDrawOptions options)
	{
		options.Hooks.OnDrawRail(options, this);
		foreach (TrainSubPath connection in Connections)
		{
			if (HasAuthorityOver(connection.To) && connection.Colors.Count != 0)
			{
				DrawRailOverviewMode(options, connection);
			}
		}
	}

	private void DrawRailOverviewMode(FrameDrawOptions options, TrainSubPath connection)
	{
		DrawRailOverviewUX(options, Position_TG, connection.To.Position_TG, connection.Descriptor.Type, connection.Descriptor.Direction, Globals.Resources.ThemePrimary.Color, isPending: false, 0.3f);
	}
}
