using System.Collections.Generic;
using UnityEngine.Events;

public interface IBlueprintLibraryAccess
{
	UnityEvent Changed { get; }

	void Refresh();

	IReadOnlyList<BlueprintLibraryEntry> GetEntries();

	void SaveEntry(BlueprintLibraryEntry entry);

	void RemoveEntry(BlueprintLibraryEntry entry);

	bool TryRenameEntry(BlueprintLibraryEntry entry, string newName);
}
