using Unity.Mathematics;
using UnityEngine;

public static class BlueprintMeshGenerator
{
	public static void GenerateMesh(MetaBuildingInternalVariant internalVariant, ref CombinedMesh target, int lod = 0)
	{
		MeshBuilder builder = new MeshBuilder(lod);
		VisualThemeBaseResources resources = Singleton<GameCore>.G.Theme.BaseResources;
		Mesh blueprintMesh = internalVariant.BlueprintMeshBase;
		if (blueprintMesh != null)
		{
			builder.AddTranslate(blueprintMesh, new float3(0f, 0f, 0f));
		}
		if (!internalVariant.BlueprintMeshOverride)
		{
			if (internalVariant.HasGlassMesh)
			{
				builder.AddTranslate(internalVariant.GlassMeshLOD, new float3(0f, 0f, 0f));
			}
			MetaBuildingInternalVariant.AdditionalBlueprintMesh[] additionalBlueprintMeshes = internalVariant.AdditionalBlueprintMeshes;
			foreach (MetaBuildingInternalVariant.AdditionalBlueprintMesh additionalMesh in additionalBlueprintMeshes)
			{
				builder.AddTRS(additionalMesh.Mesh, Matrix4x4.TRS(additionalMesh.Pos, additionalMesh.Rotate, additionalMesh.Scale));
			}
		}
		MetaBuildingInternalVariant.BeltIO[] beltInputs = internalVariant.BeltInputs;
		foreach (MetaBuildingInternalVariant.BeltIO io in beltInputs)
		{
			MeshBuilder builder2 = builder;
			MetaBuildingInternalVariant.BaseIO io2 = io;
			MetaBuildingInternalVariant.BeltIOType iOType = io.IOType;
			if (1 == 0)
			{
			}
			LOD3Mesh mesh = iOType switch
			{
				MetaBuildingInternalVariant.BeltIOType.ElevatedBorder => resources.BeltCapInputWithBorder[0], 
				MetaBuildingInternalVariant.BeltIOType.None => null, 
				_ => resources.BeltCapInput[0], 
			};
			if (1 == 0)
			{
			}
			Generate_EndCap(builder2, io2, mesh);
		}
		MetaBuildingInternalVariant.BeltIO[] beltOutputs = internalVariant.BeltOutputs;
		foreach (MetaBuildingInternalVariant.BeltIO io3 in beltOutputs)
		{
			MeshBuilder builder3 = builder;
			MetaBuildingInternalVariant.BaseIO io4 = io3;
			MetaBuildingInternalVariant.BeltIOType iOType2 = io3.IOType;
			if (1 == 0)
			{
			}
			LOD3Mesh mesh = iOType2 switch
			{
				MetaBuildingInternalVariant.BeltIOType.ElevatedBorder => resources.BeltCapOutputWithBorder[0], 
				MetaBuildingInternalVariant.BeltIOType.None => null, 
				_ => resources.BeltCapOutput[0], 
			};
			if (1 == 0)
			{
			}
			Generate_EndCap(builder3, io4, mesh);
		}
		MetaBuildingInternalVariant.FluidContainerConfig[] fluidContainers = internalVariant.FluidContainers;
		foreach (MetaBuildingInternalVariant.FluidContainerConfig container in fluidContainers)
		{
			MetaBuildingInternalVariant.FluidIO[] connections = container.Connections;
			foreach (MetaBuildingInternalVariant.FluidIO io5 in connections)
			{
				Generate_EndCap(builder, io5, Singleton<GameCore>.G.Theme.BaseResources.PipeStandsAndEndCap[math.max(0, io5.Position_L.z)]);
			}
		}
		builder.Generate(ref target);
	}

	private static void Generate_EndCap(MeshBuilder builder, MetaBuildingInternalVariant.BaseIO io, LODBaseMesh mesh)
	{
		if (mesh != null)
		{
			builder.AddTranslateRotate(mesh, (GlobalTileCoordinate.Origin + io.Position_L + io.Direction_L).ToCenter_W(), io.Direction_L);
		}
	}
}
