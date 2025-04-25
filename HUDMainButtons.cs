using System;
using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public class HUDMainButtons : HUDPart
{
	public enum ButtonLocation
	{
		Left,
		Middle,
		Right
	}

	[SerializeField]
	private RectTransform UIButtonsParent;

	[SerializeField]
	private GameObject UIDividerPrefab;

	[SerializeField]
	private PrefabViewReference<HUDMainButtonToolbarSlot> UIMainButtonToolbarSlotPrefab;

	protected List<HUDMainButtonConfig> ButtonRequests = new List<HUDMainButtonConfig>();

	protected bool ButtonsInitialized = false;

	[Construct]
	public void Construct()
	{
		UIButtonsParent.RemoveAllChildren();
		Events.RegisterMainButton.AddListener(RegisterMainButton);
	}

	protected override void OnDispose()
	{
		Events.RegisterMainButton.RemoveListener(RegisterMainButton);
	}

	protected void RegisterMainButton(HUDMainButtonConfig config)
	{
		if (ButtonsInitialized)
		{
			throw new Exception("There is no support (yet) for adding buttons dynamically to the toolbar");
		}
		ButtonRequests.Add(config);
	}

	protected void InitButtonInstances()
	{
		if (ButtonsInitialized)
		{
			throw new Exception("There is no support (yet) for adding buttons dynamically to the toolbar");
		}
		ButtonsInitialized = true;
		UIButtonsParent.RemoveAllChildren();
		ButtonRequests.Sort((HUDMainButtonConfig a, HUDMainButtonConfig b) => a.Location - b.Location);
		ButtonLocation? currentLocation = null;
		foreach (HUDMainButtonConfig config in ButtonRequests)
		{
			if (config.Part == null)
			{
				Debug.LogError("Empty provider for main button");
				continue;
			}
			if (currentLocation.HasValue && config.Location != currentLocation.Value)
			{
				UnityEngine.Object.Instantiate(UIDividerPrefab, UIButtonsParent);
			}
			currentLocation = config.Location;
			HUDMainButtonToolbarSlot handle = RequestChildView(UIMainButtonToolbarSlotPrefab).PlaceAt(UIButtonsParent);
			handle.SetConfig(config);
		}
	}

	private void SwitchBetweenButtons(int offset)
	{
		List<HUDMainButtonConfig> enabledButtons = ButtonRequests.Where((HUDMainButtonConfig b) => b.IsVisible() && b.IsEnabled()).ToList();
		int activeButtonIndex = enabledButtons.FindIndex((HUDMainButtonConfig b) => b.IsActive());
		if (activeButtonIndex < 0 || enabledButtons.Count < 2)
		{
			Globals.UISounds.PlayError();
			return;
		}
		int nextIndex = FastMath.SafeMod(activeButtonIndex + offset, enabledButtons.Count);
		enabledButtons[nextIndex].OnActivate?.Invoke();
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (!ButtonsInitialized)
		{
			InitButtonInstances();
		}
		GameObject hudParent = Singleton<GameCore>.G.DrawSceneReferences.HUDRoot.gameObject;
		if (!hudParent.activeSelf && context.ConsumeWasActivated("global.cancel"))
		{
			hudParent.SetActiveSelfExt(active: true);
		}
		if (context.ConsumeWasActivated("main.toggle-ui"))
		{
			hudParent.SetActiveSelfExt(!hudParent.activeSelf);
		}
		if (context.ConsumeWasActivated("toolbar.next-toolbar"))
		{
			SwitchBetweenButtons(1);
		}
		else if (context.ConsumeWasActivated("toolbar.previous-toolbar"))
		{
			SwitchBetweenButtons(-1);
		}
		base.OnGameUpdate(context, drawOptions);
		base.gameObject.SetActiveSelfExt(ButtonRequests.Any((HUDMainButtonConfig b) => b.IsVisible()));
	}
}
