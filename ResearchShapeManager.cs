using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResearchShapeManager
{
	private struct MappedShape
	{
		public string TargetHash;

		public string Label;

		public List<IResearchableHandle> Researchables;
	}

	private ResearchTreeHandle Tree;

	private Dictionary<string, MappedShape> DefinitionMappings = new Dictionary<string, MappedShape>();

	private ResearchProgress Progress;

	public ResearchShapeManager(ResearchProgress progress, ResearchTreeHandle tree)
	{
		Tree = tree;
		Progress = progress;
	}

	public void Initialize()
	{
		IResearchableHandle[] allResearchables = Tree.AllResearchables;
		foreach (IResearchableHandle researchable in allResearchables)
		{
			ResearchUnlockCost cost = researchable.Cost;
			if (cost != null && cost.Type != ResearchUnlockCost.CostType.Free)
			{
				if (string.IsNullOrEmpty(cost.DefinitionHash))
				{
					Debug.LogError("Ignoring empty shape goal for " + researchable.Meta.name);
				}
				else
				{
					AddMappings(cost.DefinitionHash, researchable.Meta.name, researchable);
				}
			}
		}
		List<GameModeBlueprintCurrencyShape> currencyShapes = Singleton<GameCore>.G.Mode.ResearchConfig.BlueprintCurrencyShapes;
		for (int bpShapeIndex = 0; bpShapeIndex < currencyShapes.Count; bpShapeIndex++)
		{
			AddMappings(currencyShapes[bpShapeIndex].Shape, "BlueprintShape[" + bpShapeIndex + "]");
		}
	}

	public bool IsCurrencyGrantingShape(string hash)
	{
		hash = TrimShape(hash);
		if (!DefinitionMappings.TryGetValue(hash, out var mapped))
		{
			return false;
		}
		if (Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.BlueprintsUnlock))
		{
			List<GameModeBlueprintCurrencyShape> blueprintShapes = Singleton<GameCore>.G.Mode.ResearchConfig.BlueprintCurrencyShapes;
			for (int i = 0; i < blueprintShapes.Count; i++)
			{
				if (blueprintShapes[i].Shape == mapped.TargetHash)
				{
					return true;
				}
			}
		}
		if (mapped.Researchables == null)
		{
			return false;
		}
		return mapped.Researchables.Any((IResearchableHandle researchable) => Progress.IsUnlocked(researchable) && researchable.Cost.HasCurrencyValue);
	}

	public bool IsResearchShape(string hash)
	{
		hash = TrimShape(hash);
		if (!DefinitionMappings.TryGetValue(hash, out var mapped))
		{
			return false;
		}
		if (mapped.Researchables != null)
		{
			return mapped.Researchables.Any((IResearchableHandle researchable) => !Progress.IsUnlocked(researchable) && Singleton<GameCore>.G.Research.CanReach(researchable));
		}
		return true;
	}

	public string TrimAndUnifyShape(string hash)
	{
		hash = TrimShape(hash);
		if (DefinitionMappings.TryGetValue(hash, out var result))
		{
			return result.TargetHash;
		}
		return hash;
	}

	private string TrimShape(string hash)
	{
		ShapeManager shapes = Singleton<GameCore>.G.Shapes;
		ShapeDefinition baseDefinition = shapes.GetDefinitionByHash(hash);
		string resultHash = shapes.Op_ClearAllPinsInternal.Execute(baseDefinition);
		if (string.IsNullOrEmpty(resultHash))
		{
			return hash;
		}
		return resultHash;
	}

	protected void AddMappings(string sourceHash, string label, IResearchableHandle researchable = null)
	{
		ShapeManager shapes = Singleton<GameCore>.G.Shapes;
		ShapeDefinition sourceDefinition;
		try
		{
			sourceDefinition = shapes.GetDefinitionByHash(TrimShape(sourceHash));
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to add mapping for " + sourceHash + " @ " + label + ": " + ex);
			throw ex;
		}
		for (int rotation = 0; rotation < 4; rotation++)
		{
			string rotatedHash = Singleton<GameCore>.G.Shapes.Op_Rotate.Execute(new ShapeOperationRotatePayload
			{
				AmountClockwise = rotation,
				Shape = sourceDefinition
			});
			if (DefinitionMappings.TryGetValue(rotatedHash, out var mapped))
			{
				if (mapped.TargetHash != sourceHash)
				{
					Debug.LogError($"ResearchShapeManager:: Ambiguous research (r={rotation}): Existing mapping {rotatedHash} -> {mapped.TargetHash} for {mapped.Label} but new mapping is {rotatedHash} -> {sourceHash} for {label}");
					continue;
				}
			}
			else
			{
				mapped = new MappedShape
				{
					TargetHash = sourceHash,
					Label = label,
					Researchables = new List<IResearchableHandle>()
				};
				DefinitionMappings[rotatedHash] = mapped;
			}
			if (researchable != null)
			{
				ref string label2 = ref mapped.Label;
				label2 = label2 + "; " + label;
				mapped.Researchables.Add(researchable);
			}
		}
	}
}
