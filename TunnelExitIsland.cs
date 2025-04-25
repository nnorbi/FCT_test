using System;

public class TunnelExitIsland : Island
{
	protected TunnelEntranceIsland CachedEntrance;

	public TunnelExitIsland(CtorData data)
		: base(data)
	{
		LinkWithEntrance();
	}

	protected void LinkWithEntrance()
	{
		TunnelEntranceIsland entrance = TunnelEntranceIsland.Tunnels_FindEntrance(Map, Origin_GC, Metadata.LayoutRotation);
		if (CachedEntrance != entrance)
		{
			if (CachedEntrance != null)
			{
				CachedEntrance.Tunnels_UnlinkFromExit();
			}
			CachedEntrance = entrance;
			entrance.Tunnels_LinkExit(this);
		}
	}

	public void Tunnels_LinkEntrance(TunnelEntranceIsland entrance)
	{
		if (entrance != CachedEntrance)
		{
			if (CachedEntrance != null)
			{
				CachedEntrance.Tunnels_UnlinkFromExit();
			}
			CachedEntrance = entrance;
		}
	}

	public void Tunnels_UnlinkFromEntrance()
	{
		if (CachedEntrance == null)
		{
			throw new Exception("Entrance already null!");
		}
		CachedEntrance = null;
	}

	protected override void Hook_OnBeforeDestroyed()
	{
		if (CachedEntrance != null)
		{
			CachedEntrance.Tunnels_UnlinkFromExit();
		}
	}
}
