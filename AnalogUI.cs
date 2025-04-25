using System;
using Unity.Mathematics;
using UnityEngine;

public static class AnalogUI
{
	public static int INSTANCING_ID_HUB_SPOT_VALID = Shader.PropertyToID("placement-renderer::hub-spot-valid");

	public static void DrawHubBeltPortIndicators(Player player, FrameDrawOptions drawOptions, Island island, IslandTileCoordinate? currentTile_I = null, Grid.Direction? currentDirection_G = null)
	{
		IslandChunk chunk = island.Chunks.Find((IslandChunk islandChunk) => islandChunk is HUBCenterIslandChunk);
		if (!(chunk is HUBCenterIslandChunk { Hub: var hub }))
		{
			return;
		}
		foreach (HubEntity.InputSlot slot in hub.InputSlots)
		{
			if (slot.Tile_I.z == player.Viewport.Layer)
			{
				MapEntity contents = island.GetEntity_I(in slot.Tile_I);
				if (!(contents is BeltPortSenderEntity))
				{
					DrawValidBeltPortPlacementIndicator(player, drawOptions, island, slot.Tile_I, slot.Direction_G, currentTile_I, currentDirection_G);
				}
			}
		}
	}

	public static void DrawValidBeltPortPlacementIndicator(Player player, FrameDrawOptions drawOptions, Island island, IslandTileCoordinate tile_I, Grid.Direction direction, IslandTileCoordinate? currentTile_I, Grid.Direction? currentDirection_G)
	{
		float3 translation = tile_I.To_W(island);
		translation.y = player.Viewport.Height + 0.1f;
		drawOptions.Draw3DPlaneWithMaterialInstanced(INSTANCING_ID_HUB_SPOT_VALID, drawOptions.Theme.BaseResources.UXHubSpotPlacementValidMaterial, FastMatrix.TranslateRotate(in translation, Grid.OppositeDirection(direction)));
	}

	public static void DrawNotchBeltPortIndicators(Player player, FrameDrawOptions drawOptions, Island island, IslandTileCoordinate? currentTile_I = null, Grid.Direction? currentDirection_G = null)
	{
		foreach (IslandChunk chunk in island.Chunks)
		{
			IslandChunkNotch[] notches = chunk.Notches;
			foreach (IslandChunkNotch notch in notches)
			{
				ChunkTileCoordinate[] notchTiles_L = notch.NotchTiles_L;
				foreach (ChunkTileCoordinate notchTile_L in notchTiles_L)
				{
					IslandTileCoordinate notchTile_I = notchTile_L.To_I(in chunk.Coordinate_IC);
					notchTile_I.z = player.Viewport.Layer;
					if (island.GetEntity_I(in notchTile_I) == null)
					{
						DrawValidBeltPortPlacementIndicator(player, drawOptions, island, notchTile_I, notch.Direction, currentTile_I, currentDirection_G);
					}
				}
			}
		}
	}

	public static void Draw3DPlaneWithMaterial(this FrameDrawOptions options, Material material, in Matrix4x4 trs, MaterialPropertyBlock properties = null)
	{
		options.AnalogUIRenderer.DrawMesh(Globals.Resources.UXPlaneMeshUVMapped, in trs, material, RenderCategory.AnalogUI, properties);
	}

	public static void Draw3DPlaneWithMaterialInstanced(this FrameDrawOptions options, int key, Material material, in Matrix4x4 trs, MaterialPropertyBlock propertyBlock = null)
	{
		options.AnalogUIInstanceManager.AddInstance(key, Globals.Resources.UXPlaneMeshUVMapped, material, in trs, propertyBlock);
	}

	public static void DrawUIGeneralBuildingPlacementIndicatorMesh(FrameDrawOptions options, Mesh mesh, in Matrix4x4 trs)
	{
		options.AnalogUIInstanceManager.AddInstanceSlow(mesh, options.Theme.BaseResources.UXGeneralBuildingPlacementIndicatorMaterial, in trs);
	}

