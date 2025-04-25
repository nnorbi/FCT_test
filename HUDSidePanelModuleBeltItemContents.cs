using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HUDSidePanelModuleBeltItemContents : HUDSidePanelModule
{
	protected class UIItemSlot
	{
		public GameObject UIItemParent;

		public BeltItem UICurrentItem;

		public Button UIInfoButton;
	}

	protected UIItemSlot[] UISlots;

	protected Func<List<BeltItem>> ItemGetter;

	public HUDSidePanelModuleBeltItemContents(Func<List<BeltItem>> itemGetter)
	{
		UISlots = new UIItemSlot[4];
		ItemGetter = itemGetter;
		for (int i = 0; i < UISlots.Length; i++)
		{
			UISlots[i] = new UIItemSlot();
		}
	}

	public HUDSidePanelModuleBeltItemContents(IEnumerable<BeltLane> lanes)
		: this(() => lanes.Select((BeltLane lane) => lane.Item).ToList())
	{
	}

	public override void Setup(GameObject container)
	{
		base.Setup(container);
		for (int i = 0; i < UISlots.Length; i++)
		{
			UIItemSlot slot = UISlots[i];
			GameObject slotParent = container.transform.Find("$Slot" + i).gameObject;
			slot.UIItemParent = slotParent.transform.Find("$ItemParent").gameObject;
			slot.UICurrentItem = null;
			slot.UIInfoButton = slotParent.FindButton("$InfoButton");
			HUDTheme.PrepareTheme(slot.UIInfoButton, HUDTheme.ButtonColorsNormal).onClick.AddListener(delegate
			{
				if (slot.UICurrentItem != null && slot.UICurrentItem is ShapeItem)
				{
					Singleton<GameCore>.G.HUD.Events.ShowShapeViewer.Invoke((slot.UICurrentItem as ShapeItem).Definition);
				}
			});
			slot.UIInfoButton.gameObject.SetActive(value: false);
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context)
	{
		List<BeltItem> items = ItemGetter();
		items.RemoveAll((BeltItem item) => item == null);
		for (int i = 0; i < UISlots.Length; i++)
		{
			UIItemSlot slot = UISlots[i];
			BeltItem newItem = ((items.Count > i) ? items[i] : null);
			if (newItem != slot.UICurrentItem)
			{
				slot.UICurrentItem = newItem;
				slot.UIItemParent.transform.RemoveAllChildren();
				if (slot.UICurrentItem == null)
				{
					slot.UIInfoButton.gameObject.SetActive(value: false);
					continue;
				}
				slot.UIInfoButton.gameObject.SetActive(slot.UICurrentItem is ShapeItem);
				HUDBeltItemRenderer.RenderItem(slot.UICurrentItem, slot.UIItemParent.transform, 50f);
			}
		}
	}
}
