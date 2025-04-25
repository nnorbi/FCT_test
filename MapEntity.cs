using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public abstract class MapEntity : IPlayerSelectable
{
	public class Belts_ClearItemsTraverser : IBeltLaneTraverser
	{
		public void Traverse(BeltLane lane)
		{
			if (lane.HasItem)
			{
				lane.ClearLane();
			}
		}
	}

	public class Belts_LinkedEntity
	{
		public MapEntity Entity = null;

		public int SlotIndex = -1;

		public MetaBuildingInternalVariant.BeltIO Slot;

		public bool Conflicting = false;
	}

	public enum Config_UpdateMode
	{
		Normal,
		Never
	}

	public enum Drawing_CullMode
	{
		DrawWhenInView,
		DrawWhenIslandInView,
		DrawAlways_NEEDS_MANUAL_CULLING
	}

	public class Fluids_LinkedContainer
	{
		public int FromConnectionIndex = 0;

		public int ToConnectionIndex = 0;

		public MetaBuildingInternalVariant.FluidIO FromConnection;

		public MetaBuildingInternalVariant.FluidIO ToConnection;

		public FluidContainer Container;

		public MapEntity Entity;
	}

	public struct CtorArgs
	{
		public IslandTileCoordinate Tile_I;

		public Grid.Direction Rotation;

		public MetaBuildingInternalVariant InternalVariant;

		public Island Island;
	}

	protected static Belts_ClearItemsTraverser BELTS_CLEAR_ITEMS_TRAVERSER = new Belts_ClearItemsTraverser();

	public Grid.Direction Rotation_G;

	public MetaBuildingInternalVariant InternalVariant;

	public IslandTileCoordinate Tile_I;

	public Island Island;

	public MetaBuildingVariant Variant => InternalVariant.Variant;

	public GlobalTileCoordinate Tile_G
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Tile_I.To_G(Island);
		}
	}

	public bool Selectable => Variant.Selectable && Island.Metadata.Layout.CanModifyIslandContents;

	public virtual BeltLane Belts_GetLaneForInput(int index)
	{
		throw new Exception("Belts_GetLaneForInput(" + index + ") is not implemented on " + InternalVariant.name + "!");
	}

	public virtual BeltLane Belts_GetLaneForOutput(int index)
	{
		throw new Exception("Belts_GetLaneForOutput(" + index + ") is not implemented on " + InternalVariant.name + "!");
	}

	public virtual BeltItem Belts_GetPredictedInput(int index)
	{
		BeltLane inputLane = Belts_GetLaneForInput(index);
		return inputLane.Item;
	}

	public virtual BeltItem Belts_GetPredictedOutput(int index)
	{
		BeltLane outputLane = Belts_GetLaneForOutput(index);
		return outputLane.Item;
	}

	public Belts_LinkedEntity[] Belts_GetInputConnections()
	{
		MetaBuildingInternalVariant.BeltIO[] inputs = InternalVariant.BeltInputs;
		Belts_LinkedEntity[] result = new Belts_LinkedEntity[inputs.Length];
		for (int i = 0; i < inputs.Length; i++)
		{
			result[i] = new Belts_LinkedEntity();
			MetaBuildingInternalVariant.BeltIO localInput = inputs[i];
			IslandTileCoordinate inputTile_I = localInput.Position_L.To_I(this);
			Grid.Direction inputDirection_I = I_From_L_Direction(localInput.Direction_L);
			IslandTileCoordinate sourceTile_I = inputTile_I.NeighbourTile(inputDirection_I);
			MapEntity sourceEntity = Island.GetEntity_I(in sourceTile_I);
			if (sourceEntity == null)
			{
				continue;
			}
			result[i].Conflicting = true;
			MetaBuildingInternalVariant.BeltIO[] outputs = sourceEntity.InternalVariant.BeltOutputs;
			for (int outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
			{
				MetaBuildingInternalVariant.BeltIO output = outputs[outputIndex];
				Grid.Direction outputDirection_I = sourceEntity.I_From_L_Direction(output.Direction_L);
				IslandTileCoordinate outputTile_I = output.Position_L.To_I(sourceEntity);
				if (outputTile_I.NeighbourTile(outputDirection_I).Equals(inputTile_I) && sourceTile_I.Equals(outputTile_I))
				{
					result[i] = new Belts_LinkedEntity
					{
						Entity = sourceEntity,
						SlotIndex = outputIndex,
						Slot = output
					};
				}
			}
		}
		return result;
	}

	public Belts_LinkedEntity[] Belts_GetOutputConnections()
	{
		MetaBuildingInternalVariant.BeltIO[] outputs = InternalVariant.BeltOutputs;
		Belts_LinkedEntity[] result = new Belts_LinkedEntity[outputs.Length];
		for (int i = 0; i < outputs.Length; i++)
		{
			result[i] = new Belts_LinkedEntity();
			MetaBuildingInternalVariant.BeltIO localOutput = outputs[i];
			IslandTileCoordinate outputTile_I = localOutput.Position_L.To_I(this);
			Grid.Direction outputDirection_I = I_From_L_Direction(localOutput.Direction_L);
			IslandTileCoordinate destinationTile_I = outputTile_I.NeighbourTile(outputDirection_I);
			MapEntity destinationEntity = Island.GetEntity_I(in destinationTile_I);
			if (destinationEntity == null)
			{
				continue;
			}
			result[i].Conflicting = true;
			MetaBuildingInternalVariant.BeltIO[] inputs = destinationEntity.InternalVariant.BeltInputs;
			for (int inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
			{
				MetaBuildingInternalVariant.BeltIO input = inputs[inputIndex];
				Grid.Direction inputDirection_I = destinationEntity.I_From_L_Direction(input.Direction_L);
				IslandTileCoordinate inputTile_I = input.Position_L.To_I(destinationEntity);
				if (inputTile_I.NeighbourTile(inputDirection_I).Equals(outputTile_I) && destinationTile_I.Equals(inputTile_I))
				{
					result[i] = new Belts_LinkedEntity
					{
						Entity = destinationEntity,
						SlotIndex = inputIndex,
						Slot = input
					};
					break;
				}
			}
		}
		return result;
	}

	protected virtual void Belts_LinkLanesAfterCreate()
	{
		Belts_LinkedEntity[] connectedInputs = Belts_GetInputConnections();
		for (int i = 0; i < connectedInputs.Length; i++)
		{
			Belts_LinkedEntity connection = connectedInputs[i];
			if (connection.Entity != null)
			{
				BeltLane lane = connection.Entity.Belts_GetLaneForOutput(connection.SlotIndex);
				if (lane.NextLane != null)
				{
					throw new Exception("On lane link: next=" + lane.NextLane?.ToString() + " but should be null");
				}
				lane.NextLane = Belts_GetLaneForInput(i);
			}
		}
		Belts_LinkedEntity[] connectedOutputs = Belts_GetOutputConnections();
		for (int j = 0; j < connectedOutputs.Length; j++)
		{
			Belts_LinkedEntity connection2 = connectedOutputs[j];
			if (connection2.Entity != null)
			{
				BeltLane nextLane = connection2.Entity.Belts_GetLaneForInput(connection2.SlotIndex);
				BeltLane lane2 = Belts_GetLaneForOutput(j);
				lane2.NextLane = nextLane;
			}
		}
	}

	protected virtual void Belts_UnlinkLanesAfterDestroy()
	{
		Belts_LinkedEntity[] connected = Belts_GetInputConnections();
		for (int i = 0; i < connected.Length; i++)
		{
			Belts_LinkedEntity connection = connected[i];
			if (connection.Entity != null)
			{
				BeltLane lane = connection.Entity.Belts_GetLaneForOutput(connection.SlotIndex);
				if (lane.NextLane != Belts_GetLaneForInput(i))
				{
					throw new Exception("On lane unlink: next=" + lane.NextLane?.ToString() + " but should be " + Belts_GetLaneForInput(i));
				}
				lane.NextLane = null;
			}
		}
	}

	public virtual void Belts_TraverseLanes(IBeltLaneTraverser traverser)
	{
		for (int i = 0; i < InternalVariant.BeltInputs.Length; i++)
		{
			traverser.Traverse(Belts_GetLaneForInput(i));
		}
		for (int j = 0; j < InternalVariant.BeltOutputs.Length; j++)
		{
			traverser.Traverse(Belts_GetLaneForOutput(j));
		}
		Belts_TraverseAdditionalLanes(traverser);
	}

	protected virtual void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
	}

	protected virtual void Belts_SerializeLanes(ISerializationVisitor visitor)
	{
		Belts_TraverseLanes(visitor);
	}

	public virtual void Belts_ClearContents()
	{
		Belts_TraverseLanes(BELTS_CLEAR_ITEMS_TRAVERSER);
	}

	public virtual BeltItem Belts_ComputeRepresentativeShapeTransferItem()
	{
		return null;
	}

	public virtual HashSet<MapEntity> Belts_GetDependencies()
	{
		HashSet<MapEntity> result = new HashSet<MapEntity>();
		Belts_LinkedEntity[] inputs = Belts_GetOutputConnections();
		Belts_LinkedEntity[] array = inputs;
		foreach (Belts_LinkedEntity input in array)
		{
			if (input.Entity != null)
			{
				result.Add(input.Entity);
			}
		}
		return result;
	}

	public virtual Config_UpdateMode Order_ComputeUpdateMode()
	{
		return Config_UpdateMode.Normal;
	}

	public virtual Drawing_CullMode Order_GetCullMode()
	{
		return Drawing_CullMode.DrawWhenInView;
	}

	protected void DrawDynamic_BeltLane(FrameDrawOptions options, BeltLane lane, float rotation_L = 0f)
	{
		if (lane.HasItem)
		{
			DrawDynamic_BeltItem(options, lane.Item, lane.Definition.GetPosFromTicks_L(lane.Progress_T), rotation_L);
		}
	}

	protected void DrawDynamic_BeltItem(FrameDrawOptions options, BeltItem item, in float3 pos_L, float rotationDegrees_G = 0f)
	{
		float3 pos_W = W_From_L(pos_L + new float3(0f, 0f, Globals.Resources.BeltShapeHeight));
		options.ShapeInstanceManager.AddInstance(item.GetDefaultInstancingKey(), item.GetMesh(), item.GetMaterial(), FastMatrix.TranslateRotateDegrees(in pos_W, rotationDegrees_G));
	}

	protected void DrawDynamic_RawShape(FrameDrawOptions options, ShapeDefinition definition, in float3 pos_L, float rotationDegrees_G = 0f)
	{
		float3 pos_W = W_From_L(in pos_L);
		options.ShapeInstanceManager.AddInstance(definition.InstancingID, definition.GetMesh(), Globals.Resources.ShapeMaterial, FastMatrix.TranslateRotateDegrees(in pos_W, rotationDegrees_G));
	}

	protected void DrawDynamic_LeftShapeSupportMesh(FrameDrawOptions options, in float3 pos_L, float alpha = 1f)
	{
		if (!(alpha < 0.01f))
		{
			CachedInstancingMesh leftSupport = ShapeItem.LEFT_SUPPORT_MESH;
			if (alpha > 0.995f)
			{
				options.ShapeInstanceManager.AddInstance(leftSupport.InstancingID, leftSupport.Mesh, Globals.Resources.ShapeMaterial, FastMatrix.Translate(W_From_L(in pos_L)));
			}
			else
			{
				options.RegularRenderer.DrawMesh(leftSupport.Mesh, FastMatrix.Translate(W_From_L(in pos_L)), Globals.Resources.ShapeMaterialFadeout, RenderCategory.Shapes, MaterialPropertyHelpers.CreateAlphaBlock(alpha));
			}
		}
	}

	protected void DrawDynamic_LeftShapeSupportMeshRaw(FrameDrawOptions options, in Matrix4x4 transform, float alpha = 1f)
	{
		if (!(alpha < 0.01f))
		{
			CachedInstancingMesh leftSupport = ShapeItem.LEFT_SUPPORT_MESH;
			if (alpha > 0.995f)
			{
				options.ShapeInstanceManager.AddInstance(leftSupport.InstancingID, leftSupport.Mesh, Globals.Resources.ShapeMaterial, in transform);
			}
			else
			{
				options.RegularRenderer.DrawMesh(leftSupport.Mesh, in transform, Globals.Resources.ShapeMaterialFadeout, RenderCategory.Shapes, MaterialPropertyHelpers.CreateAlphaBlock(alpha));
			}
		}
	}

	protected void DrawDynamic_RightShapeSupportMesh(FrameDrawOptions options, in float3 pos_L, float alpha = 1f)
	{
		if (!(alpha < 0.01f))
		{
			CachedInstancingMesh rightSupport = ShapeItem.RIGHT_SUPPORT_MESH;
			if (alpha > 0.995f)
			{
				options.ShapeInstanceManager.AddInstance(rightSupport.InstancingID, rightSupport.Mesh, Globals.Resources.ShapeMaterial, FastMatrix.Translate(W_From_L(in pos_L)));
			}
			else
			{
				options.RegularRenderer.DrawMesh(rightSupport.Mesh, FastMatrix.Translate(W_From_L(in pos_L)), Globals.Resources.ShapeMaterialFadeout, RenderCategory.Shapes, MaterialPropertyHelpers.CreateAlphaBlock(alpha));
			}
		}
	}

	protected void DrawDynamic_RightShapeSupportMeshRaw(FrameDrawOptions options, in Matrix4x4 transform, float alpha = 1f)
	{
		if (!(alpha < 0.01f))
		{
			CachedInstancingMesh rightSupport = ShapeItem.RIGHT_SUPPORT_MESH;
			if (alpha > 0.995f)
			{
				options.ShapeInstanceManager.AddInstance(rightSupport.InstancingID, rightSupport.Mesh, Globals.Resources.ShapeMaterial, in transform);
			}
			else
			{
				options.RegularRenderer.DrawMesh(rightSupport.Mesh, in transform, Globals.Resources.ShapeMaterialFadeout, RenderCategory.Shapes, MaterialPropertyHelpers.CreateAlphaBlock(alpha));
			}
		}
	}

	protected void DrawDynamic_ShapeCollapseResult(FrameDrawOptions options, ShapeCollapseResult result, in float3 pos_L, float progress_FallDown, float progress_ScaleX, float progress_ScaleY, float rotationDegrees_G = 0f, float alpha = 1f)
	{
		DrawDynamic_ShapeCollapseResult(options, result, in pos_L, progress_FallDown, progress_ScaleX, progress_ScaleY, InstancedShapeDrawer.Default, rotationDegrees_G, alpha);
	}

	protected void DrawDynamic_ShapeCollapseResult<T>(FrameDrawOptions options, ShapeCollapseResult result, in float3 pos_L, float progress_FallDown, float progress_ScaleX, float progress_ScaleY, T mainShapeDrawer, float rotationDegrees_G = 0f, float alpha = 1f) where T : IShapeDrawer
	{
		if (alpha < 0.01f)
		{
			return;
		}
		if (progress_FallDown > 0.995f && progress_ScaleX > 0.995f && progress_ScaleY > 0.995f && alpha > 0.995f)
		{
			if (!result.ResultsInEmptyShape)
			{
				ShapeDefinition definition = Singleton<GameCore>.G.Shapes.GetDefinitionByHash(result.ResultDefinition);
				if (definition == null)
				{
					Debug.LogError("DrawDynamic_ShapeCollapseResult: Definition = null: " + result.ResultDefinition);
					return;
				}
				Matrix4x4 transform = FastMatrix.TranslateRotateDegrees(W_From_L(in pos_L), rotationDegrees_G);
				mainShapeDrawer.DrawShape(options, definition, transform);
			}
			return;
		}
		float3 pos_W = W_From_L(in pos_L);
		for (int i = 0; i < result.Entries.Length; i++)
		{
			ShapeCollapseResultEntry entry = result.Entries[i];
			float sourceScale = ShapeLogic.Logic_LayerScale(entry.FallDownLayers);
			float scaleX = math.lerp(sourceScale, 1f, progress_ScaleX);
			float scaleY = math.lerp(sourceScale, 1f, progress_ScaleY);
			float heightOffset = math.lerp(Globals.Resources.ShapeLayerHeight * (float)entry.FallDownLayers, 0f, progress_FallDown);
			ShapeDefinition definition2 = Singleton<GameCore>.G.Shapes.GetDefinitionByHash(entry.ShapeDefinition);
			Matrix4x4 transform2 = Matrix4x4.TRS(pos_W + new float3(0f, heightOffset, 0f), FastMatrix.RotateYAngle(rotationDegrees_G), new Vector3(scaleX, 1f, scaleY));
			if (entry.Vanish || alpha < 0.995f)
			{
				MaterialPropertyBlock wastePropertyBlock = MaterialPropertyHelpers.CreateAlphaBlock((1f - progress_FallDown) * alpha);
				options.RegularRenderer.DrawMesh(definition2.GetMesh(), in transform2, Globals.Resources.ShapeMaterialFadeout, RenderCategory.Shapes, wastePropertyBlock);
			}
			else
			{
				Matrix4x4 transform3 = transform2;
				mainShapeDrawer.DrawShape(options, definition2, transform3);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void DrawDynamic_Main(FrameDrawOptions options)
	{
		Hook_OnDrawDynamic(options);
		if (InternalVariant.RenderVoidBelow)
		{
			for (int i = 0; i < InternalVariant.Tiles.Length; i++)
			{
				IslandTileCoordinate tile_I = InternalVariant.Tiles[i].To_I(this);
				options.Theme.Draw_RenderVoid(options, Island, tile_I);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("Do not use a DrawDynamic method with a Matrix4x4 - use lodMesh.TryGet instead, to avoid computing the matrix just to find out the object will not be rendered.")]
	protected void DrawDynamic_Mesh(FrameDrawOptions options, LODBaseMesh lodMesh, in Matrix4x4 trs)
	{
		throw new NotImplementedException("This method doesn't exist for a good reason.");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void DrawDynamic_Mesh(FrameDrawOptions options, LODBaseMesh lodMesh, in float3 pos_L)
	{
		if (lodMesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh reference))
		{
			options.DynamicBuildingsInstanceManager.AddInstance(reference, options.Theme.BaseResources.BuildingMaterial, FastMatrix.TranslateRotate(W_From_L(in pos_L), Rotation_G));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void DrawDynamic_Mesh(FrameDrawOptions options, LODBaseMesh lodMesh, in float3 pos_L, Grid.Direction rotation_L)
	{
		if (lodMesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh reference))
		{
			options.DynamicBuildingsInstanceManager.AddInstance(reference, options.Theme.BaseResources.BuildingMaterial, FastMatrix.TranslateRotate(W_From_L(in pos_L), Grid.RotateDirection(rotation_L, Rotation_G)));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void DrawDynamic_Mesh(FrameDrawOptions options, LOD2Mesh lodMesh, in float3 pos_L, float rotationDegrees_L)
	{
		if (lodMesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh reference))
		{
			float rotationDegrees_G = Grid.DirectionToDegrees(Rotation_G) + rotationDegrees_L;
			options.DynamicBuildingsInstanceManager.AddInstance(reference, options.Theme.BaseResources.BuildingMaterial, FastMatrix.TranslateRotateDegrees(W_From_L(in pos_L), rotationDegrees_G));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void DrawDynamic_Mesh(FrameDrawOptions options, LOD2Mesh lodMesh, in float3 pos_L, in float3 scale_L)
	{
		if (lodMesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh reference))
		{
			options.DynamicBuildingsInstanceManager.AddInstance(reference, options.Theme.BaseResources.BuildingMaterial, Matrix4x4.TRS(W_From_L(in pos_L), FastMatrix.RotateY(Rotation_G), Grid.Scale_W_From_G(in scale_L)));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void DrawDynamic_MeshGlobalRotation(FrameDrawOptions options, LODBaseMesh lodMesh, in float3 pos_L)
	{
		if (lodMesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh reference))
		{
			options.DynamicBuildingsInstanceManager.AddInstance(reference, options.Theme.BaseResources.BuildingMaterial, FastMatrix.Translate(W_From_L(in pos_L)));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void DrawDynamic_MeshGlobalRotation(FrameDrawOptions options, LODBaseMesh lodMesh, in float3 pos_L, Grid.Direction rotation_G)
	{
		if (lodMesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh reference))
		{
			options.DynamicBuildingsInstanceManager.AddInstance(reference, options.Theme.BaseResources.BuildingMaterial, FastMatrix.TranslateRotate(W_From_L(in pos_L), rotation_G));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void DrawDynamic_MeshGlobalRotation(FrameDrawOptions options, LODBaseMesh lodMesh, in float3 pos_L, float rotationDegrees_G)
	{
		if (lodMesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh reference))
		{
			options.DynamicBuildingsInstanceManager.AddInstance(reference, options.Theme.BaseResources.BuildingMaterial, FastMatrix.TranslateRotateDegrees(W_From_L(in pos_L), rotationDegrees_G));
		}
	}

	public virtual FluidContainer Fluids_GetContainerByIndex(int index)
	{
		throw new Exception("Entity has no fluid containers!");
	}

	public virtual void Fluids_OnContainedContainerFlushed(FluidContainer contained)
	{
	}

	public List<Fluids_LinkedContainer> Fluids_GetConnectedContainers(MetaBuildingInternalVariant.FluidContainerConfig containerConfig)
	{
		MetaBuildingInternalVariant.FluidIO[] ourConnections = containerConfig.Connections;
		List<Fluids_LinkedContainer> result = new List<Fluids_LinkedContainer>();
		for (int ourConnectionIndex = 0; ourConnectionIndex < ourConnections.Length; ourConnectionIndex++)
		{
			MetaBuildingInternalVariant.FluidIO ourConnection = ourConnections[ourConnectionIndex];
			IslandTileCoordinate ourConnectionTile_I = ourConnection.Position_L.To_I(this);
			Grid.Direction ourConnectionDirection_I = I_From_L_Direction(ourConnection.Direction_L);
			IslandTileCoordinate ourDestinationTile_I = ourConnectionTile_I.NeighbourTile(ourConnectionDirection_I);
			MapEntity otherEntity = Island.GetEntity_I(in ourDestinationTile_I);
			if (otherEntity == null)
			{
				continue;
			}
			MetaBuildingInternalVariant.FluidContainerConfig[] otherContainers = otherEntity.InternalVariant.FluidContainers;
			for (int otherContainerIndex = 0; otherContainerIndex < otherContainers.Length; otherContainerIndex++)
			{
				MetaBuildingInternalVariant.FluidIO[] otherConnections = otherContainers[otherContainerIndex].Connections;
				for (int otherConnectionIndex = 0; otherConnectionIndex < otherConnections.Length; otherConnectionIndex++)
				{
					MetaBuildingInternalVariant.FluidIO otherConnection = otherConnections[otherConnectionIndex];
					IslandTileCoordinate otherConnectionTile_I = otherConnection.Position_L.To_I(otherEntity);
					Grid.Direction otherConnectionDirection_I = otherEntity.I_From_L_Direction(otherConnection.Direction_L);
					if (otherConnectionTile_I.NeighbourTile(otherConnectionDirection_I).Equals(ourConnectionTile_I) && ourDestinationTile_I.Equals(otherConnectionTile_I))
					{
						result.Add(new Fluids_LinkedContainer
						{
							FromConnectionIndex = ourConnectionIndex,
							ToConnectionIndex = otherConnectionIndex,
							FromConnection = ourConnection,
							ToConnection = otherConnection,
							Container = otherEntity.Fluids_GetContainerByIndex(otherContainerIndex),
							Entity = otherEntity
						});
						break;
					}
				}
			}
		}
		return result;
	}

	protected void Fluids_RegisterContainers()
	{
		for (int i = 0; i < InternalVariant.FluidContainers.Length; i++)
		{
			FluidContainer container = Fluids_GetContainerByIndex(i);
			FluidNetwork.InsertToNetwork(container, Fluids_GetConnectedContainers(container.Config));
		}
	}

	protected void Fluids_UnregisterContainers()
	{
		for (int i = 0; i < InternalVariant.FluidContainers.Length; i++)
		{
			FluidContainer container = Fluids_GetContainerByIndex(i);
			FluidNetwork.RemoveFromNetwork(container);
		}
	}

	protected virtual void Fluids_SerializeContainers(ISerializationVisitor visitor)
	{
		for (int i = 0; i < InternalVariant.FluidContainers.Length; i++)
		{
			FluidContainer container = Fluids_GetContainerByIndex(i);
			container.Sync(visitor);
		}
	}

	public virtual void Fluids_ClearContents()
	{
		for (int i = 0; i < InternalVariant.FluidContainers.Length; i++)
		{
			FluidContainer container = Fluids_GetContainerByIndex(i);
			container.Flush();
		}
	}

	public virtual HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[0];
	}

	public static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return 1f;
	}

	public static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[0];
	}

	public static MetaResearchSpeed HUD_GetResearchSpeed(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Speed;
	}

	protected static HUDSidePanelModuleStatProcessingTime HUD_CreateProcessingTimeStat(float processingDuration, MetaResearchSpeed researchSpeed)
	{
		return new HUDSidePanelModuleStatProcessingTime(processingDuration / ((float)Singleton<GameCore>.G.Research.SpeedManager.GetSpeedValue(researchSpeed) / 100f), researchSpeed);
	}

	public byte[] Serialization_SerializeSingle(bool serializeContents, bool serializeConfig)
	{
		using MemoryStream stream = new MemoryStream();
		BinarySerializationVisitor serializer = new BinarySerializationVisitor(writing: true, checkpoints: false, Savegame.VERSION, stream);
		if (serializeContents)
		{
			Serialization_SyncContents(serializer);
		}
		if (serializeConfig)
		{
			Serialization_SyncConfig(serializer);
		}
		stream.Flush();
		return stream.ToArray() ?? new byte[0];
	}

	public void Serialization_DeserializeSingle(byte[] data, bool deserializeContents, bool deserializeConfig)
	{
		using MemoryStream stream = new MemoryStream(data);
		BinarySerializationVisitor serializer = new BinarySerializationVisitor(writing: false, checkpoints: false, Savegame.VERSION, stream);
		if (deserializeContents)
		{
			Serialization_SyncContents(serializer);
		}
		if (deserializeConfig)
		{
			Serialization_SyncConfig(serializer);
		}
	}

	public virtual void Serialization_SyncContents(ISerializationVisitor visitor)
	{
		Belts_SerializeLanes(visitor);
		Fluids_SerializeContainers(visitor);
		Hook_SyncAdditionalContents(visitor);
	}

	public virtual void Serialization_SyncConfig(ISerializationVisitor visitor)
	{
		Hook_SyncConfig(visitor);
	}

	public virtual void Serialization_SyncLate(ISerializationVisitor visitor)
	{
		Hook_SyncLate(visitor);
	}

	public virtual void DrawStatic_Main(MeshBuilder builder)
	{
		DrawStatic_BaseMesh(builder);
		if (builder.TargetLOD <= 3)
		{
			DrawStatic_EndCaps(builder);
		}
	}

	public virtual void DrawStatic_Glass(MeshBuilder builder)
	{
		if (InternalVariant.HasGlassMesh)
		{
			float3 pos_W = Tile_I.To_W(Island);
			builder.AddTRS(InternalVariant.GlassMeshLOD, FastMatrix.TranslateRotate(in pos_W, Rotation_G));
		}
	}

	protected virtual void DrawStatic_BaseMesh(MeshBuilder builder)
	{
		if (!InternalVariant.HasMainMesh)
		{
			return;
		}
		if (InternalVariant.IndividualMainMeshPerLayer)
		{
			LOD4Mesh[] meshes = InternalVariant.MainMeshPerLayerLOD;
			int meshIndex = math.min(Tile_I.z, meshes.Length - 1);
			if (meshes[meshIndex].TryGet(builder.TargetLOD, out LODBaseMesh.CachedMesh entry))
			{
				builder.AddTranslateRotate(entry, Tile_I.To_W(Island), Rotation_G);
			}
		}
		else
		{
			builder.AddTranslateRotate(InternalVariant.MainMeshLOD, Tile_I.To_W(Island), Rotation_G);
		}
	}

	protected virtual int DrawStatic_GetStandHeight_L(in TileDirection tile_L)
	{
		int distanceToFloor = tile_L.z + Tile_I.z;
		IslandTileCoordinate standTile_I = tile_L.To_I(this);
		return distanceToFloor - Island.GetTileInfo_UNSAFE_I(in standTile_I).Height;
	}

	protected virtual void DrawStatic_EndCaps(MeshBuilder builder)
	{
		VisualThemeBaseResources resources = Singleton<GameCore>.G.Theme.BaseResources;
		DrawStatic_BeltEndCaps(builder, Belts_GetInputConnections(), InternalVariant.BeltInputs, resources.BeltCapInput, resources.BeltCapInputWithBorder, resources.BeltCapInputBorderOnly, resources.BeltCapInputConflict);
		DrawStatic_BeltEndCaps(builder, Belts_GetOutputConnections(), InternalVariant.BeltOutputs, resources.BeltCapOutput, resources.BeltCapOutputWithBorder, resources.BeltCapOutputBorderOnly, resources.BeltCapOutputConflict);
		DrawStatic_FluidEndCapsAndStands(builder);
	}

	protected virtual void DrawStatic_BeltEndCaps(MeshBuilder builder, Belts_LinkedEntity[] linked, MetaBuildingInternalVariant.BeltIO[] ioSlots, LOD3Mesh[] capMesh, LOD3Mesh[] capMeshWithBorder, LOD3Mesh[] capMeshBorderOnly, LOD3Mesh[] capMeshConflict)
	{
		VisualThemeBaseResources themeResources = Singleton<GameCore>.G.Theme.BaseResources;
		for (int i = 0; i < ioSlots.Length; i++)
		{
			MetaBuildingInternalVariant.BeltIO ioSlot = ioSlots[i];
			Belts_LinkedEntity link = linked[i];
			MetaBuildingInternalVariant.BeltIO otherSlot = link.Slot;
			int layer = Tile_I.z + ioSlot.Position_L.z;
			if (ioSlot.Seperators)
			{
				if (link.Entity != null && link.Slot.Seperators)
				{
					if (ComputeHasPriority(link.Entity))
					{
						DrawStatic_Separator(builder, ioSlot, themeResources.BuildingSeperatorsShared);
					}
				}
				else
				{
					DrawStatic_Separator(builder, ioSlot, themeResources.BuildingSeperators);
				}
			}
			if (link.Conflicting)
			{
				DrawStatic_EndCap(builder, ioSlot, capMeshConflict[math.min(layer, capMeshConflict.Length - 1)]);
			}
			else
			{
				switch (ioSlot.IOType)
				{
				case MetaBuildingInternalVariant.BeltIOType.Regular:
					if (link.Entity == null)
					{
						DrawStatic_EndCap(builder, ioSlot, capMesh[math.min(layer, capMesh.Length - 1)]);
					}
					break;
				case MetaBuildingInternalVariant.BeltIOType.ElevatedBorder:
					if (link.Entity == null)
					{
						DrawStatic_EndCap(builder, ioSlot, capMeshWithBorder[math.min(layer, capMeshWithBorder.Length - 1)]);
					}
					else if (otherSlot.IOType != MetaBuildingInternalVariant.BeltIOType.ElevatedBorder && !otherSlot.Seperators)
					{
						DrawStatic_EndCap(builder, ioSlot, capMeshBorderOnly[math.min(layer, capMeshBorderOnly.Length - 1)]);
					}
					break;
				}
			}
			bool shouldDrawStand = link.Entity == null || ComputeHasPriority(link.Entity) || otherSlot.StandType == MetaBuildingInternalVariant.BeltStandType.None;
			if (link.Entity != null && shouldDrawStand)
			{
				Grid.Direction direction_I = I_From_L_Direction(ioSlot.Direction_L);
				IslandTileCoordinate pos_I = ioSlot.Position_L.To_I(this);
				int2 pos_Stand = new int2(pos_I.x, pos_I.y) * 2 * 2 + Grid.DirectionToUnitVector(direction_I);
				shouldDrawStand = ((direction_I != Grid.Direction.Right && direction_I != Grid.Direction.Left) ? (shouldDrawStand && pos_Stand.y % (2 * Globals.Resources.DistanceBetweenStands) == 1) : (shouldDrawStand && pos_Stand.x % (2 * Globals.Resources.DistanceBetweenStands) == 1));
			}
			MetaBuildingInternalVariant.BeltStandType standType = ioSlot.StandType;
			MetaBuildingInternalVariant.BeltStandType beltStandType = standType;
			if (beltStandType != MetaBuildingInternalVariant.BeltStandType.None && beltStandType == MetaBuildingInternalVariant.BeltStandType.Normal && shouldDrawStand)
			{
				DrawStatic_BeltStands(builder, in ioSlot.Position_L, themeResources.BeltCapStandsNormal, ioSlot.Direction_L);
			}
		}
	}

	protected virtual void DrawStatic_BeltStands(MeshBuilder builder, in TileDirection stand_L, LOD4Mesh[] standMeshes, Grid.Direction rotation_L)
	{
		int standHeight = DrawStatic_GetStandHeight_L(in stand_L);
		if (standHeight > 0)
		{
			int index = math.min(standHeight - 1, standMeshes.Length - 1);
			LOD4Mesh standMesh = standMeshes[index];
			if (standMesh != null)
			{
				builder.AddTranslateRotate(standMesh, stand_L.ToCenter_W(this), Grid.OppositeDirection(Grid.RotateDirection(Rotation_G, rotation_L)));
			}
		}
	}

	protected virtual void DrawStatic_FluidEndCapsAndStands(MeshBuilder builder)
	{
		MetaBuildingInternalVariant.FluidContainerConfig[] containers = InternalVariant.FluidContainers;
		VisualThemeBaseResources resources = Singleton<GameCore>.G.Theme.BaseResources;
		for (int containerIndex = 0; containerIndex < containers.Length; containerIndex++)
		{
			MetaBuildingInternalVariant.FluidIO[] connections = containers[containerIndex].Connections;
			List<Fluids_LinkedContainer> linkedContainers = Fluids_GetConnectedContainers(containers[containerIndex]);
			FluidContainer container = Fluids_GetContainerByIndex(containerIndex);
			int connectionIndex = 0;
			while (connectionIndex < connections.Length)
			{
				MetaBuildingInternalVariant.FluidIO connection = connections[connectionIndex];
				int layer = connection.Position_L.z + Tile_I.z;
				Fluids_LinkedContainer link = linkedContainers.Find((Fluids_LinkedContainer linked) => linked.FromConnectionIndex == connectionIndex);
				if (link == null)
				{
					LOD4Mesh[] meshes = ((connection.IOType == MetaBuildingInternalVariant.FluidIOType.Building) ? resources.PipeBuildingStandsAndEndCap : resources.PipeStandsAndEndCap);
					DrawStatic_EndCap(builder, connection, meshes[math.min(layer, meshes.Length - 1)]);
				}
				else if (container.HasRightToUpdate(link.Container))
				{
					if (link.FromConnection.IOType == MetaBuildingInternalVariant.FluidIOType.Pipe && link.ToConnection.IOType == MetaBuildingInternalVariant.FluidIOType.Pipe)
					{
						DrawStatic_PipeStand(builder, DrawStatic_GetStandHeight_L(in connection.Position_L), connection.Position_L, connection.Direction_L);
					}
					else
					{
						DrawStatic_EndCap(builder, connection, resources.PipeBuildingConnector);
					}
				}
				int num = connectionIndex + 1;
				connectionIndex = num;
			}
		}
	}

	protected virtual void DrawStatic_EndCap(MeshBuilder builder, MetaBuildingInternalVariant.BaseIO io, LODBaseMesh mesh)
	{
		builder.AddTranslateRotate(mesh, GetIOTargetTile_I(io).To_W(Island), Grid.RotateDirection(Rotation_G, io.Direction_L));
	}

	protected virtual void DrawStatic_Separator(MeshBuilder builder, MetaBuildingInternalVariant.BaseIO io, LODBaseMesh mesh)
	{
		builder.AddTranslateRotate(mesh, GetIOTargetTile_I(io).To_W(Island), Grid.RotateDirection(Rotation_G, io.Direction_L));
	}

	protected virtual void DrawStatic_PipeStand(MeshBuilder builder, int standHeight, TileDirection stand_L, Grid.Direction direction_L)
	{
		LOD4Mesh[] meshes = Singleton<GameCore>.G.Theme.BaseResources.PipeStandsBetweenPipes;
		standHeight = math.min(standHeight, meshes.Length - 1);
		bool shouldDrawStand = true;
		Grid.Direction direction_I = I_From_L_Direction(direction_L);
		IslandTileCoordinate pos_I = stand_L.To_I(this);
		int2 pos_Stand = new int2(pos_I.x, pos_I.y) * 2 + Grid.DirectionToUnitVector(direction_I);
		if ((direction_I != Grid.Direction.Right && direction_I != Grid.Direction.Left) ? (pos_Stand.y % (2 * Globals.Resources.DistanceBetweenStands) == 1) : (pos_Stand.x % (2 * Globals.Resources.DistanceBetweenStands) == 1))
		{
			LOD4Mesh standMesh = meshes[standHeight];
			if (standMesh != null)
			{
				builder.AddTranslateRotate(standMesh, stand_L.ToCenter_W(this), Grid.RotateDirection(Rotation_G, direction_L));
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 W_From_L(in float3 tile_L)
	{
		float3 tile_I = I_From_L(in tile_L);
		return Island.W_From_I(in tile_I);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 I_From_L(in float3 tile_L)
	{
		return (int3)Tile_I + Grid.Rotate(in tile_L, Rotation_G);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 L_From_I(in float3 tile_I)
	{
		float2 xy = Grid.RotateInverse(tile_I.xy - ((int3)Tile_I).xy, Rotation_G);
		return new float3(xy, tile_I.z - (float)Tile_I.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 L_From_W(in float3 tile_W)
	{
		return L_From_I(Island.I_From_W(in tile_W));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 I_From_L(in float3 tile_L, Grid.Direction rotation, in IslandTileCoordinate baseTile_IorG)
	{
		float2 xy = Grid.Rotate(tile_L.xy, rotation) + ((int3)baseTile_IorG).xy;
		return new float3(xy, tile_L.z + (float)baseTile_IorG.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Grid.Direction I_From_L_Direction(Grid.Direction direction_L)
	{
		return Grid.RotateDirection(direction_L, Rotation_G);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Grid.Direction L_From_I_Direction(Grid.Direction direction_G)
	{
		return Grid.RotateDirectionInverse(direction_G, Rotation_G);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IslandTileCoordinate GetIOTargetTile_I(MetaBuildingInternalVariant.BaseIO definition)
	{
		IslandTileCoordinate outputTile_I = definition.Position_L.To_I(this);
		Grid.Direction outputDirection_I = I_From_L_Direction(definition.Direction_L);
		return outputTile_I.NeighbourTile(outputDirection_I);
	}

	public MapEntity(CtorArgs payload)
	{
		Tile_I = payload.Tile_I;
		Rotation_G = payload.Rotation;
		InternalVariant = payload.InternalVariant;
		Island = payload.Island;
	}

	protected bool ComputeHasPriority(MapEntity other)
	{
		GlobalChunkCoordinate thisIslandFirstChunk_GC = Island.Layout.Chunks[0].Tile_IC.To_GC(Island);
		GlobalChunkCoordinate otherIslandFirstChunk_GC = other.Island.Layout.Chunks[0].Tile_IC.To_GC(other.Island);
		if (thisIslandFirstChunk_GC.x != otherIslandFirstChunk_GC.x)
		{
			return thisIslandFirstChunk_GC.x > otherIslandFirstChunk_GC.x;
		}
		if (thisIslandFirstChunk_GC.y != otherIslandFirstChunk_GC.y)
		{
			return thisIslandFirstChunk_GC.y > otherIslandFirstChunk_GC.y;
		}
		if (other.Tile_I.x != Tile_I.x)
		{
			return other.Tile_I.x > Tile_I.x;
		}
		if (other.Tile_I.y != Tile_I.y)
		{
			return other.Tile_I.y > Tile_I.y;
		}
		if (other.Tile_I.z != Tile_I.z)
		{
			return other.Tile_I.z > Tile_I.z;
		}
		IslandTileCoordinate tile_I = Tile_I;
		string text = tile_I.ToString();
		tile_I = other.Tile_I;
		Debug.LogWarning("Entities could not be distinguished: " + text + " vs " + tile_I.ToString());
		return false;
	}

	public bool IsAliveAndNotDestroyed()
	{
		if (Island.ChunkLookup_C == null)
		{
			return false;
		}
		return Island.GetEntity_I(in Tile_I) == this;
	}

	public void OnCreated()
	{
		Hook_OnCreated();
		Fluids_RegisterContainers();
		Belts_LinkLanesAfterCreate();
		Hook_AfterCreated();
	}

	public void OnDestroyed()
	{
		Fluids_UnregisterContainers();
		Hook_OnDestroyed();
		Belts_UnlinkLanesAfterDestroy();
	}

	public void OnUpdate(TickOptions options)
	{
		Hook_OnUpdate(options);
	}

	protected virtual void Hook_SyncAdditionalContents(ISerializationVisitor visitor)
	{
	}

	protected virtual void Hook_SyncConfig(ISerializationVisitor visitor)
	{
	}

	protected virtual void Hook_SyncLate(ISerializationVisitor visitor)
	{
	}

	protected virtual void Hook_OnCreated()
	{
	}

	protected virtual void Hook_AfterCreated()
	{
	}

	protected virtual void Hook_OnDestroyed()
	{
	}

	protected virtual void Hook_OnUpdate(TickOptions options)
	{
	}

	protected virtual void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
	}
}
public abstract class MapEntity<TInternalVariant> : MapEntity where TInternalVariant : MetaBuildingInternalVariant
{
	protected new readonly TInternalVariant InternalVariant;

	protected MapEntity(CtorArgs payload)
		: base(payload)
	{
		InternalVariant = (TInternalVariant)payload.InternalVariant;
	}
}
