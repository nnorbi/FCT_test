using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public static class ShapeLogic
{
	public class ShapePartGroup
	{
		public int FallDown;

		public bool Vanish = false;

		public List<ShapePartReference> References;

		public bool CanMerge(ShapePartGroup other)
		{
			return FallDown == other.FallDown && Vanish == other.Vanish;
		}
	}

	public class UnfoldResult
	{
		public List<ShapePartReference> References = new List<ShapePartReference>();

		public List<HashSet<ShapePartReference>> FusedReferences = new List<HashSet<ShapePartReference>>();
	}

	public static bool VERBOSE;

	public static bool Logic_IsConnected(ShapePartReference a, ShapePartReference b, int partCount)
	{
		if (a.LayerIndex == b.LayerIndex && (FastMath.SafeMod(a.PartIndex + 1, partCount) == b.PartIndex || FastMath.SafeMod(a.PartIndex - 1, partCount) == b.PartIndex) && a.Part.Shape.Code != 'P' && b.Part.Shape.Code != 'P')
		{
			return true;
		}
		return false;
	}

	public static bool Logic_IsFused(ShapePartReference a, ShapePartReference b, int partCount)
	{
		if (a.Part.Shape.Code != 'c' || b.Part.Shape.Code != 'c')
		{
			return false;
		}
		if (a.LayerIndex == b.LayerIndex)
		{
			return FastMath.SafeMod(a.PartIndex + 1, partCount) == b.PartIndex || FastMath.SafeMod(a.PartIndex - 1, partCount) == b.PartIndex;
		}
		if (a.PartIndex == b.PartIndex && math.abs(a.LayerIndex - b.LayerIndex) == 1)
		{
			return true;
		}
		return false;
	}

	public static bool Logic_AllowFallDownByOne(ShapePartReference fallDown, ShapePartReference potentialBlocker)
	{
		if (fallDown.PartIndex != potentialBlocker.PartIndex)
		{
			return true;
		}
		if (fallDown.LayerIndex == potentialBlocker.LayerIndex + 1)
		{
			return false;
		}
		return true;
	}

	public static float Logic_LayerScale(int index)
	{
		return math.pow(1f - Globals.Resources.ShapeLayerScaleReduction, index);
	}

	public static UnfoldResult Unfold(ShapeLayer[] layers)
	{
		UnfoldResult result = new UnfoldResult();
		for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
		{
			ShapeLayer layer = layers[layerIndex];
			for (int partIndex = 0; partIndex < layer.Parts.Length; partIndex++)
			{
				ShapePart part = layer.Parts[partIndex];
				if (!part.IsEmpty)
				{
					result.References.Add(new ShapePartReference
					{
						LayerIndex = layerIndex,
						Part = part,
						PartIndex = partIndex
					});
				}
			}
		}
		Dictionary<ShapePartReference, HashSet<ShapePartReference>> partToGroup = new Dictionary<ShapePartReference, HashSet<ShapePartReference>>();
		int partCount = layers[0].Parts.Length;
		List<HashSet<ShapePartReference>> groups = new List<HashSet<ShapePartReference>>();
		foreach (ShapePartReference reference in result.References)
		{
			HashSet<ShapePartReference> connected = new HashSet<ShapePartReference>();
			foreach (ShapePartReference otherReference in result.References)
			{
				if (reference != otherReference && Logic_IsFused(reference, otherReference, partCount))
				{
					HashSet<ShapePartReference> referenceGroup = null;
					HashSet<ShapePartReference> otherReferenceGroup = null;
					partToGroup.TryGetValue(reference, out referenceGroup);
					partToGroup.TryGetValue(otherReference, out otherReferenceGroup);
					if (referenceGroup == null && otherReferenceGroup == null)
					{
						HashSet<ShapePartReference> group = (partToGroup[otherReference] = (partToGroup[reference] = new HashSet<ShapePartReference> { reference, otherReference }));
						groups.Add(group);
					}
					else if (referenceGroup == null)
					{
						partToGroup[reference] = otherReferenceGroup;
						otherReferenceGroup.Add(reference);
					}
					else if (otherReferenceGroup == null)
					{
						partToGroup[otherReference] = referenceGroup;
						referenceGroup.Add(otherReference);
					}
					else if (referenceGroup != otherReferenceGroup)
					{
						referenceGroup.UnionWith(otherReferenceGroup);
						groups.Remove(otherReferenceGroup);
					}
				}
			}
		}
		if (VERBOSE)
		{
			Debug.Log("  GROUPS: ");
			foreach (HashSet<ShapePartReference> group2 in groups)
			{
				Debug.Log(" ---- GROUP ---- ");
				foreach (ShapePartReference item in group2)
				{
					Debug.Log("  " + item);
				}
			}
		}
		result.FusedReferences = groups;
		return result;
	}

	public static ShapeLayer[] Fold(List<ShapePartReference> references, int partCount)
	{
		if (references.Count == 0)
		{
			return null;
		}
		List<ShapeLayer> result = new List<ShapeLayer>();
		foreach (ShapePartReference part in references)
		{
			while (result.Count <= part.LayerIndex)
			{
				result.Add(ShapeLayer.Empty(partCount));
			}
			result[part.LayerIndex].Parts[part.PartIndex] = part.Part;
		}
		return result.ToArray();
	}

	public static ShapeDefinition FoldAndWrap(List<ShapePartReference> references, int partCount)
	{
		ShapeLayer[] result = Fold(references, partCount);
		if (result == null)
		{
			return null;
		}
		return new ShapeDefinition(result);
	}

	public static string FoldAndHash(List<ShapePartReference> references, int partCount)
	{
		ShapeLayer[] result = Fold(references, partCount);
		if (result == null)
		{
			return null;
		}
		return new ShapeDefinition(result).Hash;
	}

	public static ShapePartGroup SplitGroupIntoKeepAndVanish(ShapePartGroup group, List<ShapePartReference> keep, List<ShapePartReference> vanish)
	{
		if (group.Vanish)
		{
			return null;
		}
		if (vanish.Count == 0)
		{
			return null;
		}
		if (keep.Count == 0)
		{
			if (VERBOSE)
			{
				Debug.Log("  -> Group vanish completely!");
			}
			group.Vanish = true;
			return null;
		}
		if (VERBOSE)
		{
			Debug.Log("  -> Group vanish partially! Keep " + keep.Count + ", destroy " + vanish.Count);
		}
		group.References = keep;
		return new ShapePartGroup
		{
			FallDown = group.FallDown,
			References = vanish,
			Vanish = true
		};
	}

	public static void MergeGroups(List<ShapePartGroup> groups)
	{
		if (VERBOSE)
		{
			Debug.Log("Re-grouping mergable groups");
		}
		bool anyGroupMerged = false;
		do
		{
			anyGroupMerged = false;
			if (VERBOSE)
			{
				Debug.Log("  Iteration:");
			}
			foreach (ShapePartGroup group in groups)
			{
				if (VERBOSE)
				{
					Debug.Log("   Group Fall Down " + group.FallDown + " references " + group.References.Count);
				}
				foreach (ShapePartGroup otherGroup in groups)
				{
					if (group == otherGroup || !group.CanMerge(otherGroup))
					{
						continue;
					}
					if (VERBOSE)
					{
						Debug.Log("     Merge with fall down" + otherGroup.FallDown + " references " + otherGroup.References.Count);
					}
					group.References.AddRange(otherGroup.References);
					groups.Remove(otherGroup);
					anyGroupMerged = true;
					break;
				}
				if (anyGroupMerged)
				{
					break;
				}
			}
		}
		while (anyGroupMerged);
	}

	public static List<ShapePartReference> GetAllReferences(IEnumerable<ShapePartGroup> groups)
	{
		if (VERBOSE)
		{
			Debug.Log("Final result after grouping:");
		}
		List<ShapePartReference> allReferences = new List<ShapePartReference>();
		foreach (ShapePartGroup group in groups)
		{
			if (VERBOSE)
			{
				Debug.Log("  GROUP");
			}
			foreach (ShapePartReference reference in group.References)
			{
				if (VERBOSE)
				{
					Debug.Log("   " + reference);
				}
				if (!group.Vanish)
				{
					allReferences.Add(reference);
				}
			}
		}
		return allReferences;
	}

	public static void ComputeFusingResults(List<ShapePartGroup> groups, List<HashSet<ShapePartReference>> fuseGroups, List<ShapePartReference> references, List<ShapePartReference> originalReferences)
	{
		if (VERBOSE)
		{
			Debug.Log("Removing fused groups");
		}
		HashSet<ShapePartReference> removeReferences = new HashSet<ShapePartReference>();
		foreach (HashSet<ShapePartReference> fuseGroup in fuseGroups)
		{
			bool groupSeperated = false;
			foreach (ShapePartReference fuseReference in fuseGroup)
			{
				if (originalReferences.Contains(fuseReference))
				{
					continue;
				}
				if (VERBOSE)
				{
					Debug.Log("Fuse group got destroyed! Missing part " + fuseReference);
				}
				groupSeperated = true;
				break;
			}
			if (!groupSeperated)
			{
				continue;
			}
			foreach (ShapePartReference fuseReference2 in fuseGroup)
			{
				if (originalReferences.Contains(fuseReference2))
				{
					removeReferences.Add(references.Find((ShapePartReference r) => r.LayerIndex == fuseReference2.LayerIndex && r.PartIndex == fuseReference2.PartIndex));
				}
			}
		}
		if (VERBOSE)
		{
			Debug.Log("Removing " + removeReferences.Count + " references from groups:");
			foreach (ShapePartReference item in removeReferences)
			{
				Debug.Log("  Remove " + item?.ToString() + " because of fuse group destroy");
			}
		}
		foreach (ShapePartGroup group in groups.ToList())
		{
			if (group.Vanish)
			{
				continue;
			}
			List<ShapePartReference> keep = new List<ShapePartReference>();
			List<ShapePartReference> vanish = new List<ShapePartReference>();
			foreach (ShapePartReference reference in group.References)
			{
				if (removeReferences.Contains(reference))
				{
					vanish.Add(reference);
				}
				else
				{
					keep.Add(reference);
				}
			}
			if (VERBOSE)
			{
				Debug.Log("Group: Keep " + keep.Count + " vanish " + vanish.Count);
			}
			ShapePartGroup newGroup = SplitGroupIntoKeepAndVanish(group, keep, vanish);
			if (newGroup != null)
			{
				groups.Add(newGroup);
			}
		}
	}

	public static void SimulateFallDownPhysics(List<ShapePartGroup> groups)
	{
		if (VERBOSE)
		{
			Debug.Log("Simulating physics:");
		}
		bool anyChange = false;
		do
		{
			if (VERBOSE)
			{
				Debug.Log(" Iteration: ");
			}
			anyChange = false;
			foreach (ShapePartGroup group in groups)
			{
				if (VERBOSE)
				{
					Debug.Log("  Group");
				}
				if (group.Vanish)
				{
				}
				bool groupCanFallDown = true;
				foreach (ShapePartReference reference in group.References)
				{
					if (reference.LayerIndex <= 0)
					{
						groupCanFallDown = false;
						break;
					}
					if (VERBOSE)
					{
						Debug.Log("   " + reference);
					}
					bool referenceCanFallDown = true;
					foreach (ShapePartGroup otherGroup in groups)
					{
						if (group == otherGroup || otherGroup.Vanish)
						{
							continue;
						}
						foreach (ShapePartReference otherReference in otherGroup.References)
						{
							if (!Logic_AllowFallDownByOne(reference, otherReference))
							{
								if (VERBOSE)
								{
									Debug.Log("    -> (!) blocked by " + otherReference);
								}
								referenceCanFallDown = false;
								break;
							}
						}
					}
					if (!referenceCanFallDown)
					{
						groupCanFallDown = false;
						break;
					}
				}
				if (!groupCanFallDown)
				{
					continue;
				}
				if (VERBOSE)
				{
					Debug.Log("  -> Group fall down by 1!");
				}
				anyChange = true;
				group.FallDown++;
				List<ShapePartReference> vanish = new List<ShapePartReference>();
				List<ShapePartReference> keep = new List<ShapePartReference>();
				foreach (ShapePartReference reference2 in group.References)
				{
					if (reference2.Part.Shape.DestroyOnFallDown)
					{
						vanish.Add(reference2);
					}
					else
					{
						keep.Add(reference2);
					}
					reference2.LayerIndex--;
				}
				ShapePartGroup newGroup = SplitGroupIntoKeepAndVanish(group, keep, vanish);
				if (newGroup != null)
				{
					groups.Add(newGroup);
					break;
				}
			}
		}
		while (anyChange);
	}

	public static void RemoveExcessLayers(List<ShapePartGroup> groups, int maxLayers)
	{
		List<ShapePartGroup> additionalGroups = new List<ShapePartGroup>();
		if (VERBOSE)
		{
			Debug.Log("Un-grouping based on cut-off layers (max=" + maxLayers + ")");
		}
		foreach (ShapePartGroup group in groups)
		{
			List<ShapePartReference> cutOffReferences = group.References.Where((ShapePartReference shapePartReference) => shapePartReference.LayerIndex >= maxLayers).ToList();
			if (cutOffReferences.Count <= 0)
			{
				continue;
			}
			if (VERBOSE)
			{
				Debug.Log("  split off " + cutOffReferences.Count + " references from group since they will be cut off into new (vanishing) group");
			}
			foreach (ShapePartReference reference in cutOffReferences)
			{
				group.References.Remove(reference);
			}
			additionalGroups.Add(new ShapePartGroup
			{
				FallDown = group.FallDown,
				References = cutOffReferences,
				Vanish = true
			});
		}
		groups.AddRange(additionalGroups);
	}

	public static List<ShapePartGroup> BuildGroups(List<ShapePartReference> references, int partCount)
	{
		List<ShapePartGroup> groups = new List<ShapePartGroup>();
		List<ShapePartReference> consumableReferences = references.ToList();
		while (consumableReferences.Count > 0)
		{
			ShapePartReference primaryReference = consumableReferences[0];
			List<ShapePartReference> group = new List<ShapePartReference> { primaryReference };
			bool anyAdded = false;
			do
			{
				anyAdded = false;
				foreach (ShapePartReference potentialReference in consumableReferences)
				{
					if (group.Contains(potentialReference))
					{
						continue;
					}
					foreach (ShapePartReference groupMemberReference in group)
					{
						if (Logic_IsConnected(groupMemberReference, potentialReference, partCount))
						{
							group.Add(potentialReference);
							anyAdded = true;
							break;
						}
					}
				}
			}
			while (anyAdded);
			foreach (ShapePartReference part in group)
			{
				consumableReferences.Remove(part);
			}
			groups.Add(new ShapePartGroup
			{
				References = group,
				FallDown = 0,
				Vanish = false
			});
		}
		return groups;
	}

	public static ShapeCollapseResult Collapse(List<ShapePartReference> sourceReferences, int partCount, List<HashSet<ShapePartReference>> fuseGroups = null)
	{
		if (sourceReferences.Count == 0)
		{
			return null;
		}
		List<ShapePartReference> references = sourceReferences.Select((ShapePartReference reference) => new ShapePartReference(reference)).ToList();
		if (VERBOSE)
		{
			Debug.Log("--------------------------------");
			Debug.Log("Shape part references:");
			foreach (ShapePartReference item in references)
			{
				Debug.Log("   " + item);
			}
		}
		List<ShapePartGroup> groups = BuildGroups(references, partCount);
		if (VERBOSE)
		{
			Debug.Log("Found the following groups: ");
		}
		foreach (ShapePartGroup group in groups)
		{
			if (VERBOSE)
			{
				Debug.Log("  GROUP");
			}
			foreach (ShapePartReference part in group.References)
			{
				if (VERBOSE)
				{
					Debug.Log("   " + part);
				}
			}
		}
		if (fuseGroups != null && fuseGroups.Count > 0)
		{
			ComputeFusingResults(groups, fuseGroups, references, sourceReferences);
		}
		SimulateFallDownPhysics(groups);
		RemoveExcessLayers(groups, Singleton<GameCore>.G.Mode.MaxShapeLayers);
		groups = groups.Where((ShapePartGroup shapePartGroup) => shapePartGroup.References.Count > 0).ToList();
		MergeGroups(groups);
		List<ShapePartReference> allReferences = GetAllReferences(groups);
		string finalDefinition = FoldAndHash(allReferences, partCount);
		return new ShapeCollapseResult
		{
			ResultDefinition = finalDefinition,
			Entries = groups.Select((ShapePartGroup shapePartGroup) => new ShapeCollapseResultEntry
			{
				ShapeDefinition = FoldAndHash(shapePartGroup.References, partCount),
				FallDownLayers = shapePartGroup.FallDown,
				Vanish = shapePartGroup.Vanish
			}).ToArray()
		};
	}
}
