using System.Collections.Generic;

public class ResearchChunkLimitManager
{
	private ResearchProgress Progress;

	private Player Player;

	private GameModeHandle Mode;

	public int CurrentChunkLimit { get; protected set; } = 0;

	public ResearchChunkLimitManager(ResearchProgress progress, Player player, GameModeHandle mode)
	{
		Progress = progress;
		Player = player;
		Mode = mode;
	}

	public void Initialize()
	{
		ComputeChunkLimit();
		Progress.OnChanged.AddListener(ComputeChunkLimit);
	}

	private void ComputeChunkLimit()
	{
		GameModeHandle mode = Mode;
		int limit = mode.ResearchConfig.InitialChunkLimit;
		foreach (KeyValuePair<MetaResearchable, int> entry in mode.ResearchConfig.ChunkLimitUnlocks)
		{
			if (Progress.IsUnlocked(entry.Key))
			{
				limit += entry.Value;
			}
		}
		CurrentChunkLimit = limit;
	}

	public int ComputeCurrentChunkUsage()
	{
		int result = 0;
		List<Island> islands = Player.CurrentMap.Islands;
		for (int i = 0; i < islands.Count; i++)
		{
			Island island = islands[i];
			if (island.Metadata.Layout.PlayerBuildable)
			{
				result += island.Metadata.Layout.ChunkCount;
			}
		}
		return result;
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("research.set-chunk-limit", new DebugConsole.IntOption("amount", 0), delegate(DebugConsole.CommandContext ctx)
		{
			int currentChunkLimit = ctx.GetInt(0);
			CurrentChunkLimit = currentChunkLimit;
		}, isCheat: true);
	}

	public bool CanAfford(int chunkCost)
	{
		int currentUsage = ComputeCurrentChunkUsage();
		return currentUsage + chunkCost <= CurrentChunkLimit;
	}
}