	public static void DrawBuildingPreview(FrameDrawOptions drawOptions, GlobalTileCoordinate tile_G, Grid.Direction rotation, MetaBuildingInternalVariant internalVariant, BuildingPlacementFeedback feedback, bool highlight = false)
	{
		GameResources resources = Globals.Resources;
		if (1 == 0)
		{
		}
		EditorShaderColor editorShaderColor = feedback switch
		{
			BuildingPlacementFeedback.WillBePlaced => resources.ThemeWillBePlaced, 
			BuildingPlacementFeedback.WillBePlacedButAltersFactory => resources.ThemeWillBePlacedWithWarning, 
			BuildingPlacementFeedback.WontBePlacedBecauseAltersFactory => resources.ThemeWontBePlaced, 
			BuildingPlacementFeedback.InvalidPlacement => resources.ThemeImpossibleToPlace, 
			_ => throw new ArgumentOutOfRangeException("feedback"), 
		};
		if (1 == 0)
		{
		}
		EditorShaderColor color = editorShaderColor;
		if (1 == 0)
		{
		}
		float num = feedback switch
		{
			BuildingPlacementFeedback.WillBePlaced => 0.001f, 
			BuildingPlacementFeedback.WillBePlacedButAltersFactory => 0.005f, 
			BuildingPlacementFeedback.WontBePlacedBecauseAltersFactory => 0.01f, 
			BuildingPlacementFeedback.InvalidPlacement => 0.02f, 
			_ => throw new ArgumentOutOfRangeException("feedback"), 
		};
		if (1 == 0)
		{
		}
		float offset = num;
		offset += (highlight ? 0.05f : 0.001f);
		DrawBuildingPreview(drawOptions, internalVariant, tile_G, rotation, color, offset);
	}

	private static void DrawBuildingPreview(FrameDrawOptions drawOptions, MetaBuildingInternalVariant internalVariant, GlobalTileCoordinate tile_G, Grid.Direction rotation, EditorShaderColor color, float offset)
	{
		float scale = ComputePulseAnimation(internalVariant);
		Matrix4x4 transform = Matrix4x4.TRS(tile_G.ToCenter_W() + offset * WorldDirection.Up, FastMatrix.RotateY(rotation), new Vector3(scale, scale, scale));
		internalVariant.CombinedBlueprintMesh.Draw(drawOptions, drawOptions.Theme.BaseResources.BlueprintPreWriteDepthMaterial, in transform, RenderCategory.SelectionAndBp);
		internalVariant.CombinedBlueprintMesh.Draw(drawOptions, drawOptions.Theme.BaseResources.BlueprintMaterial, in transform, RenderCategory.SelectionAndBp, color.PropertyBlock, castShadows: true, receiveShadows: true);
	}

	public static void DrawPlacementIndicators(FrameDrawOptions drawOptions, GameMap map, GlobalTileCoordinate tile_G, Grid.Direction rotation, MetaBuildingInternalVariant internalVariant, BuildingPlacementFeedback feedback)
	{
		Island island = map.GetIslandAt_G(in tile_G);
		if (island != null && (feedback == BuildingPlacementFeedback.WillBePlaced || feedback == BuildingPlacementFeedback.WillBePlacedButAltersFactory))
		{
			EditorClassIDSingleton<IBuildingPlacementIndicator>[] placementIndicators = internalVariant.Variant.PlacementIndicators;
			foreach (EditorClassIDSingleton<IBuildingPlacementIndicator> entry in placementIndicators)
			{
				entry.Instance.Draw(drawOptions, island, tile_G.To_I(island), tile_G, rotation, internalVariant);
			}
		}
	}

	public static float ComputePulseAnimation(MetaBuildingInternalVariant internalVariant)
	{
		return 1.0001f + 0.02f * HUDTheme.PulseAnimation() / (float)internalVariant.Tiles.Length;
	}

	public static float ScaleForLocalIOLayer(int layer)
	{
		return math.clamp(1f - (float)math.abs(layer) * 0.4f, 0.4f, 1f);
	}

