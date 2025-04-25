using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WKExample", menuName = "Metadata/Wiki Entry")]
public class MetaWikiEntry : ScriptableObject, IResearchUnlock
{
	[Serializable]
	public class Entry
	{
		public EntryType Type;

		[ValidateTranslation]
		public string SubTextId;

		[ValidateTranslation]
		public string SubHeadingId;

		public Sprite Image;

		public MetaResearchable RequiresResearch;
	}

	[Serializable]
	public enum EntryType
	{
		Text,
		Heading,
		Image
	}

	public MetaResearchable RequiresResearch;

	public bool ReadByDefault = false;

	public Entry[] Entries;

	public Sprite Icon => null;

	public string Title => ("wiki." + base.name + ".title").tr();
}
