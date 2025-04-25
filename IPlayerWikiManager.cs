using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public interface IPlayerWikiManager
{
	UnityEvent Changed { get; }

	bool HasRead(MetaWikiEntry entry);

	void MarkRead(MetaWikiEntry entry);

	IEnumerable<MetaWikiEntry> GetAvailableWikiEntries(ResearchProgress progress);

	int ComputeUnreadCount(ResearchProgress progress)
	{
		return GetAvailableWikiEntries(progress).Count((MetaWikiEntry entry) => !HasRead(entry));
	}
}