	public static void DrawBuildingInAndOutputs(FrameDrawOptions options, Island island, IslandTileCoordinate baseTile_I, Grid.Direction rotation, MetaBuildingInternalVariant internalVariant, bool drawOverlapOnly = false)
	{
		VisualThemeBaseResources themeResources = options.Theme.BaseResources;
		if (!drawOverlapOnly)
		{
			MetaBuildingInternalVariant.FluidContainerConfig[] fluidContainers = internalVariant.FluidContainers;
			foreach (MetaBuildingInternalVariant.FluidContainerConfig container in fluidContainers)
			{
				MetaBuildingInternalVariant.FluidIO[] connections = container.Connections;
				foreach (MetaBuildingInternalVariant.FluidIO connection in connections)
				{
					IslandTileCoordinate connectionTile_I = connection.Position_L.To_I(rotation, in baseTile_I);
					float3 connectionTile_W = connectionTile_I.ToCenter_W(in island.Origin_GC);
					IslandTileCoordinate destTile_I = (connection.Position_L + connection.Direction_L).To_I(rotation, in baseTile_I);
					if (!island.IsValidAndFilledTile_I(in destTile_I))
					{
						continue;
					}
					bool anyConnected = false;
					MapEntity destEntity = island.GetEntity_I(in destTile_I);
					if (destEntity != null)
					{
						MetaBuildingInternalVariant.FluidContainerConfig[] fluidContainers2 = destEntity.InternalVariant.FluidContainers;
						foreach (MetaBuildingInternalVariant.FluidContainerConfig otherContainer in fluidContainers2)
						{
							MetaBuildingInternalVariant.FluidIO[] connections2 = otherContainer.Connections;
							foreach (MetaBuildingInternalVariant.FluidIO otherConnection in connections2)
							{
								IslandTileCoordinate otherTile_I = otherConnection.Position_L.To_I(destEntity);
								if ((otherConnection.Position_L + otherConnection.Direction_L).To_I(destEntity).Equals(connectionTile_I) && destTile_I.Equals(otherTile_I))
								{
									anyConnected = true;
									break;
								}
							}
						}
					}
					Grid.Direction outputRotation = Grid.RotateDirection(connection.Direction_L, rotation);
					Matrix4x4 trs = Matrix4x4.TRS(connectionTile_W + 0.8f * (WorldDirection)outputRotation + 0.45f * WorldDirection.Up, FastMatrix.RotateY(Grid.OppositeDirection(outputRotation)), Vector3.one * 0.5f * ScaleForLocalIOLayer(connection.Position_L.z));
					options.Draw3DPlaneWithMaterial(anyConnected ? themeResources.UXBuildingFluidIOConnectedMaterial : themeResources.UXBuildingFluidIONotConnectedMaterial, in trs);
				}
			}
		}
		MetaBuildingInternalVariant.BeltIO[] beltInputs = internalVariant.BeltInputs;
		foreach (MetaBuildingInternalVariant.BeltIO input in beltInputs)
		{
			BeltItem inputItem = null;
			IslandTileCoordinate inputTile_I = input.Position_L.To_I(rotation, in baseTile_I);
			float3 inputTile_W = inputTile_I.ToCenter_W(in island.Origin_GC);
			IslandTileCoordinate sourceTile_I = (input.Position_L + input.Direction_L).To_I(rotation, in baseTile_I);
			bool anyConnected2 = false;
			bool overlap = false;
			if (!island.IsValidAndFilledTile_I(in sourceTile_I))
			{
				overlap = true;
			}
			else
			{
				MapEntity sourceEntity = island.GetEntity_I(in sourceTile_I);
				if (sourceEntity != null)
				{
					MetaBuildingInternalVariant.BeltIO[] sourceOutputs = sourceEntity.InternalVariant.BeltOutputs;
					foreach (MetaBuildingInternalVariant.BeltIO output in sourceOutputs)
					{
						IslandTileCoordinate outputTile_I = output.Position_L.To_I(sourceEntity);
						if ((output.Position_L + output.Direction_L).To_I(sourceEntity).Equals(inputTile_I) && sourceTile_I.Equals(outputTile_I))
						{
							anyConnected2 = true;
							break;
						}
					}
					MetaBuildingInternalVariant.BeltIO[] beltInputs2 = sourceEntity.InternalVariant.BeltInputs;
					foreach (MetaBuildingInternalVariant.BeltIO otherInput in beltInputs2)
					{
						IslandTileCoordinate otherInputTile_I = otherInput.Position_L.To_I(sourceEntity);
						if ((otherInput.Position_L + otherInput.Direction_L).To_I(sourceEntity).Equals(inputTile_I) && sourceTile_I.Equals(otherInputTile_I))
						{
							overlap = true;
							break;
						}
					}
				}
			}
			if (!drawOverlapOnly || overlap)
			{
				Grid.Direction inputRotation = Grid.RotateDirection(input.Direction_L, rotation);
				Matrix4x4 trs = Matrix4x4.TRS(inputTile_W + 0.7f * (WorldDirection)inputRotation + 0.35f * WorldDirection.Up, FastMatrix.RotateY(inputRotation), Vector3.one * 0.5f * ScaleForLocalIOLayer(input.Position_L.z));
				options.Draw3DPlaneWithMaterial(overlap ? themeResources.UXBuildingBeltIOConflictMaterial : (anyConnected2 ? themeResources.UXBuildingBeltInputConnectedMaterial : themeResources.UXBuildingBeltInputNotConnectedMaterial), in trs);
			}
		}
		for (int outputIndex = 0; outputIndex < internalVariant.BeltOutputs.Length; outputIndex++)
		{
			MetaBuildingInternalVariant.BeltIO output2 = internalVariant.BeltOutputs[outputIndex];
			IslandTileCoordinate outputTile_I2 = output2.Position_L.To_I(rotation, in baseTile_I);
			float3 outputTile_W = outputTile_I2.ToCenter_W(in island.Origin_GC);
			IslandTileCoordinate destTile_I2 = (output2.Position_L + output2.Direction_L).To_I(rotation, in baseTile_I);
			bool anyConnected3 = false;
			bool overlap2 = false;
			if (!island.IsValidAndFilledTile_I(in destTile_I2))
			{
				overlap2 = true;
			}
			else
			{
				MapEntity destEntity2 = island.GetEntity_I(in destTile_I2);
				if (destEntity2 != null)
				{
					anyConnected3 = false;
					MetaBuildingInternalVariant.BeltIO[] beltInputs3 = destEntity2.InternalVariant.BeltInputs;
					foreach (MetaBuildingInternalVariant.BeltIO input2 in beltInputs3)
					{
						IslandTileCoordinate inputTile_I2 = input2.Position_L.To_I(destEntity2);
						if ((input2.Position_L + input2.Direction_L).To_I(destEntity2).Equals(outputTile_I2) && destTile_I2.Equals(inputTile_I2))
						{
							anyConnected3 = true;
							break;
						}
					}
					MetaBuildingInternalVariant.BeltIO[] beltOutputs = destEntity2.InternalVariant.BeltOutputs;
					foreach (MetaBuildingInternalVariant.BeltIO otherOutput in beltOutputs)
					{
						IslandTileCoordinate otherOutputTile_I = otherOutput.Position_L.To_I(destEntity2);
						if ((otherOutput.Position_L + otherOutput.Direction_L).To_I(destEntity2).Equals(outputTile_I2) && destTile_I2.Equals(otherOutputTile_I))
						{
							overlap2 = true;
							break;
						}
					}
				}
			}
			if (!drawOverlapOnly || overlap2)
			{
				Grid.Direction outputRotation2 = Grid.RotateDirection(output2.Direction_L, rotation);
				Matrix4x4 trs = Matrix4x4.TRS(outputTile_W + 0.7f * (WorldDirection)outputRotation2 + 0.35f * WorldDirection.Up, FastMatrix.RotateY(Grid.OppositeDirection(outputRotation2)), Vector3.one * 0.5f * ScaleForLocalIOLayer(output2.Position_L.z));
				options.Draw3DPlaneWithMaterial(overlap2 ? themeResources.UXBuildingBeltIOConflictMaterial : (anyConnected3 ? themeResources.UXBuildingBeltOutputConnectedMaterial : themeResources.UXBuildingBeltOutputNotConnectedMaterial), in trs);
			}
		}
	}
}
