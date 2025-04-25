using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Building Internal Variant", menuName = "Metadata/Building/3 - Internal Variant", order = 3)]
public class MetaBuildingInternalVariant : ScriptableObject
{
	[Serializable]
	public struct CurveEntry
	{
		public string ID;

		public AnimationCurve Curve;
	}

	[Serializable]
	public class BaseIO
	{
		public Grid.Direction Direction_L;

		public TileDirection Position_L;
	}

	[Serializable]
	public class CollisionBox
	{
		public float3 Center_L = float3.zero;

		public float3 Dimensions_L = new float3(1f, 1f, 1f);

		[HideInInspector]
		public float3[] DimensionsByRotation_W = new float3[4];

		public void ComputeDimensions()
		{
			for (int i = 0; i < 4; i++)
			{
				float2 rotatedXYBounds = math.abs(Grid.Rotate(Dimensions_L.xy, (Grid.Direction)i));
				DimensionsByRotation_W[i] = Grid.Scale_W_From_G(new float3(rotatedXYBounds.xy, Dimensions_L.z));
			}
		}

		public bool Contains(float3 p_L)
		{
			float3 p = p_L - Center_L;
			float3 d = Dimensions_L / 2f;
			return p.x >= 0f - d.x && p.x <= d.x && p.y >= 0f - d.y && p.y <= d.y && p.z >= 0f - d.z && p.z <= d.z;
		}
	}

	[Serializable]
	public class BeltIO : BaseIO
	{
		public BeltIOType IOType = BeltIOType.Regular;

		public BeltStandType StandType = BeltStandType.Normal;

		public bool Seperators = true;
	}

	[Serializable]
	public class FluidIO : BaseIO
	{
		public FluidIOType IOType = FluidIOType.Building;
	}

	[Serializable]
	public class FluidContainerConfig
	{
		public string Name = "Container";

		public float Max = 50f;

		public bool AllowDrain = true;

		public bool AllowGain = true;

		public float3 Position_L;

		public FluidIO[] Connections;

		[HideInInspector]
		public int Index;
	}

	[Serializable]
	public class AdditionalBlueprintMesh
	{
		public float3 Pos = new float3(0.5f, 0.5f, 0.5f);

		public Quaternion Rotate = Quaternion.identity;

		public float3 Scale = new float3(1f, 1f, 1f);

		[ValidateMesh(2000)]
		public Mesh Mesh;
	}

	public enum BeltIOType
	{
		None,
		Regular,
		ElevatedBorder
	}

	public enum BeltStandType
	{
		None,
		Normal
	}

	public enum FluidIOType
	{
		Building,
		Pipe
	}

	[Space(20f)]
	public bool RenderVoidBelow = false;

	public bool SymmetricOnXAxis = false;

	public bool IsMirrored;

	public MetaBuildingInternalVariant MirroredInternalVariant;

	[SerializeField]
	public EditorClassID<MapEntity> Implementation = new EditorClassID<MapEntity>("MapEntity");

	public EditorClassIDSingleton<BuildingOutputPredictor> OutputPredictorClass = new EditorClassIDSingleton<BuildingOutputPredictor>("BuildingOutputPredictor");

	[Space(20f)]
	public bool HasMainMesh = true;

	public bool IndividualMainMeshPerLayer = false;

	public LOD4Mesh MainMeshLOD = new LOD4Mesh();

	public LOD4Mesh[] MainMeshPerLayerLOD = new LOD4Mesh[0];

	[Space(20f)]
	[ValidateMesh(5000)]
	public Mesh BlueprintMeshOverride = null;

	public AdditionalBlueprintMesh[] AdditionalBlueprintMeshes = new AdditionalBlueprintMesh[0];

	[Space(20f)]
	public bool HasGlassMesh = false;

	public LOD4Mesh GlassMeshLOD = new LOD4Mesh();

	[Space(20f)]
	public LOD2Mesh[] SupportMeshesInternalLOD = new LOD2Mesh[0];

	[Space(20f)]
	public CurveEntry[] AnimationCurves = new CurveEntry[0];

	[Space(20f)]
	public CollisionBox[] Colliders = new CollisionBox[0];

	[Space(20f)]
	[SerializeField]
	public BeltIO[] BeltInputs = new BeltIO[0];

	[Space(10f)]
	[SerializeField]
	public BeltIO[] BeltOutputs = new BeltIO[0];

	[Space(20f)]
	[SerializeField]
	public BeltLaneDefinition[] BeltLaneDefinitions = new BeltLaneDefinition[0];

	[Space(20f)]
	public FluidContainerConfig[] FluidContainers = new FluidContainerConfig[0];

	[HideInInspector]
	public MetaBuildingVariant Variant;

	[Space(20f)]
	public TileDirection[] Tiles = new TileDirection[0];

	[NonSerialized]
	public int3x2 BoundsInTileSpace;

	protected CombinedMesh CachedBlueprintMesh = null;

	[NonSerialized]
	public int Height;

	public TileDirection DimensionsInTileSpace => new TileDirection(BoundsInTileSpace.c1 - BoundsInTileSpace.c0);

	public bool Mirrorable => SymmetricOnXAxis || MirroredInternalVariant != null;

	public Mesh BlueprintMeshBase
	{
		get
		{
			if (BlueprintMeshOverride != null)
			{
				return BlueprintMeshOverride;
			}
			if (HasMainMesh)
			{
				if (IndividualMainMeshPerLayer)
				{
					return MainMeshPerLayerLOD[0].LODNormal;
				}
				return MainMeshLOD.LODNormal;
			}
			return null;
		}
	}

