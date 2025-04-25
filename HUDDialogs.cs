using System;
using System.Collections.Generic;
using Core.Dependency;
using UnityEngine;

public class HUDDialogs : HUDPart
{
	protected List<HUDDialog> Stack = new List<HUDDialog>();

	[Construct]
	private void Construct()
	{
		Events.ShowDialog.AddListener(ShowDialog);
	}

	protected override void OnDispose()
	{
		Events.ShowDialog.RemoveListener(ShowDialog);
	}

	protected void ShowDialog(GameObject dialog, Action<HUDDialog> callback)
	{
		GameObject instance = UnityEngine.Object.Instantiate(dialog, base.transform);
		HUDDialog uiDialog = instance.GetComponent<HUDDialog>();
		uiDialog.CloseRequested.AddListener(delegate
		{
			uiDialog.Hide(destroyOnComplete: true);
			if (Stack.Contains(uiDialog))
			{
				Stack.Remove(uiDialog);
			}
		});
		uiDialog.Show();
		Stack.Add(uiDialog);
		callback(uiDialog);
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (Stack.Count != 0)
		{
			Stack[Stack.Count - 1].OnGameUpdate(context);
		}
	}
}
