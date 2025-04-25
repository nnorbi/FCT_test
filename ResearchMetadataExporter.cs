using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public static class ResearchMetadataExporter
{
	public class ResearchNodeExport
	{
		public string Id;

		public string Title;

		public string Description;

		public string GoalShape;

		public int GoalAmount;

		public string[] Unlocks;

		public static ResearchNodeExport FromNode(IResearchableHandle researchable)
		{
			List<string> unlocks = new List<string>();
			foreach (IResearchUnlock unlock in researchable.Meta.Unlocks)
			{
				if (unlock is MetaBuildingVariant variant)
				{
					unlocks.Add(variant.name);
				}
				else if (unlock is MetaIslandLayout layout)
				{
					unlocks.Add(layout.name);
				}
				else
				{
					unlocks.Add(unlock.Title);
				}
			}
			foreach (MetaResearchable.SpeedAdjustmentData speedOverride in researchable.Meta.SpeedAdjustments)
			{
				unlocks.Add(speedOverride.Speed.name + " +" + speedOverride.AdditiveSpeed + "%");
			}
			return new ResearchNodeExport
			{
				Id = researchable.Meta.name,
				Title = researchable.Meta.Title,
				Description = researchable.Meta.Description,
				GoalShape = researchable.Cost.DefinitionHash,
				GoalAmount = researchable.Cost.AmountFixed,
				Unlocks = unlocks.ToArray()
			};
		}
	}

	public class ResearchLevelExport
	{
		public ResearchNodeExport Node;

		public int LevelIndex;

		public ResearchNodeExport[] SideGoals;

		public static ResearchLevelExport FromLevel(ResearchLevelHandle level)
		{
			ResearchLevelExport researchLevelExport = new ResearchLevelExport();
			researchLevelExport.Node = ResearchNodeExport.FromNode(level);
			researchLevelExport.SideGoals = level.SideGoals.Select(ResearchNodeExport.FromNode).ToArray();
			return researchLevelExport;
		}
	}

	public class ResearchTreeExport
	{
		public string GameVersion;

		public ResearchLevelExport[] Levels;

		public static ResearchTreeExport FromTree(ResearchTreeHandle tree)
		{
			return new ResearchTreeExport
			{
				GameVersion = GameEnvironmentManager.VERSION,
				Levels = tree.Levels.Select((ResearchLevelHandle l) => ResearchLevelExport.FromLevel(l)).ToArray()
			};
		}
	}

	public static void ExportMetadata()
	{
		ResearchTreeExport data = ResearchTreeExport.FromTree(Singleton<GameCore>.G.Research.Tree);
		string json = JsonConvert.SerializeObject(data, SavegameSerializerBase.JSON_SETTINGS);
		File.WriteAllText(Path.Join(GameEnvironmentManager.DATA_PATH, "research-metadata.json"), json);
	}
}
