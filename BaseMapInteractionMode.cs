using System.Collections.Generic;

public class BaseMapInteractionMode
{
	protected bool IsInternalPlayer(Player player)
	{
		return player.Role == Player.PlayerRole.GameInternal;
	}

	public virtual bool AllowBuildingPlacement(Player player, Island island = null)
	{
		if (IsInternalPlayer(player))
		{
			return true;
		}
		if (island != null && !island.Metadata.Layout.CanModifyIslandContents)
		{
			return false;
		}
		return true;
	}

	public virtual bool AllowBuildingVariant(Player player, MetaBuildingVariant variant, Island island = null)
	{
		if (IsInternalPlayer(player))
		{
			return true;
		}
		if (!variant.PlayerBuildable)
		{
			return false;
		}
		if (!Singleton<GameCore>.G.Research.Progress.IsUnlocked(variant))
		{
			return false;
		}
		return AllowBuildingPlacement(player, island);
	}

	public virtual bool AllowBuildingDelete(Player player, MapEntity entity)
	{
		if (IsInternalPlayer(player))
		{
			return true;
		}
		return entity.Variant.Removable && entity.Island.Metadata.Layout.CanModifyIslandContents;
	}

	public virtual bool AllowBlueprints(Player player)
	{
		if (IsInternalPlayer(player))
		{
			return true;
		}
		return Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.BlueprintsUnlock);
	}

	public virtual bool AllowRailManagement(Player player)
	{
		if (IsInternalPlayer(player))
		{
			return true;
		}
		if (player.Viewport.Scope == GameScope.Overview)
		{
			return false;
		}
		return Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.RailsUnlock);
	}

	public virtual bool AllowIslandManagement(Player player)
	{
		if (IsInternalPlayer(player))
		{
			return true;
		}
		if (player.Viewport.Scope != GameScope.Islands)
		{
			return false;
		}
		return Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.IslandManagementUnlock);
	}

	public virtual bool AllowIslandDeletion(Player player, MetaIslandLayout layout)
	{
		if (IsInternalPlayer(player))
		{
			return true;
		}
		if (!AllowIslandManagement(player))
		{
			return false;
		}
		return layout.PlayerBuildable;
	}

	public virtual bool AllowIslandPlacement(Player player, MetaIslandLayout layout)
	{
		if (IsInternalPlayer(player))
		{
			return true;
		}
		if (!AllowIslandManagement(player))
		{
			return false;
		}
		if (!Singleton<GameCore>.G.Research.Progress.IsUnlocked(layout))
		{
			return false;
		}
		return layout.PlayerBuildable;
	}

	public virtual short GetMaximumAllowedLayer(Player player)
	{
		if (IsInternalPlayer(player))
		{
			return Singleton<GameCore>.G.Mode.MaxLayer;
		}
		List<MetaGenericResearchUnlock> unlocks = Singleton<GameCore>.G.Mode.ResearchConfig.LayerUnlocks;
		for (int i = unlocks.Count - 1; i >= 0; i--)
		{
			MetaGenericResearchUnlock unlock = unlocks[i];
			if (Singleton<GameCore>.G.Research.Progress.IsUnlocked(unlock))
			{
				return (short)(i + 1);
			}
		}
		return 0;
	}
}
