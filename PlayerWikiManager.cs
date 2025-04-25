using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PlayerWikiManager : IPlayerWikiManager
{
	public struct SerializedData
	{
		public string[] Entries;
	}

	private HashSet<string> ReadEntries = new HashSet<string>();

	public UnityEvent Changed { get; } = new UnityEvent();

	public bool HasRead(MetaWikiEntry entry)
	{
		return entry.ReadByDefault || ReadEntries.Contains(entry.name);
	}

	public void MarkRead(MetaWikiEntry entry)
	{
		if (ReadEntries.Add(entry.name))
		{
			Changed.Invoke();
		}
	}

	public IEnumerable<MetaWikiEntry> GetAvailableWikiEntries(ResearchProgress progress)
	{
		HashSet<MetaWikiEntry> seen = new HashSet<MetaWikiEntry>();
		foreach (MetaResearchable node in progress.UnlockedResearchables)
		{
			foreach (IResearchUnlock entry in node.Unlocks)
			{
				if (entry is MetaWikiEntry wikiEntry)
				{
					if (seen.Contains(wikiEntry))
					{
						Debug.LogWarning("Duplicate wikiEntry: " + wikiEntry.name + " in research " + node.name);
					}
					else if (IsEntryAvailable(wikiEntry, progress))
					{
						seen.Add(wikiEntry);
						yield return wikiEntry;
					}
				}
			}
		}
	}

	public bool IsEntryAvailable(MetaWikiEntry entry, ResearchProgress progress)
	{
		if (entry.RequiresResearch != null)
		{
			return progress.IsUnlocked(entry.RequiresResearch);
		}
		return true;
	}

	public SerializedData Serialize()
	{
		return new SerializedData
		{
			Entries = ReadEntries.ToArray()
		};
	}

	public void Deserialize(SerializedData serializedData)
	{
		ReadEntries = (serializedData.Entries ?? new string[0]).ToHashSet();
		Changed.Invoke();
	}
}
