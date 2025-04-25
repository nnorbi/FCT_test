using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class SavegameManager : ISavegameNameProvider, ISavegameManager
{
	public const string SAVEGAME_FILE_PREFIX = "backup-v";

	public const string SAVEGAME_EXTENSION = ".spz2";

	public UnityEvent<SavegameReference> OnSavegameRemoved { get; } = new UnityEvent<SavegameReference>();

	public UnityEvent<SavegameReference> OnSavegameAdded { get; } = new UnityEvent<SavegameReference>();

	public void DeleteSavegame(SavegameReference reference)
	{
		string targetDirectory = Path.Join(GameEnvironmentManager.SAVEGAME_PATH, reference.UID);
		Directory.Delete(targetDirectory, recursive: true);
		OnSavegameRemoved.Invoke(reference);
	}

	public void CleanupOldSavegameBackups(string uid)
	{
		int keep = Globals.Settings.General.SavegameBackupCount.Current.Value;
		if (keep < 0)
		{
			return;
		}
		DateTime oneHourBefore = DateTime.Now.AddHours(-1.0);
		List<SavegameReference> versions = (from savegameReference in DiscoverAllEntriesForSavegame(uid)
			orderby savegameReference.SnapshotIndex
			select savegameReference).ToList();
		Debug.Log("SavegameManager:: Cleaning backups for " + uid + ", keep=" + keep + ", found=" + versions.Count);
		if (versions.Count <= keep)
		{
			return;
		}
		IEnumerable<SavegameReference> entriesToDelete = versions.Where((SavegameReference savegameReference) => savegameReference.LastChange < oneHourBefore && savegameReference.SnapshotIndex % 50 != 1).Take(math.min(versions.Count - keep, 25));
		foreach (SavegameReference entry in entriesToDelete)
		{
			Debug.Log("SavegameManager:: Delete old " + entry.SnapshotIndex + " " + entry.LastChange.ToString() + " " + entry.FullPath);
			try
			{
				File.Delete(entry.FullPath);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("SavegameManager:: Failed to delete old savegame backup: " + entry.FullPath + ": " + ex);
			}
		}
	}

	public void RenameSavegame(SavegameReference reference, string newName)
	{
		PlayerPrefs.SetString("savegame." + reference.UID + ".name", newName);
		PlayerPrefs.Save();
	}

	public SavegameReference FindMostRecentSavegameEntryByUID(string uid)
	{
		SavegameReference best = null;
		foreach (SavegameReference entry in DiscoverAllEntriesForSavegame(uid))
		{
			if (best == null)
			{
				best = entry;
			}
			else if (entry.SnapshotIndex > best.SnapshotIndex)
			{
				best = entry;
			}
			else if (entry.SnapshotIndex == best.SnapshotIndex && entry.LastChange > best.LastChange)
			{
				Debug.LogWarning("Found 2 conflicting snapshots, using the one with the later modification date.");
				best = entry;
			}
		}
		if (best != null)
		{
		}
		return best;
	}

	public string BuildSavegameNameFromUID(string uid)
	{
		SavegameReference last = FindMostRecentSavegameEntryByUID(uid);
		if (last == null)
		{
			return BuildFilename(uid, 1);
		}
		return BuildFilename(uid, last.SnapshotIndex + 1);
	}

	public List<SavegameReference> DiscoverAllSavegames()
	{
		List<SavegameReference> result = new List<SavegameReference>();
		string savegameFolder = GameEnvironmentManager.SAVEGAME_PATH;
		string[] potentialUids = Directory.GetDirectories(savegameFolder);
		string[] array = potentialUids;
		foreach (string dir in array)
		{
			string uid = Path.GetFileName(dir);
			SavegameReference entry = FindMostRecentSavegameEntryByUID(uid);
			if (entry != null)
			{
				result.Add(entry);
			}
		}
		return result;
	}

	public void CreateSavegamesFolder()
	{
		try
		{
			if (!Directory.Exists(GameEnvironmentManager.SAVEGAME_PATH))
			{
				Directory.CreateDirectory(GameEnvironmentManager.SAVEGAME_PATH);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to create savegame folder at " + GameEnvironmentManager.SAVEGAME_PATH + ": " + ex);
		}
	}

	public string GenerateNewUID()
	{
		return Guid.NewGuid().ToString("D");
	}

	public void Import(string sourceFile)
	{
		string uid = Globals.Savegames.GenerateNewUID();
		string targetDirectory = Path.Join(GameEnvironmentManager.SAVEGAME_PATH, uid);
		Directory.CreateDirectory(targetDirectory);
		string filename = BuildFilename(uid, 1);
		File.Copy(sourceFile, filename);
		File.SetLastWriteTime(filename, DateTime.Now);
		SavegameReference entry = FindMostRecentSavegameEntryByUID(uid);
		if (entry == null)
		{
			throw new Exception("Imported savegame but couldn't find entry for " + uid);
		}
		OnSavegameAdded.Invoke(entry);
	}

	public string GetSavegameDisplayName(SavegameReference reference)
	{
		return PlayerPrefs.GetString("savegame." + reference.UID + ".name", "menu.play.default-savegame-title".tr());
	}

	private List<SavegameReference> DiscoverAllEntriesForSavegame(string uid)
	{
		List<SavegameReference> result = new List<SavegameReference>();
		string savegameDirectory = Path.Join(GameEnvironmentManager.SAVEGAME_PATH, uid);
		if (!Directory.Exists(savegameDirectory))
		{
			return result;
		}
		string[] entries = Directory.GetFiles(savegameDirectory);
		string[] array = entries;
		foreach (string file in array)
		{
			try
			{
				string snapshotName = Path.GetFileNameWithoutExtension(file);
				if (snapshotName.StartsWith("backup-v") && file.EndsWith(".spz2"))
				{
					string actualPart = snapshotName.Substring("backup-v".Length).Split("-")[0];
					int.TryParse(actualPart, out var snapshotIndex);
					DateTime lastWrite = File.GetLastWriteTime(file);
					result.Add(new SavegameReference
					{
						UID = uid,
						FullPath = file,
						LastChange = lastWrite,
						SnapshotIndex = snapshotIndex
					});
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(" Failed to process " + file + ": " + ex);
			}
		}
		return result;
	}

	private string BuildFilename(string uid, int snapshotIndex)
	{
		return Path.Join(GameEnvironmentManager.SAVEGAME_PATH, uid, "backup-v" + snapshotIndex + "-" + DateTime.Now.ToString("yyyy-M-dd--HH-mm-ss--fffffff") + ".spz2");
	}
}
