using System.Collections.Generic;
using UnityEngine.Events;

public interface ISavegameManager : ISavegameNameProvider
{
	UnityEvent<SavegameReference> OnSavegameRemoved { get; }

	UnityEvent<SavegameReference> OnSavegameAdded { get; }

	void CleanupOldSavegameBackups(string uid);

	void Import(string sourceFile);

	void RenameSavegame(SavegameReference reference, string newName);

	void DeleteSavegame(SavegameReference reference);

	SavegameReference FindMostRecentSavegameEntryByUID(string uid);

	string BuildSavegameNameFromUID(string uid);

	List<SavegameReference> DiscoverAllSavegames();

	void CreateSavegamesFolder();

	string GenerateNewUID();
}
