using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDSidePanelModuleGenericButton : HUDSidePanelModule
{
	protected string ButtonLabel;

	protected Action Action;

	public HUDSidePanelModuleGenericButton(string buttonLabel, Action action)
	{
		Action = action;
		ButtonLabel = buttonLabel;
	}

	public override void Setup(GameObject container)
	{
		base.Setup(container);
		Button btn = container.FindButton("$ConfigBtn");
		btn.transform.GetChild(0).GetComponent<TMP_Text>().text = ButtonLabel;
		HUDTheme.PrepareTheme(btn, HUDTheme.ButtonColorsActive).onClick.AddListener(delegate
		{
			Action();
		});
	}
}
