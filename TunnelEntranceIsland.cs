using System;

public class TunnelEntranceIsland : Island
{
	public TunnelExitIsland CachedExit { get; protected set; }

	public static TunnelExitIsland Tunnels_FindExit(GameMap map, GlobalChunkCoordinate entranceTile_GC, Grid.Direction entranceRotation_G)
	{
		for (int offset = 1; offset < TunnelConstants.TunnelMaxLength; offset++)
		{
			Island contents = map.GetIslandAt_GC(entranceTile_GC + offset * (ChunkDirection)entranceRotation_G);
			if (contents != null)
			{
				if (contents is TunnelEntranceIsland)
				{
					break;
				}
				if (contents is TunnelExitIsland exitIsland && exitIsland.Metadata.LayoutRotation == entranceRotation_G)
				{
					return exitIsland;
				}
			}
		}
		return null;
	}

	public static TunnelEntranceIsland Tunnels_FindEntrance(GameMap map, GlobalChunkCoordinate exitTile_GC, Grid.Direction exitRotation_G)
	{
		for (int offset = 1; offset < TunnelConstants.TunnelMaxLength; offset++)
		{
			Island contents = map.GetIslandAt_GC(exitTile_GC - offset * (ChunkDirection)exitRotation_G);
			if (contents is TunnelEntranceIsland entranceIsland && entranceIsland.Metadata.LayoutRotation == exitRotation_G)
			{
				return entranceIsland;
			}
		}
		return null;
	}

	public TunnelEntranceIsland(CtorData data)
		: base(data)
	{
		LinkWithExit();
	}

	protected void LinkWithExit()
	{
		TunnelExitIsland exit = Tunnels_FindExit(Map, Origin_GC, Metadata.LayoutRotation);
		if (CachedExit != exit)
		{
			if (CachedExit != null)
			{
				CachedExit.Tunnels_UnlinkFromEntrance();
			}
			CachedExit = exit;
			exit.Tunnels_LinkEntrance(this);
		}
	}

	public void Tunnels_LinkExit(TunnelExitIsland exit)
	{
		if (exit != CachedExit)
		{
			if (CachedExit != null)
			{
				CachedExit.Tunnels_UnlinkFromEntrance();
			}
			CachedExit = exit;
		}
	}

	protected override bool Simulation_ShouldRenderAtHighUPS(bool gameIsRendering)
	{
		return true;
	}

	public void Tunnels_UnlinkFromExit()
	{
		if (CachedExit == null)
		{
			throw new Exception("Exit already null!");
		}
		CachedExit = null;
	}

	protected override void Hook_OnBeforeDestroyed()
	{
		if (CachedExit != null)
		{
			CachedExit.Tunnels_UnlinkFromEntrance();
		}
	}
}
