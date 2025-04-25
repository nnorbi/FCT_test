using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResearchManager
{
	public class SerializedData
	{
		public ResearchProgress.SerializedData ResearchProgress = new ResearchProgress.SerializedData();

		public BlueprintCurrencyManager.SerializedData Blueprints = new BlueprintCurrencyManager.SerializedData();
	}

	public const string DATA_FILENAME = "research.json";

	private Player Player;

	public ResearchSpeedManager SpeedManager;

	public ResearchShapeManager ShapeManager;

	public ResearchChunkLimitManager ChunkLimitManager;

	public ResearchShapeStorage ShapeStorage;

	public ResearchLevelManager LevelManager;

	public BlueprintCurrencyManager BlueprintCurrencyManager;

	public ResearchDiscountManager DiscountManager;

	public ResearchProgress Progress;

	private GameModeHandle Mode;

	public ResearchTreeHandle Tree { get; private set; }

	public ResearchManager(Player player)
	{
		Player = player;
	}

	public void Serialize(SavegameBlobWriter writer)
	{
		writer.WriteObjectAsJson("research.json", new SerializedData
		{
			ResearchProgress = Progress.Serialize(),
			Blueprints = BlueprintCurrencyManager.Serialize()
		});
	}

	public void InitExistingGameFromSerialized(SavegameBlobReader reader)
	{
		InitializeInstances();
		SerializedData data = reader.ReadObjectFromJson<SerializedData>("research.json");
		Progress.Deserialize(data.ResearchProgress);
		LevelManager.EnsureInitialLevelUnlocked();
		ShapeManager.Initialize();
		BlueprintCurrencyManager.InitializeExisting(ShapeStorage, data.Blueprints);
		DiscountManager.Initialize();
		SpeedManager.Initialize();
		ChunkLimitManager.Initialize();
		LevelManager.Initialize();
		LevelManager.EnsureInitialLevelUnlocked();
	}

	public void InitNewGame()
	{
		InitializeInstances();
		LevelManager.EnsureInitialLevelUnlocked();
		ShapeManager.Initialize();
		BlueprintCurrencyManager.Initialize(ShapeStorage);
		DiscountManager.Initialize();
		SpeedManager.Initialize();
		ChunkLimitManager.Initialize();
		LevelManager.Initialize();
	}

	private void InitializeInstances()
	{
		if (Tree != null)
		{
			throw new Exception("Can not initialize research twice");
		}
		Mode = Singleton<GameCore>.G.Mode;
		Tree = Mode.ResearchTree;
		Progress = new ResearchProgress(Tree);
		ShapeManager = new ResearchShapeManager(Progress, Tree);
		ShapeStorage = new ResearchShapeStorage(Player, ShapeManager);
		LevelManager = new ResearchLevelManager(Progress, Tree);
		BlueprintCurrencyManager = new BlueprintCurrencyManager(Mode);
		DiscountManager = new ResearchDiscountManager(Progress);
		ChunkLimitManager = new ResearchChunkLimitManager(Progress, Player, Mode);
		SpeedManager = new ResearchSpeedManager(Progress, Tree, Mode);
		SanityChecks();
	}

	public bool CanReach(IResearchableHandle researchable)
	{
		if (researchable.LevelDependency != null && !Progress.IsUnlocked(researchable.LevelDependency))
		{
			return false;
		}
		return true;
	}

	public bool CanUnlock(IResearchableHandle researchable)
	{
		if (Progress.IsUnlocked(researchable) || !CanReach(researchable))
		{
			return false;
		}
		return CanAffordCost(researchable.Cost);
	}

	public bool CanAffordCost(ResearchUnlockCost cost)
	{
		if (cost.Type == ResearchUnlockCost.CostType.Fixed)
		{
			return ShapeStorage.CanAfford(cost.DefinitionHash, cost.AmountFixed);
		}
		if (cost.Type == ResearchUnlockCost.CostType.Free)
		{
			return true;
		}
		throw new NotImplementedException(cost.Type.ToString());
	}

	public bool TryTakeCost(ResearchUnlockCost cost)
	{
		if (cost.Type == ResearchUnlockCost.CostType.Fixed)
		{
			return ShapeStorage.TryTake(cost.DefinitionHash, cost.AmountFixed);
		}
		if (cost.Type == ResearchUnlockCost.CostType.Free)
		{
			return true;
		}
		throw new NotImplementedException(cost.Type.ToString());
	}

	public bool TryUnlock(IResearchableHandle researchable, bool forced = false)
	{
		if (Progress.IsUnlocked(researchable))
		{
			return false;
		}
		if (!forced)
		{
			if (!CanUnlock(researchable))
			{
				return false;
			}
			if (!TryTakeCost(researchable.Cost))
			{
				throw new Exception("Failed to take cost");
			}
		}
		Progress.Unlock(researchable);
		if (!forced)
		{
			Singleton<GameCore>.G.HUD.Events.ResearchCompletedByPlayer.Invoke(researchable);
		}
		return true;
	}

	protected bool TryLock(IResearchableHandle researchable)
	{
		if (!Progress.IsUnlocked(researchable))
		{
			return false;
		}
		Progress.Lock(researchable);
		return true;
	}

	public IResearchableHandle FindResearchableById(string name)
	{
		return Tree.AllResearchables.FirstOrDefault((IResearchableHandle researchable) => researchable.Meta.name == name);
	}

	public int ComputeUnlockableSideGoalsCount()
	{
		return Tree.SideGoals.Count(CanUnlock);
	}

	protected void SanityChecks()
	{
		foreach (MetaBuilding building in Mode.Buildings)
		{
			foreach (MetaBuildingVariant variant in building.Variants)
			{
				if (variant.PlayerBuildable && !Tree.AllUnlocks.Contains(variant))
				{
					Debug.LogWarning("Building variant '" + variant.name + "' is not referenced in the research tree, thus can never be unlocked.");
				}
			}
		}
		foreach (MetaIslandLayout layout in Mode.IslandLayouts)
		{
			if (layout.PlayerBuildable && !Tree.AllUnlocks.Contains(layout))
			{
				Debug.LogWarning("Island layout '" + layout.name + "' is not referenced in the research tree, thus can never be unlocked.");
			}
		}
		foreach (KeyValuePair<MetaResearchable, int> entry in Mode.ResearchConfig.HUBSizeUnlocks)
		{
			if (!Array.Exists(Tree.AllResearchables, (IResearchableHandle researchable) => researchable.Meta == entry.Key))
			{
				Debug.LogWarning("Node '" + entry.Key.name + "' for HUB Size Unlock is not referenced in the research tree, thus can never be unlocked.");
			}
		}
		foreach (KeyValuePair<MetaResearchable, int> entry2 in Mode.ResearchConfig.ChunkLimitUnlocks)
		{
			if (!Array.Exists(Tree.AllResearchables, (IResearchableHandle researchable) => researchable.Meta == entry2.Key))
			{
				Debug.LogWarning("Node '" + entry2.Key.name + "' for Chunk Limit increase is not referenced in the research tree, thus can never be unlocked.");
			}
		}
	}

	public void RegisterCommands(DebugConsole console)
	{
		ShapeStorage.RegisterCommands(console);
		BlueprintCurrencyManager.RegisterCommands(console);
		ChunkLimitManager.RegisterCommands(console);
		console.Register("research.list", delegate(DebugConsole.CommandContext ctx)
		{
			IResearchableHandle[] allResearchables = Tree.AllResearchables;
			ctx.Output("Following " + allResearchables.Length + " research nodes are available:");
			ResearchLevelHandle[] levels = Tree.Levels;
			foreach (ResearchLevelHandle researchLevelHandle in levels)
			{
				ctx.Output($"Level {researchLevelHandle.LevelIndex} [{researchLevelHandle.Meta.name}] {researchLevelHandle.Meta.Title}");
				foreach (ResearchSideGoalHandle current in researchLevelHandle.SideGoals)
				{
					ctx.Output("  - [" + current.Meta.name + "] " + current.Meta.Title);
				}
			}
		});
		console.Register("research.set", new DebugConsole.StringOption("name"), new DebugConsole.BoolOption("unlocked"), delegate(DebugConsole.CommandContext ctx)
		{
			IResearchableHandle researchableHandle = FindResearchableById(ctx.GetString(0));
			if (researchableHandle == null)
			{
				ctx.Output("node not found. Use research.list to see all");
			}
			else if (ctx.GetBool(1))
			{
				if (TryUnlock(researchableHandle, forced: true))
				{
					ctx.Output(researchableHandle.Meta.name + " is now unlocked.");
				}
				else
				{
					ctx.Output("Failed to unlock " + researchableHandle.Meta.name);
				}
			}
			else if (TryLock(researchableHandle))
			{
				ctx.Output(researchableHandle.Meta.name + " is now locked.");
			}
			else
			{
				ctx.Output("Failed to lock " + researchableHandle.Meta.name);
			}
		}, isCheat: true);
		console.Register("research.lock-all", delegate(DebugConsole.CommandContext ctx)
		{
			ResearchLevelHandle initialLevel = Tree.Levels[0];
			Progress.Unlock(initialLevel);
			Progress.LockBulk(Tree.AllResearchables.Where((IResearchableHandle researchable) => researchable != initialLevel));
			ctx.Output("All research nodes have been locked (except initial nodes)");
		}, isCheat: true);
		console.Register("research.unlock-all", delegate(DebugConsole.CommandContext ctx)
		{
			Progress.UnlockBulk(Tree.AllResearchables);
			foreach (MetaBuilding current in Mode.Buildings)
			{
				foreach (MetaBuildingVariant current2 in current.Variants)
				{
					Player.TutorialState.TryMarkInteractedWithBuilding(current2);
				}
			}
			ctx.Output("All research nodes have been unlocked");
		}, isCheat: true);
		console.Register("research.unlock-all-levels", delegate(DebugConsole.CommandContext ctx)
		{
			Progress.UnlockBulk(Tree.AllResearchables.Where((IResearchableHandle researchable) => researchable is ResearchLevelHandle));
			ctx.Output("All research levels have been unlocked");
		}, isCheat: true);
	}
}
