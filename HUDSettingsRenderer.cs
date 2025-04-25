using System;
using System.Collections.Generic;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public class HUDSettingsRenderer : HUDComponent, IHUDContentGroupProvider
{
	private interface IContentGroup : IDisposable
	{
		void Enable();

		bool TryDisable();
	}

	private class ContentGroup<THUDSettingsContentGroup> : IContentGroup, IDisposable where THUDSettingsContentGroup : HUDComponent, IHUDSettingsContentGroup
	{
		private readonly HUDMenuButton Button;

		public readonly PrefabViewReference<THUDSettingsContentGroup> Prefab;

		private readonly IHUDContentGroupProvider ContentGroupProvider;

		private readonly Action<IContentGroup> ChangeToContentGroup;

		private THUDSettingsContentGroup Instance;

		public ContentGroup(HUDMenuButton button, PrefabViewReference<THUDSettingsContentGroup> prefab, IHUDContentGroupProvider contentGroupProvider, Action<IContentGroup> changeToContentGroup)
		{
			Button = button;
			Prefab = prefab;
			ContentGroupProvider = contentGroupProvider;
			ChangeToContentGroup = changeToContentGroup;
			Button.Clicked.AddListener(OnButtonClicked);
		}

		public void Enable()
		{
			if (!(Instance != null))
			{
				Button.SetHighlighted(highlighted: true);
				Instance = ContentGroupProvider.Request(Prefab);
			}
		}

		public bool TryDisable()
		{
			if (Instance == null)
			{
				return true;
			}
			if (!Instance.TryLeave())
			{
				return false;
			}
			Button.SetHighlighted(highlighted: false);
			ContentGroupProvider.Release(Instance);
			Instance = null;
			return true;
		}

		public void Dispose()
		{
			Button.Clicked.RemoveListener(OnButtonClicked);
			if (Instance != null)
			{
				ContentGroupProvider.Release(Instance);
				Instance = null;
			}
		}

		private void OnButtonClicked()
		{
			ChangeToContentGroup(this);
		}
	}

	[SerializeField]
	private PrefabViewReference<HUDGeneralSettingsRenderer> UIGeneralSettingsRendererPrefab;

	[SerializeField]
	private PrefabViewReference<HUDKeybindingsRenderer> UIKeybindingsRendererPrefab;

	[SerializeField]
	private PrefabViewReference<HUDGraphicSettingsRenderer> UIGraphicsRendererPrefab;

	[SerializeField]
	private PrefabViewReference<HUDDevSettingsRenderer> UIDevSettingsRendererPrefab;

	public RectTransform UIContentParent;

	public RectTransform UIContentHeaderParent;

	[SerializeField]
	private PrefabViewReference<HUDMenuButton> UIMenuButtonPrefab;

	private readonly List<IContentGroup> ContentGroups = new List<IContentGroup>();

	private IContentGroup CurrentContentGroup;

	private IHUDDialogStack DialogStack;

	private List<HUDMenuButton> ContentGroupButtons = new List<HUDMenuButton>();

	THUDSettingsContentGroup IHUDContentGroupProvider.Request<THUDSettingsContentGroup>(PrefabViewReference<THUDSettingsContentGroup> prefab)
	{
		return RequestChildView(prefab).PlaceAt(UIContentParent);
	}

	void IHUDContentGroupProvider.Release<THUDSettingsContentGroup>(THUDSettingsContentGroup instance)
	{
		ReleaseChildView(instance);
	}

	public void ChangeToDefaultGroup()
	{
		ChangeToContentGroup(ContentGroups[0]);
	}

	[Construct]
	public void Construct(IHUDDialogStack dialogStack)
	{
		DialogStack = dialogStack;
		CreateContentGroup(Globals.Settings.General.TitleId, UIGeneralSettingsRendererPrefab);
		CreateContentGroup("menu.settings.input", UIKeybindingsRendererPrefab);
		CreateContentGroup(Globals.Settings.Graphics.TitleId, UIGraphicsRendererPrefab);
		CreateContentGroup(Globals.Settings.Dev.TitleId, UIDevSettingsRendererPrefab);
	}

	protected override void OnDispose()
	{
		foreach (IContentGroup contentGroup in ContentGroups)
		{
			contentGroup.Dispose();
		}
		ContentGroups.Clear();
		foreach (HUDMenuButton button in ContentGroupButtons)
		{
			ReleaseChildView(button);
		}
		ContentGroupButtons.Clear();
	}

	private void ChangeToContentGroup(IContentGroup newContentGroup)
	{
		TryChangeToContentGroup(newContentGroup);
	}

	private bool TryChangeToContentGroup(IContentGroup newContentGroup)
	{
		if (newContentGroup == CurrentContentGroup)
		{
			return true;
		}
		if (CurrentContentGroup != null && !CurrentContentGroup.TryDisable())
		{
			return false;
		}
		CurrentContentGroup = newContentGroup;
		CurrentContentGroup?.Enable();
		return true;
	}

	private void CreateContentGroup<THUDSettingsContentGroup>(string translationId, PrefabViewReference<THUDSettingsContentGroup> contentGroupPrefab) where THUDSettingsContentGroup : HUDComponent, IHUDSettingsContentGroup
	{
		HUDMenuButton button = RequestChildView(UIMenuButtonPrefab).PlaceAt(UIContentHeaderParent);
		button.TranslationId = translationId;
		ContentGroupButtons.Add(button);
		ContentGroup<THUDSettingsContentGroup> contentGroup = new ContentGroup<THUDSettingsContentGroup>(button, contentGroupPrefab, this, ChangeToContentGroup);
		ContentGroups.Add(contentGroup);
	}

	public bool TryLeave()
	{
		return TryChangeToContentGroup(null);
	}
}