	public CombinedMesh CombinedBlueprintMesh
	{
		get
		{
			if (CachedBlueprintMesh == null)
			{
				BlueprintMeshGenerator.GenerateMesh(this, ref CachedBlueprintMesh);
			}
			return CachedBlueprintMesh;
		}
	}

	protected virtual void OnValidate()
	{
		if (HasMainMesh)
		{
			if (IndividualMainMeshPerLayer)
			{
				if (MainMeshPerLayerLOD.Length < 1)
				{
					Debug.LogWarning("Building internal " + base.name + " has no main mesh per layer!");
				}
			}
			else if (MainMeshLOD.LODNormal == null)
			{
				Debug.LogWarning("Building internal " + base.name + " has no main mesh!");
			}
		}
		if (Tiles.Length == 0)
		{
			throw new Exception("Building internal " + base.name + " has no tiles assigned!");
		}
		if (!HasMainMesh && BlueprintMeshOverride == null)
		{
			Debug.LogWarning("Building internal " + base.name + " has no main mesh AND no blueprint override! Will be invisible");
		}
		if (Colliders.Length == 0)
		{
			throw new Exception("Building internal " + base.name + " has no colliders defined! Will be not selectable or deletable.");
		}
		if (SymmetricOnXAxis)
		{
			MirroredInternalVariant = null;
		}
		Implementation.Validate();
		HashSet<TileDirection> tiles = new HashSet<TileDirection>();
		TileDirection[] tiles2 = Tiles;
		foreach (TileDirection tile in tiles2)
		{
			if (tiles.Contains(tile))
			{
				string text = base.name;
				TileDirection tileDirection = tile;
				throw new Exception("Building internal " + text + " has double tile: " + tileDirection.ToString());
			}
			tiles.Add(tile);
		}
		HashSet<TileDirection> inputs = new HashSet<TileDirection>();
		HashSet<TileDirection> outputs = new HashSet<TileDirection>();
		BeltIO[] beltInputs = BeltInputs;
		foreach (BeltIO input in beltInputs)
		{
			if (!tiles.Contains(input.Position_L))
			{
				TileDirection tileDirection = input.Position_L;
				throw new Exception("Input tile not contained in building: " + tileDirection.ToString());
			}
			TileDirection target = input.Position_L + input.Direction_L;
			if (inputs.Contains(target))
			{
				string[] obj = new string[6] { "Duplicate input pointing torwads ", null, null, null, null, null };
				TileDirection tileDirection = target;
				obj[1] = tileDirection.ToString();
				obj[2] = " -> ";
				tileDirection = input.Position_L;
				obj[3] = tileDirection.ToString();
				obj[4] = " / ";
				obj[5] = input.Direction_L.ToString();
				throw new Exception(string.Concat(obj));
			}
			inputs.Add(target);
		}
		BeltIO[] beltOutputs = BeltOutputs;
		foreach (BeltIO output in beltOutputs)
		{
			if (!tiles.Contains(output.Position_L))
			{
				TileDirection tileDirection = output.Position_L;
				throw new Exception("Output tile not contained in building: " + tileDirection.ToString() + " at " + base.name);
			}
			TileDirection target2 = output.Position_L + output.Direction_L;
			if (inputs.Contains(target2) || outputs.Contains(target2))
			{
				string[] obj2 = new string[8] { "Duplicate output on ", base.name, " pointing torwads ", null, null, null, null, null };
				TileDirection tileDirection = target2;
				obj2[3] = tileDirection.ToString();
				obj2[4] = " -> ";
				tileDirection = output.Position_L;
				obj2[5] = tileDirection.ToString();
				obj2[6] = " / ";
				obj2[7] = output.Direction_L.ToString();
				throw new Exception(string.Concat(obj2));
			}
			outputs.Add(target2);
		}
	}

	public void Init(MetaBuildingVariant variant)
	{
		Variant = variant;
		CachedBlueprintMesh = null;
		CollisionBox[] colliders = Colliders;
		foreach (CollisionBox box in colliders)
		{
			box.ComputeDimensions();
		}
		for (int j = 0; j < FluidContainers.Length; j++)
		{
			FluidContainers[j].Index = j;
		}
		int3 min_L = (int3)Tiles[0];
		int3 max_L = (int3)Tiles[0];
		for (int k = 1; k < Tiles.Length; k++)
		{
			min_L = math.min(min_L, (int3)Tiles[k]);
			max_L = math.max(max_L, (int3)Tiles[k]);
		}
		BoundsInTileSpace = new int3x2(min_L, max_L + 1);
		Height = Tiles.Max((TileDirection t) => t.z) + 1;
	}

	public float GetCurve(int index, float progress)
	{
		return AnimationCurves[index].Curve.Evaluate(progress);
	}

	public HUDSidePanelModuleBaseStat[] HUD_GetStats()
	{
		Type implementation = Implementation.Type;
		MethodInfo statMethod = implementation.GetMethod("HUD_ComputeStats", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		return (HUDSidePanelModuleBaseStat[])statMethod.Invoke(null, new object[1] { this });
	}

	public void ClearBlueprintMeshCache()
	{
		CachedBlueprintMesh?.Clear();
		CachedBlueprintMesh = null;
	}
}
