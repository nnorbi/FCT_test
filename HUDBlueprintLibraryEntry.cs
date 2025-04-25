#define UNITY_ASSERTIONS
using Core.Dependency;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDBlueprintLibraryEntry : HUDComponent
{
	[SerializeField]
	private HUDPrimaryButtonPanel UIPanel;

	[SerializeField]
	private TMP_Text UITitleText;

	[SerializeField]
	private TMP_Text UICostText;

	[SerializeField]
	private TMP_Text UIBuildingCountText;

	[SerializeField]
	private TMP_Text UIIslandCountText;

	[SerializeField]
	private Sprite UISpriteBuildingBlueprint;

	[SerializeField]
	private Sprite UISpriteIslandBlueprint;

	[SerializeField]
	private HUDIconButton UIButtonRename;

	[SerializeField]
	private HUDIconButton UIButtonDelete;

	[SerializeField]
	private Image UIMainIcon;

	private HUDEvents Events;

	private IBlueprintLibraryAccess BlueprintLibrary;

	private IHUDDialogStack DialogStack;

	private BlueprintLibraryEntry _Entry;

	private PlayerActionManager ActionManager;

	private Player Player;

	public BlueprintLibraryEntry Entry
	{
		set
		{
			if (_Entry != value)
			{
				_Entry = value;
				OnEntryChanged();
			}
		}
	}

	[Construct]
	private void Construct(HUDEvents events, IBlueprintLibraryAccess blueprintLibrary, IHUDDialogStack dialogStack, PlayerActionManager actionManager, Player player)
	{
		BlueprintLibrary = blueprintLibrary;
		DialogStack = dialogStack;
		Events = events;
		ActionManager = actionManager;
		Player = player;
		AddChildView(UIPanel);
		AddChildView(UIButtonDelete);
		AddChildView(UIButtonRename);
		UIPanel.OnClicked.AddListener(StartPlaceBlueprint);
		UIButtonDelete.Clicked.AddListener(RequestDeleteBlueprint);
		UIButtonRename.Clicked.AddListener(RenameEntry);
	}

	private void StartPlaceBlueprint()
	{
		if (_Entry != null)
		{
			ActionSelectBlueprint action = new ActionSelectBlueprint(Player, _Entry.Blueprint);
			if (!ActionManager.TryScheduleAction(action))
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				Globals.UISounds.PlayClick();
			}
		}
	}

	protected override void OnDispose()
	{
		UIPanel.OnClicked.RemoveListener(StartPlaceBlueprint);
		UIButtonDelete.Clicked.RemoveListener(RequestDeleteBlueprint);
		UIButtonRename.Clicked.RemoveListener(RenameEntry);
		_Entry = null;
	}

	private void RequestDeleteBlueprint()
	{
		if (_Entry == null)
		{
			return;
		}
		HUDDialogSimpleConfirm dialog = DialogStack.ShowUIDialog<HUDDialogSimpleConfirm>();
		dialog.InitDialogContents("blueprint-library.delete-entry.title".tr(), "blueprint-library.delete-entry.description".tr(("<name>", _Entry.Title)), "global.btn-delete".tr());
		dialog.OnConfirmed.AddListener(delegate
		{
			if (_Entry != null)
			{
				BlueprintLibrary.RemoveEntry(_Entry);
			}
		});
	}

	private void RenameEntry()
	{
		if (_Entry == null)
		{
			return;
		}
		HUDDialogSimpleInput dialog = Singleton<GameCore>.G.HUD.DialogStack.ShowUIDialog<HUDDialogSimpleInput>();
		dialog.InitDialogContents("blueprint-library.rename-entry.title".tr(), "blueprint-library.add-new.description".tr(), "global.btn-confirm".tr(), _Entry.Title);
		dialog.OnConfirmed.AddListener(delegate(string text)
		{
			if (_Entry != null)
			{
				if (text.Length == 0)
				{
					Globals.UISounds.PlayError();
				}
				else if (!BlueprintLibrary.TryRenameEntry(_Entry, text))
				{
					Globals.UISounds.PlayError();
				}
			}
		});
	}

	private void OnEntryChanged()
	{
		UITitleText.text = _Entry.Title;
		UICostText.text = StringFormatting.FormatBlueprintCurrency(_Entry.Blueprint.Cost);
		UIBuildingCountText.text = StringFormatting.FormatGenericCount(_Entry.Blueprint.BuildingCount);
		if (_Entry.Blueprint is IslandBlueprint islandBlueprint)
		{
			UIIslandCountText.text = StringFormatting.FormatGenericCount(islandBlueprint.Entries.Length);
			UIMainIcon.sprite = UISpriteIslandBlueprint;
		}
		else if (_Entry.Blueprint is BuildingBlueprint)
		{
			UIIslandCountText.text = "-";
			UIMainIcon.sprite = UISpriteBuildingBlueprint;
		}
		else
		{
			Debug.Assert(condition: false, "Invalid blueprint type");
		}
	}
}
