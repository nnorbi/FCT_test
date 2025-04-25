using System;
using Unity.Mathematics;
using UnityEngine;

public class HUDOverviewModeTunnelsRenderer
{
	private static int INSTANCING_ID = Shader.PropertyToID("overview-mode-tunnels::sprite");

	public void DrawSuperChunk(FrameDrawOptions options, MapSuperChunk superChunk, float alpha)
	{
		foreach (Island island in superChunk.Islands)
		{
			if (island is TunnelEntranceIsland entrance)
			{
				DrawTunnel(options, entrance, alpha);
			}
		}
	}

	private void DrawTunnel(FrameDrawOptions options, TunnelEntranceIsland entrance, float alpha)
	{
		TunnelExitIsland receiver = entrance.CachedExit;
		if (receiver == null)
		{
			return;
		}
		bool goViaX = entrance.Origin_GC.y == receiver.Origin_GC.y;
		int start = (goViaX ? entrance.Origin_GC.x : entrance.Origin_GC.y);
		int end = (goViaX ? receiver.Origin_GC.x : receiver.Origin_GC.y);
		if (start == end)
		{
			GlobalChunkCoordinate origin_GC = entrance.Origin_GC;
			string text = origin_GC.ToString();
			origin_GC = receiver.Origin_GC;
			throw new Exception("invalid tunnel connection: " + text + " vs " + origin_GC.ToString());
		}
		int steps = math.abs(end - start);
		int delta = (int)math.sign(end - start);
		Material material = options.Theme.BaseResources.UXOverviewModeTunnelsMaterial;
		Quaternion rotationQuaternion = FastMatrix.RotateY(Grid.OppositeDirection(entrance.Metadata.LayoutRotation));
		for (int i = 1; i < steps; i++)
		{
			float3 basePos_W = (entrance.Origin_GC + (goViaX ? new ChunkDirection(i * delta, 0) : new ChunkDirection(0, i * delta))).ToCenter_W(-1f);
			if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, new Bounds(basePos_W, Vector3.one * 20f)))
			{
				float3 coordinate_W = basePos_W;
				int iNSTANCING_ID = INSTANCING_ID;
				Vector3 pos = coordinate_W;
				Vector3 s = new Vector3(20f, 1f, 20f * alpha);
				options.Draw3DPlaneWithMaterialInstanced(iNSTANCING_ID, material, Matrix4x4.TRS(pos, rotationQuaternion, s));
			}
		}
	}
}
