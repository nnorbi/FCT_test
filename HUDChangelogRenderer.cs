using System.Collections.Generic;
using Core.Dependency;
using TMPro;
using UnityEngine;

public class HUDChangelogRenderer : HUDComponent
{
	[SerializeField]
	private TMP_Text UIChangelogText;

	[Construct]
	private void Construct()
	{
	}

	public void ShowChangelogSince(Changelog changelog, string untilVersion)
	{
		IReadOnlyCollection<Changelog.Entry> entries = changelog.Entries;
		string result = "";
		Debug.Log("Rendering " + entries.Count + " changelog entries since " + untilVersion);
		foreach (Changelog.Entry entry in entries)
		{
			if (!string.IsNullOrEmpty(untilVersion) && entry.Version == untilVersion)
			{
				break;
			}
			result = result + BuildEntryText(entry) + "\n\n\n";
		}
		UIChangelogText.text = result;
	}

	public void ShowFullChangelog(Changelog changelog)
	{
		ShowChangelogSince(changelog, null);
	}

	private string BuildEntryText(Changelog.Entry entry)
	{
		string categoryPrefix = "[[";
		string categorySuffix = "]]";
		string result = "<color=#FFA036><size=30>" + entry.Version + "</size></color> - <b>" + entry.Date + "</b>\n";
		string[] entries = entry.Entries;
		foreach (string bulletPoint in entries)
		{
			if (bulletPoint.StartsWith(categoryPrefix) && bulletPoint.EndsWith(categorySuffix))
			{
				string header = bulletPoint.Substring(categoryPrefix.Length, bulletPoint.Length - categorySuffix.Length - categoryPrefix.Length);
				result = result + "\n<b><size=20>" + header.ToUpperInvariant() + "</size></b>\n";
			}
			else
			{
				result = result + "- " + bulletPoint + "\n";
			}
		}
		return result;
	}

	protected override void OnDispose()
	{
	}
}
