using System.Collections.Generic;

public class TunnelEntranceEntity : MapEntity<MetaBuildingInternalVariant>
{
	private TunnelLane[] _Lanes = new TunnelLane[4];

	public IReadOnlyCollection<TunnelLane> Lanes => (IReadOnlyCollection<TunnelLane>)(object)_Lanes;

	public TunnelEntranceEntity(CtorArgs payload)
		: base(payload)
	{
		for (int i = 0; i < _Lanes.Length; i++)
		{
			_Lanes[i] = new TunnelLane(this, i, InternalVariant.BeltLaneDefinitions[i]);
		}
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return _Lanes[index].InputLane;
	}

	public override Drawing_CullMode Order_GetCullMode()
	{
		return Drawing_CullMode.DrawAlways_NEEDS_MANUAL_CULLING;
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		for (int i = 0; i < _Lanes.Length; i++)
		{
			_Lanes[i].Update(options);
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		for (int i = 0; i < _Lanes.Length; i++)
		{
			_Lanes[i].Draw(options);
		}
	}
}
