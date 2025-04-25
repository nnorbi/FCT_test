using System.Collections.Generic;
using Drawing;
using Unity.Mathematics;
using UnityEngine;

internal class DebugViewFluidNetwork : IDebugView
{
	public const string ID = "fluids";

	public string Name => "Fluids";

	public void OnGameDraw()
	{
		CommandBuilder draw = DrawingManager.GetBuilder(renderInGame: true);
		draw.cameraTargets = new Camera[1] { Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera };
		HashSet<FluidNetwork> DrawnNetworks = new HashSet<FluidNetwork>();
		foreach (Island island in Singleton<GameCore>.G.LocalPlayer.CurrentMap.Islands)
		{
			foreach (MapEntity building in island.Buildings.Buildings)
			{
				MetaBuildingInternalVariant.FluidContainerConfig[] fluidContainers = building.InternalVariant.FluidContainers;
				foreach (MetaBuildingInternalVariant.FluidContainerConfig containerConfig in fluidContainers)
				{
					FluidContainer container = building.Fluids_GetContainerByIndex(containerConfig.Index);
					FluidNetwork network = container.Network;
					float3 containerTile_I = building.I_From_L(in containerConfig.Position_L);
					draw.WireCylinder(island.W_From_I(in containerTile_I) + new float3(0f, 0.7f, 0f), Vector3.up, 0.01f + container.Level * 0.3f, 0.25f, new Color(0f, 0f, 1f));
					draw.Label3D(island.W_From_I(in containerTile_I) + new float3(0f, 0.9f, 0f), Quaternion.identity, "N" + network.UID + "C" + container.UID + "V" + math.round(container.Value) + "/" + math.round(container.Max), 0.1f, LabelAlignment.Center, new Color(1f, 1f, 1f));
					draw.Label3D(island.W_From_I(in containerTile_I) + new float3(0f, 1.1f, 0f), Quaternion.identity, container.Name, 0.1f, LabelAlignment.Center, new Color(1f, 1f, 1f));
					List<MapEntity.Fluids_LinkedContainer> links = building.Fluids_GetConnectedContainers(container.Config);
					int k = 0;
					foreach (MapEntity.Fluids_LinkedContainer link in links)
					{
						FluidContainer otherContainer = link.Container;
						if (container.HasRightToUpdate(otherContainer))
						{
							float flow = container.FlowRates[k++];
							IslandTileCoordinate otherTile_I = link.ToConnection.Position_L.To_I(otherContainer.Entity);
							float3 container_W = island.W_From_I(in containerTile_I) + new float3(0f, 0.7f, 0f);
							float3 other_W = otherTile_I.To_W(island) + new float3(0f, 0.7f, 0f);
							if (math.abs(flow) < 0.0001f)
							{
								draw.Line(container_W, other_W, new Color(0.8f, 0.8f, 0.8f));
							}
							else if (flow < 0f)
							{
								draw.Arrow(container_W, other_W, new Color(0f, 1f, 1f));
							}
							else
							{
								draw.Arrow(other_W, container_W, new Color(0f, 1f, 1f));
							}
							draw.Label3D((other_W + container_W) / 2f, Quaternion.identity, flow.ToString("F2"), 0.1f, LabelAlignment.Center, new Color(0f, 1f, 1f));
						}
					}
					if (!DrawnNetworks.Contains(network))
					{
						DrawnNetworks.Add(network);
					}
				}
			}
		}
		draw.Dispose();
	}
}
