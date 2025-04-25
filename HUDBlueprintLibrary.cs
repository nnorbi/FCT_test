using System.Collections.Generic;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public class HUDBlueprintLibrary : HUDPart, IRunnableView, IView
{
	[SerializeField]
	private HUDDialog UIMainDialog;

	[SerializeField]
	private HUDIconButton UIButtonOpenLibraryFolder;

	[SerializeField]
	private HUDIconButton UIButtonRefresh;

	[SerializeField]
	private RectTransform UIContentParent;

	[SerializeField]
	private GameObject UINoEntriesIndicator;

	[SerializeField]
	private PrefabViewReference<HUDBlueprintLibraryEntry> UILibraryEntryPrefab;

	private List<HUDBlueprintLibraryEntry> CurrentEntries = new List<HUDBlueprintLibraryEntry>();

	private IBlueprintLibraryAccess BlueprintLibrary;

	private IHUDDialogStack DialogStack;

	public void Run()
	{
		BlueprintLibrary.Refresh();
	}

	[Construct]
	private void Construct(IBlueprintLibraryAccess blueprintLibrary, IHUDDialogStack dialogStack)
	{
		BlueprintLibrary = blueprintLibrary;
		DialogStack = dialogStack;
		AddChildView(UIButtonOpenLibraryFolder);
		AddChildView(UIButtonRefresh);
		base.gameObject.SetActive(value: true);
		UIMainDialog.gameObject.SetActive(value: false);
		UINoEntriesIndicator.SetActiveSelfExt(active: true);
		UIMainDialog.CloseRequested.AddListener(Hide);
		Events.ShowBlueprintLibrary.AddListener(Show);
		Player.CurrentBlueprint.Changed.AddListener(OnBlueprintSelected);
		Events.RequestAddBlueprintToLibrary.AddListener(OnBlueprintAddToLibraryRequest);
		UIButtonOpenLibraryFolder.Clicked.AddListener(OpenLibraryFolder);
		UIButtonRefresh.Clicked.AddListener(RefreshLibrary);
		BlueprintLibrary.Changed.AddListener(Rerender);
	}

	protected override void OnDispose()
	{
		UIMainDialog.CloseRequested.RemoveListener(Hide);
		Events.ShowBlueprintLibrary.RemoveListener(Show);
		Player.CurrentBlueprint.Changed.RemoveListener(OnBlueprintSelected);
		Events.RequestAddBlueprintToLibrary.RemoveListener(OnBlueprintAddToLibraryRequest);
		UIButtonOpenLibraryFolder.Clicked.RemoveListener(OpenLibraryFolder);
		UIButtonRefresh.Clicked.RemoveListener(RefreshLibrary);
		BlueprintLibrary.Changed.RemoveListener(Rerender);
		ClearEntries();
	}

	private void OnBlueprintAddToLibraryRequest(IBlueprint blueprint)
	{
		HUDDialogSimpleInput dialog = DialogStack.ShowUIDialog<HUDDialogSimpleInput>();
		dialog.InitDialogContents("blueprint-library.add-new.title".tr(), "blueprint-library.add-new.description".tr(), "global.btn-ok".tr(), "blueprint-library.add-new.default-name".tr());
		dialog.OnConfirmed.AddListener(delegate(string text)
		{
			BlueprintLibrary.SaveEntry(new BlueprintLibraryEntry(text, blueprint));
			Events.ShowNotification.Invoke(new HUDNotifications.Notification
			{
				Text = "blueprint-library.notification-entry-saved.text".tr(("<name>", text)),
				Type = HUDNotifications.IconType.Info,
				ShowDuration = 4f,
				Action = Show
			});
		});
	}

	private void RefreshLibrary()
	{
		BlueprintLibrary.Refresh();
	}

	private void OpenLibraryFolder()
	{
		FolderReveal.Reveal(GameEnvironmentManager.BLUEPRINT_LIBRARY_PATH);
	}

	private void OnBlueprintSelected(IBlueprint blueprint)
	{
		if (blueprint != null)
		{
			Hide();
		}
	}

	private void ClearEntries()
	{
		foreach (HUDBlueprintLibraryEntry entry in CurrentEntries)
		{
			ReleaseChildView(entry);
		}
		CurrentEntries.Clear();
	}

	private void Rerender()
	{
		ClearEntries();
		IReadOnlyList<BlueprintLibraryEntry> entries = BlueprintLibrary.GetEntries();
		foreach (BlueprintLibraryEntry entry in entries)
		{
			HUDBlueprintLibraryEntry instance = RequestChildView(UILibraryEntryPrefab).PlaceAt(UIContentParent);
			instance.Entry = entry;
			CurrentEntries.Add(instance);
		}
		UINoEntriesIndicator.SetActiveSelfExt(entries.Count == 0);
	}

	private void Hide()
	{
		UIMainDialog.Hide();
	}

	private void Show()
	{
		UIMainDialog.Show();
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (UIMainDialog.Visible)
		{
			if (context.ConsumeWasActivated("main.toggle-blueprint-library"))
			{
				Hide();
			}
			UIMainDialog.OnGameUpdate(context);
		}
	}
}
