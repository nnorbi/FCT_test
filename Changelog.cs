using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class Changelog
{
	public class Entry
	{
		public readonly string Date;

		public readonly string Version;

		public readonly string[] Entries;

		public Entry(string date, string version, string[] entries)
		{
			Date = date;
			Version = version;
			Entries = entries.ToArray();
		}
	}

	public const string CATEGORY_PREFIX = "[[";

	public const string CATEGORY_SUFFIX = "]]";

	private Entry[] _Entries;

	public IReadOnlyCollection<Entry> Entries
	{
		get
		{
			if (_Entries == null)
			{
				throw new InvalidOperationException("Changelog not yet loaded. Call Load first");
			}
			return (IReadOnlyCollection<Entry>)(object)_Entries;
		}
	}

	public string LatestEntryId
	{
		get
		{
			Entry[] entries = _Entries;
			if (entries == null || entries.Length == 0)
			{
				return null;
			}
			return entries[0].Version;
		}
	}

	public void Load()
	{
		if (_Entries != null)
		{
			return;
		}
		try
		{
			TextAsset data = Resources.Load<TextAsset>("Changelog");
			if (data == null)
			{
				throw new Exception("Empty data");
			}
			_Entries = JsonConvert.DeserializeObject<Entry[]>(data.text);
			Debug.Log("Loaded " + _Entries.Length + " changelog entries");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to load changelog:" + ex);
			_Entries = new Entry[1]
			{
				new Entry("unknown", "unknown", new string[1] { "Failed to load changelog: " + ex.Message })
			};
		}
	}
}
