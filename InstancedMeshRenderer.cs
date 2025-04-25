using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class InstancedMeshRenderer
{
	private class DataBlock
	{
		public Matrix4x4[] Data = new Matrix4x4[MAX_SIZE];

		public int Count = 0;

		public float LastUsed = -1E+10f;

		public bool Flushed = false;
	}

	public static int STATS_TOTAL_INSTANCES = 0;

	public static int MAX_SIZE = 1023;

	public static int HISTORY_ANALYSIS_SIZE = 60;

	public static int THRESHOLD_RENDER_INSTANCED = 1;

	public static float BLOCK_MAX_AGE = 5f;

	private List<DataBlock> Blocks = new List<DataBlock>();

	private List<int> RenderCountsHistory = new List<int>();

	private bool RenderInstanced = true;

	private string MeshName;

	private string MaterialName;

	public bool CastShadows;

	public bool ReceiveShadows;

	private RenderCategory Category;

	public Mesh Mesh { get; private set; }

	public Material Material { get; private set; }

	public MaterialPropertyBlock PropertyBlock { get; }

	public Camera Camera { get; }

	public int Layer { get; }

	public int BlockCount => Blocks.Count;

	public InstancedMeshRenderer(Mesh mesh, Material material, MaterialPropertyBlock propertyBlock, Camera camera, int layer, bool castShadows, bool receiveShadows, RenderCategory category)
	{
		Mesh = mesh;
		Material = material;
		PropertyBlock = propertyBlock;
		Camera = camera;
		Layer = layer;
		MeshName = mesh.name;
		MaterialName = material.name;
		CastShadows = castShadows;
		ReceiveShadows = receiveShadows;
		Category = category;
	}

	public int ComputeMaxInstanceHistoryCount()
	{
		int maxDrawnHistory = 0;
		for (int i = 0; i < RenderCountsHistory.Count; i++)
		{
			maxDrawnHistory = math.max(maxDrawnHistory, RenderCountsHistory[i]);
		}
		return maxDrawnHistory;
	}

	public void AddInstance(in Matrix4x4 trs, Mesh referenceMesh, Material referenceMaterial, bool castShadows, bool receiveShadows)
	{
		Mesh = referenceMesh;
		Material = referenceMaterial;
		CastShadows = castShadows;
		ReceiveShadows = receiveShadows;
		int blockCount = Blocks.Count;
		float now = Time.realtimeSinceStartup;
		for (int i = 0; i < blockCount; i++)
		{
			DataBlock block = Blocks[i];
			if (!block.Flushed && block.Count < MAX_SIZE)
			{
				block.Data[block.Count] = trs;
				block.LastUsed = now;
				block.Count++;
				return;
			}
		}
		DataBlock newBlock = new DataBlock();
		newBlock.Data[0] = trs;
		newBlock.Count = 1;
		newBlock.LastUsed = now;
		Blocks.Add(newBlock);
	}

	public void GarbageCollect()
	{
		float now = Time.realtimeSinceStartup;
		int blockCount = Blocks.Count;
		for (int i = blockCount - 1; i >= 0; i--)
		{
			DataBlock block = Blocks[i];
			if (now - block.LastUsed > BLOCK_MAX_AGE)
			{
				Blocks.RemoveAt(i);
			}
		}
	}

	private void Flush(DataBlock block)
	{
		if (block.Count == 0)
		{
			return;
		}
		Camera camera = Camera;
		STATS_TOTAL_INSTANCES += block.Count;
		if (RenderInstanced)
		{
			Graphics.DrawMeshInstanced(Mesh, 0, Material, block.Data, block.Count, PropertyBlock, CastShadows ? ShadowCastingMode.On : ShadowCastingMode.Off, ReceiveShadows, Layer, camera, LightProbeUsage.Off, null);
			return;
		}
		int count = block.Count;
		for (int i = 0; i < count; i++)
		{
			Graphics.DrawMesh(Mesh, block.Data[i], Material, 0, camera, 0, PropertyBlock, CastShadows, ReceiveShadows, useLightProbes: false);
		}
	}

	private int ComputeInstanceCount()
	{
		int result = 0;
		foreach (DataBlock block in Blocks)
		{
			result += block.Count;
		}
		return result;
	}

	public void Draw(FrameDrawOptions options)
	{
		int blockCount = Blocks.Count;
		bool empty = true;
		for (int i = 0; i < blockCount; i++)
		{
			if (Blocks[i].Count > 0)
			{
				empty = false;
				break;
			}
		}
		if (empty)
		{
			RenderCountsHistory.Add(0);
			return;
		}
		if (Mesh == null)
		{
			Debug.LogError("Mesh '" + MeshName + "' of instanced renderer vanished (instances: " + ComputeInstanceCount() + ")");
			return;
		}
		if (Material == null)
		{
			Debug.LogError("Material '" + MaterialName + "' of instanced renderer vanished (instances: " + ComputeInstanceCount() + ")");
			return;
		}
		RenderInstanced = THRESHOLD_RENDER_INSTANCED <= 1 || ComputeMaxInstanceHistoryCount() > THRESHOLD_RENDER_INSTANCED;
		int drawn = 0;
		for (int j = 0; j < blockCount; j++)
		{
			DataBlock block = Blocks[j];
			if (!block.Flushed && block.Count > 0)
			{
				drawn += block.Count;
				Flush(block);
			}
			block.Count = 0;
			block.Flushed = false;
		}
		RenderCountsHistory.Add(drawn);
		if (RenderCountsHistory.Count > HISTORY_ANALYSIS_SIZE)
		{
			RenderCountsHistory.RemoveAt(0);
		}
	}
}
