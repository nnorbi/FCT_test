using System.Collections.Generic;
using System.Linq;

public class HyperBeltMaterializer : IIslandMaterializer<HyperBeltNode>
{
	private Dictionary<HyperBeltPart, MetaIslandLayout> LayoutMap = new Dictionary<HyperBeltPart, MetaIslandLayout>();

	public HyperBeltMaterializer()
	{
		List<MetaIslandLayout> layouts = Singleton<GameCore>.G.Mode.IslandLayouts;
		MetaIslandLayout invalid = layouts.Single((MetaIslandLayout x) => x.name == "Layout_1");
		MetaIslandLayout tunnelEntrance = layouts.Single((MetaIslandLayout x) => x.name == "LayoutTunnelEntrance");
		MetaIslandLayout tunnelExit = layouts.Single((MetaIslandLayout x) => x.name == "LayoutTunnelExit");
		MetaIslandLayout leftTurnIsland = layouts.Single((MetaIslandLayout x) => x.name == "Layout_1_LeftTurn");
		MetaIslandLayout rightTurnIsland = layouts.Single((MetaIslandLayout x) => x.name == "Layout_1_RightTurn");
		MetaIslandLayout forwarderIsland = layouts.Single((MetaIslandLayout x) => x.name == "Layout_1_Forward");
		LayoutMap.Add(HyperBeltPart.TunnelSender, tunnelEntrance);
		LayoutMap.Add(HyperBeltPart.TunnelReceiver, tunnelExit);
		LayoutMap.Add(HyperBeltPart.LeftTurn, leftTurnIsland);
		LayoutMap.Add(HyperBeltPart.RightTurn, rightTurnIsland);
		LayoutMap.Add(HyperBeltPart.Forward, forwarderIsland);
		LayoutMap.Add(HyperBeltPart.Invalid, invalid);
	}

	public IslandDescriptor Materialize(HyperBeltNode node)
	{
		return IslandDescriptor.From(LayoutMap[node.Part], node.Direction, node.Position);
	}

	public MetaIslandLayout GetLayoutForPart(HyperBeltPart hyperBeltPart)
	{
		return LayoutMap[hyperBeltPart];
	}
}
