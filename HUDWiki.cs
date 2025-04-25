using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Core.Events;
using Unity.Core.View;
using UnityEngine;

public class HUDWiki : HUDPart
{
	[SerializeField]
	private HUDDialog UIMainDialog;

	[SerializeField]
	private RectTransform UINavParent;

	[SerializeField]
	private PrefabViewReference<HUDWikiNavEntry> UINavItemPrefab;

	[SerializeField]
	private HUDWikiEntryRenderer UIEntryRenderer;

	private List<HUDWikiNavEntry> CurrentNavEntries = new List<HUDWikiNavEntry>();

	private IPlayerWikiManager WikiManager;

	private ResearchManager ResearchManager;

	private IEventSender PassiveEventBus;

	private IEnumerable<MetaWikiEntry> AvailableEntries => WikiManager.GetAvailableWikiEntries(ResearchManager.Progress);

	[Construct]
	private void Construct(ResearchManager researchManager, IPlayerWikiManager wikiManager, IEventSender passiveEventBus)
	{
		AddChildView(UIEntryRenderer);
		WikiManager = wikiManager;
		ResearchManager = researchManager;
		PassiveEventBus = passiveEventBus;
		UIEntryRenderer.OnWikiLinkClicked.AddListener(OnWikiLinkClicked);
		UIMainDialog.gameObject.SetActive(value: false);
		UIMainDialog.CloseRequested.AddListener(UIMainDialog.Hide);
		ResearchManager.Progress.OnChanged.AddListener(BuildNavbar);
		Events.ShowWiki.AddListener(Show);
		UINavParent.RemoveAllChildren();
		BuildNavbar();
	}

	protected override void OnDispose()
	{
		UIEntryRenderer.OnWikiLinkClicked.RemoveListener(OnWikiLinkClicked);
		UIMainDialog.CloseRequested.RemoveListener(UIMainDialog.Hide);
		ResearchManager.Progress.OnChanged.RemoveListener(BuildNavbar);
		Events.ShowWiki.RemoveListener(Show);
	}

	private void ClearNavbar()
	{
		foreach (HUDWikiNavEntry entry in CurrentNavEntries)
		{
			ReleaseChildView(entry);
		}
		CurrentNavEntries.Clear();
	}

	private void BuildNavbar()
	{
		ClearNavbar();
		foreach (MetaWikiEntry entry in AvailableEntries)
		{
			HUDWikiNavEntry handle = RequestChildView(UINavItemPrefab).PlaceAt(UINavParent);
			handle.Entry = entry;
			handle.SelectRequested.AddListener(delegate
			{
				SelectEntry(entry);
			});
			CurrentNavEntries.Add(handle);
		}
		UpdateNavbarActiveState();
	}

	private void UpdateNavbarActiveState()
	{
		foreach (HUDWikiNavEntry handle in CurrentNavEntries)
		{
			handle.Selected = handle.Entry == UIEntryRenderer.Entry;
		}
	}

	private void OnWikiLinkClicked(string wikiEntryId)
	{
		MetaWikiEntry entry = AvailableEntries.FirstOrDefault((MetaWikiEntry metaWikiEntry) => metaWikiEntry.name == wikiEntryId);
		if (entry == null)
		{
			base.Logger.Warning?.Log("Failed to resolve wiki link, not available or unlocked: " + wikiEntryId);
			Globals.UISounds.PlayError();
		}
		else
		{
			Globals.UISounds.PlayClick();
			SelectEntry(entry);
		}
	}

	private void SelectEntry(MetaWikiEntry entry)
	{
		if (!(UIEntryRenderer.Entry == entry))
		{
			UIEntryRenderer.Entry = entry;
			WikiManager.MarkRead(entry);
			UpdateNavbarActiveState();
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (UIMainDialog.Visible)
		{
			if (context.ConsumeWasActivated("main.toggle-wiki"))
			{
				UIMainDialog.Hide();
			}
			UIMainDialog.OnGameUpdate(context);
			base.OnGameUpdate(context, drawOptions);
		}
	}

	private void Show()
	{
		if (UIEntryRenderer.Entry == null)
		{
			SelectEntry(AvailableEntries.FirstOrDefault());
		}
		UIMainDialog.Show();
		PassiveEventBus.Emit(new PlayerOpenedWikiEvent(Player));
	}
}
