using System.Collections.Generic;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDWikiEntryRenderer : HUDComponent
{
	[SerializeField]
	private RectTransform UIContentParent;

	[SerializeField]
	private Scrollbar UIContentScrollbar;

	[SerializeField]
	private PrefabViewReference<HUDWikiComponentHeader> UIPrefabLargeHeader;

	[SerializeField]
	private PrefabViewReference<HUDWikiComponentHeader> UIPrefabSubHeader;

	[SerializeField]
	private PrefabViewReference<HUDWikiComponentText> UIPrefabText;

	[SerializeField]
	private PrefabViewReference<HUDWikiComponentImage> UIPrefabImage;

	[SerializeField]
	private PrefabViewReference<HUDWikiComponentLocked> UIPrefabLocked;

	public readonly UnityEvent<string> OnWikiLinkClicked = new UnityEvent<string>();

	private List<HUDComponent> CurrentContents = new List<HUDComponent>();

	private MetaWikiEntry _Entry;

	private ResearchManager Research;

	public MetaWikiEntry Entry
	{
		get
		{
			return _Entry;
		}
		set
		{
			if (!(_Entry == value))
			{
				_Entry = value;
				RenderEntry();
			}
		}
	}

	[Construct]
	private void Construct(ResearchManager research)
	{
		Research = research;
		Research.Progress.OnChanged.AddListener(RenderEntry);
		Globals.Keybindings.Changed.AddListener(RenderEntry);
		UIContentParent.RemoveAllChildren();
	}

	protected override void OnDispose()
	{
		Research.Progress.OnChanged.RemoveListener(RenderEntry);
		Globals.Keybindings.Changed.RemoveListener(RenderEntry);
		ClearContents();
	}

	private void ClearContents()
	{
		foreach (HUDComponent entry in CurrentContents)
		{
			ReleaseChildView(entry);
		}
		CurrentContents.Clear();
	}

	private void RenderEntry()
	{
		ClearContents();
		if (_Entry == null)
		{
			return;
		}
		HUDWikiComponentHeader header = RequestChildView(UIPrefabLargeHeader).PlaceAt(UIContentParent);
		header.Title = _Entry.Title;
		CurrentContents.Add(header);
		MetaResearchable lastLocked = null;
		MetaWikiEntry.Entry[] entries = _Entry.Entries;
		foreach (MetaWikiEntry.Entry contentEntry in entries)
		{
			if (contentEntry.RequiresResearch != null && !Research.Progress.IsUnlocked(contentEntry.RequiresResearch))
			{
				if (!(lastLocked == contentEntry.RequiresResearch))
				{
					lastLocked = contentEntry.RequiresResearch;
					HUDWikiComponentLocked instance = RequestChildView(UIPrefabLocked).PlaceAt(UIContentParent);
					instance.Research = contentEntry.RequiresResearch;
					CurrentContents.Add(instance);
				}
				continue;
			}
			lastLocked = null;
			switch (contentEntry.Type)
			{
			case MetaWikiEntry.EntryType.Text:
			{
				HUDWikiComponentText instance4 = RequestChildView(UIPrefabText).PlaceAt(UIContentParent);
				instance4.TranslationId = contentEntry.SubTextId;
				instance4.LinkClicked.AddListener(OnLinkClicked);
				CurrentContents.Add(instance4);
				break;
			}
			case MetaWikiEntry.EntryType.Heading:
			{
				HUDWikiComponentHeader instance3 = RequestChildView(UIPrefabSubHeader).PlaceAt(UIContentParent);
				instance3.Title = contentEntry.SubHeadingId.tr();
				CurrentContents.Add(instance3);
				break;
			}
			case MetaWikiEntry.EntryType.Image:
			{
				HUDWikiComponentImage instance2 = RequestChildView(UIPrefabImage).PlaceAt(UIContentParent);
				instance2.Sprite = contentEntry.Image;
				CurrentContents.Add(instance2);
				break;
			}
			}
		}
		UIContentScrollbar.value = 1f;
	}

	private void OnLinkClicked(string linkId)
	{
		if (linkId.StartsWith("wiki:"))
		{
			string wikiEntryId = linkId.Substring("wiki:".Length);
			OnWikiLinkClicked.Invoke(wikiEntryId);
		}
		else
		{
			Debug.LogError("Invalid link type: '" + linkId + "'");
			Globals.UISounds.PlayError();
		}
	}
}
