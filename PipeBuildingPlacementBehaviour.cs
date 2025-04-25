using System;
using System.Collections.Generic;
using UnityEngine;

public class PipeBuildingPlacementBehaviour : PathBuildingPlacementBehaviour
{
	protected static Dictionary<Tuple<Grid.Direction, int>, string> CrossLayerBuildings = new Dictionary<Tuple<Grid.Direction, int>, string>
	{
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Right, 1),
			"PipeUpForwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Left, 1),
			"PipeUpBackwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Top, 1),
			"PipeUpLeftInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Bottom, 1),
			"PipeUpRightInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Right, -1),
			"PipeDownForwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Left, -1),
			"PipeDownBackwardInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Top, -1),
			"PipeDownLeftInternalVariant"
		},
		{
			new Tuple<Grid.Direction, int>(Grid.Direction.Bottom, -1),
			"PipeDownRightInternalVariant"
		}
	};

	public PipeBuildingPlacementBehaviour(CtorData data)
		: base(data)
	{
	}

	protected override bool Impl_IsOnmiDirectional()
	{
		return true;
	}

	protected override Dictionary<Tuple<Grid.Direction, int>, string> Impl_GetCrossLayerBuildings()
	{
		return CrossLayerBuildings;
	}

	protected override PathBuildingAutoReplacement[] Impl_GetAutoReplacements()
	{
		return PathBuildingAutoReplacements.Pipes;
	}

	protected override Material Draw_GetInputUXMaterial(bool connected)
	{
		return connected ? Singleton<GameCore>.G.Theme.BaseResources.UXBuildingFluidIOConnectedMaterial : Singleton<GameCore>.G.Theme.BaseResources.UXBuildingFluidIONotConnectedMaterial;
	}

	protected override Material Draw_GetOutputUXMaterial(bool connected)
	{
		return connected ? Singleton<GameCore>.G.Theme.BaseResources.UXBuildingFluidIOConnectedMaterial : Singleton<GameCore>.G.Theme.BaseResources.UXBuildingFluidIONotConnectedMaterial;
	}

	protected override void Draw_Input(FrameDrawOptions options, bool connected)
	{
		Matrix4x4 trs = Matrix4x4.TRS(InputTile_G.ToCenter_W() + 0.9f * (WorldDirection)InputDirection_G + 0.45f * WorldDirection.Up, FastMatrix.RotateY(InputDirection_G), Vector3.one * 0.5f);
		options.Draw3DPlaneWithMaterial(Draw_GetInputUXMaterial(connected), in trs);
	}

	protected override void Draw_Output(FrameDrawOptions options, bool connected)
	{
		Matrix4x4 trs = Matrix4x4.TRS(OutputTile_G.ToCenter_W() + 0.9f * (WorldDirection)OutputDirection_G + 0.45f * WorldDirection.Up, FastMatrix.RotateY(Grid.OppositeDirection(OutputDirection_G)), Vector3.one * 0.5f);
		options.Draw3DPlaneWithMaterial(Draw_GetOutputUXMaterial(connected), in trs);
	}

	protected override string Impl_GetInternalVariantNameForDirection(Grid.Direction direction)
	{
		if (1 == 0)
		{
		}
		string result = direction switch
		{
			Grid.Direction.Right => "PipeForwardInternalVariant", 
			Grid.Direction.Bottom => "PipeRightInternalVariant", 
			Grid.Direction.Left => "PipeForwardInternalVariant", 
			Grid.Direction.Top => "PipeLeftInternalVariant", 
			_ => "PipeForwardInternalVariant", 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	protected override IEnumerable<Grid.Direction> Impl_FindConnectionsAtTile(GlobalTileCoordinate tile_G, bool findInputs, Grid.Direction? preference = null, bool ignoreAutoConnectPreference = false)
	{
		Island island = Map.GetIslandAt_G(in tile_G);
		if (island == null)
		{
			yield break;
		}
		IslandTileCoordinate tile_I = tile_G.To_I(island);
		if (!island.IsValidAndFilledTile_I(in tile_I))
		{
			yield break;
		}
		Grid.Direction startDirection = preference.GetValueOrDefault();
		int directionIndex = 0;
		while (directionIndex < 4)
		{
			Grid.Direction direction = Grid.RotateDirection((Grid.Direction)directionIndex, startDirection);
			MapEntity neighbor = island.GetEntity_I(tile_I.NeighbourTile(direction));
			if (neighbor != null)
			{
				MetaBuildingInternalVariant.FluidContainerConfig[] containers = neighbor.InternalVariant.FluidContainers;
				MetaBuildingInternalVariant.FluidContainerConfig[] array = containers;
				foreach (MetaBuildingInternalVariant.FluidContainerConfig container in array)
				{
					MetaBuildingInternalVariant.FluidIO[] connections = container.Connections;
					MetaBuildingInternalVariant.FluidIO[] array2 = connections;
					foreach (MetaBuildingInternalVariant.FluidIO connection in array2)
					{
						if (neighbor.GetIOTargetTile_I(connection).Equals(tile_I))
						{
							yield return direction;
						}
					}
				}
			}
			int num = directionIndex + 1;
			directionIndex = num;
		}
	}
}
