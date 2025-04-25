using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BlueprintLibrary : IBlueprintLibraryAccess
{
	private static string BP_FILENAME_SUFFIX = ".spz2bp";

	private List<BlueprintLibraryEntry> Entries = new List<BlueprintLibraryEntry>();

	public UnityEvent Changed { get; } = new UnityEvent();

	public void SaveEntry(BlueprintLibraryEntry entry)
	{
		string filename = DetermineFullPath(entry.Title);
		string serialized = BlueprintSerializer.Serialize(entry.Blueprint);
		try
		{
			File.WriteAllText(filename, serialized);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Failed to save blueprint to '{filename}': {arg}");
		}
		Refresh();
	}

	public void RemoveEntry(BlueprintLibraryEntry entry)
	{
		string filename = DetermineFullPath(entry.Title);
		try
		{
			File.Delete(filename);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Failed to save blueprint to '{filename}': {arg}");
		}
		Refresh();
	}

	public void Refresh()
	{
		List<FileInfo> files;
		try
		{
			DirectoryInfo directory = Directory.CreateDirectory(GameEnvironmentManager.BLUEPRINT_LIBRARY_PATH);
			files = (from f in directory.EnumerateFiles()
				where f.Name.EndsWith(BP_FILENAME_SUFFIX)
				select f).ToList();
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to create and/or read blueprint library folder: " + ex);
			return;
		}
		Entries.Clear();
		files.Sort((FileInfo a, FileInfo b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
		foreach (FileInfo file in files)
		{
			string fullPath = file.FullName;
			try
			{
				string contents = File.ReadAllText(fullPath);
				if (!BlueprintSerializer.TryDeserialize(contents, out var blueprint, out var exception, trySanitize: true))
				{
					throw exception;
				}
				string name = file.Name.Substring(0, file.Name.Length - BP_FILENAME_SUFFIX.Length);
				Entries.Add(new BlueprintLibraryEntry(name, blueprint));
			}
			catch (Exception arg)
			{
				Debug.LogWarning($"Failed to read blueprint from '{fullPath}': {arg}");
			}
		}
		Changed.Invoke();
	}

	public IReadOnlyList<BlueprintLibraryEntry> GetEntries()
	{
		return Entries;
	}

	public bool TryRenameEntry(BlueprintLibraryEntry entry, string newName)
	{
		string oldFilename = DetermineFullPath(entry.Title);
		string newFilename = DetermineFullPath(newName);
		try
		{
			File.Move(oldFilename, newFilename);
		}
		catch (Exception arg)
		{
			Debug.LogWarning($"Failed to move blueprint from '{oldFilename}' -> '{newFilename}': {arg}");
			return false;
		}
		Refresh();
		return true;
	}

	private string DetermineFileName(string title)
	{
		string sanitized = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
		while (sanitized.Length < 3)
		{
			sanitized += "_";
		}
		return sanitized + BP_FILENAME_SUFFIX;
	}

	private string DetermineFullPath(string title)
	{
		return Path.Join(GameEnvironmentManager.BLUEPRINT_LIBRARY_PATH, DetermineFileName(title));
	}
}
